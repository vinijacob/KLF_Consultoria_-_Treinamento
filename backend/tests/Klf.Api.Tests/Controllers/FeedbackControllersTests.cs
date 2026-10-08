using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Klf.Api.Tests.Fakes;
using Klf.Application.DTOs.Feedback;
using Klf.Application.Interfaces.Identity;
using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Common;
using Klf.Domain.Enums;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Klf.Api.Tests.Controllers;

public sealed class FeedbackControllersTests : IClassFixture<KlfApiFactory>
{
    private static readonly Uri Forms = new("/api/v1/admin/feedback-forms", UriKind.Relative);
    private static readonly Uri Sessions = new("/api/v1/admin/feedback-sessions", UriKind.Relative);
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private readonly InMemoryFeedbackStore _store = new();
    private readonly WebApplicationFactory<Program> _factory;

    public FeedbackControllersTests(KlfApiFactory factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IFeedbackFormRepository>();
            services.RemoveAll<IFeedbackSessionRepository>();
            services.RemoveAll<IFeedbackResponseRepository>();
            services.RemoveAll<IUnitOfWork>();
            services.AddSingleton<IFeedbackFormRepository>(_store);
            services.AddSingleton<IFeedbackSessionRepository>(_store);
            services.AddSingleton<IFeedbackResponseRepository>(_store);
            services.AddSingleton<IUnitOfWork>(_store);
        }));
    }

    [Fact]
    public async Task Full_flow_builds_form_opens_session_receives_anonymous_answers_and_shows_results()
    {
        var token = TestContext.Current.CancellationToken;
        using var admin = CreateClientAs(Roles.Admin, out _);

        var form = await (await admin.PostAsJsonAsync(Forms, new CreateFeedbackFormRequest("Padrão", null, Definition()), Json, token))
            .Content.ReadFromJsonAsync<FeedbackFormResponse>(Json, token);
        var create = await admin.PostAsJsonAsync(Sessions, SessionRequest(form!.Id), Json, token);
        var session = await create.Content.ReadFromJsonAsync<FeedbackSessionResponse>(Json, token);
        var publicUri = new Uri($"/api/v1/public/feedback/{session!.PublicCode}", UriKind.Relative);

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        Assert.Equal(FeedbackSessionStatus.Open, session.Status);

        using var respondent = CreateHttpsClient();
        var page = await respondent.GetFromJsonAsync<PublicFeedbackFormResponse>(publicUri, Json, token);
        var submit = await respondent.PostAsJsonAsync($"{publicUri}/responses", Answers(10), Json, token);
        var cookie = Assert.Single(submit.Headers.GetValues("Set-Cookie"));
        var pageAfter = await respondent.GetFromJsonAsync<PublicFeedbackFormResponse>(publicUri, Json, token);
        var again = await respondent.PostAsJsonAsync($"{publicUri}/responses", Answers(10), Json, token);

        Assert.NotNull(page!.Definition);
        Assert.Equal(HttpStatusCode.NoContent, submit.StatusCode);
        Assert.StartsWith($"klf_fb_{session.PublicCode}=", cookie, StringComparison.Ordinal);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($"path=/api/v1/public/feedback/{session.PublicCode}", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.True(pageAfter!.AlreadyAnswered);
        Assert.Null(pageAfter.Definition);
        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);

        foreach (var nps in new[] { 9, 3 })
        {
            using var other = CreateHttpsClient();
            Assert.Equal(HttpStatusCode.NoContent, (await other.PostAsJsonAsync($"{publicUri}/responses", Answers(nps), Json, token)).StatusCode);
        }

        var results = await admin.GetFromJsonAsync<FeedbackSessionResultsResponse>(new Uri($"{Sessions}/{session.Id}/results", UriKind.Relative), Json, token);
        Assert.True(results!.HasEnoughResponses);
        Assert.Equal(3, results.ResponseCount);
        Assert.Equal(33, results.Nps!.Score);
        Assert.All(_store.Responses, r => Assert.Equal(4, r.Id.Version));
    }

    [Fact]
    public async Task Forged_cookie_is_ignored_and_respondent_can_answer()
    {
        var session = _store.SeedSession(Guid.CreateVersion7(), DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1));
        using var respondent = CreateHttpsClient();
        respondent.DefaultRequestHeaders.Add("Cookie", $"klf_fb_{session.PublicCode}=forjado");

        var response = await respondent.PostAsJsonAsync($"/api/v1/public/feedback/{session.PublicCode}/responses", Answers(9), Json, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Public_returns_404_for_unknown_code_and_400_in_portuguese_for_bad_answers()
    {
        var session = _store.SeedSession(Guid.CreateVersion7(), DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1));
        using var client = CreateHttpsClient();
        var token = TestContext.Current.CancellationToken;

        var unknown = await client.GetAsync(new Uri("/api/v1/public/feedback/AAAAAAAAAAAAAAAAAAAAAA", UriKind.Relative), token);
        var bad = await client.PostAsJsonAsync(
            $"/api/v1/public/feedback/{session.PublicCode}/responses",
            new SubmitFeedbackRequest([new FeedbackAnswerDto("nota", null, 9, null)]),
            Json,
            token);
        var problem = await bad.Content.ReadFromJsonAsync<ValidationProblemDetails>(token);

        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
        Assert.Equal(["Escolha um valor de 1 a 5."], problem!.Errors["Answers.nota"]);
        Assert.Equal(["Responda esta pergunta."], problem.Errors["Answers.bom"]);
        Assert.Empty(_store.Responses);
    }

    [Fact]
    public async Task Admin_routes_return_401_anonymous_403_editor_and_404_to_instructor_for_others_sessions()
    {
        var session = _store.SeedSession(Guid.CreateVersion7(), DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1));
        using var anonymous = _factory.CreateClient();
        using var editor = CreateClientAs(Roles.Editor, out _);
        using var instructor = CreateClientAs(Roles.Instructor, out _);
        var uri = new Uri($"{Sessions}/{session.Id}", UriKind.Relative);
        var token = TestContext.Current.CancellationToken;

        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(Sessions, token)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await editor.GetAsync(Sessions, token)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await instructor.GetAsync(uri, token)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await instructor.GetAsync(new Uri($"{uri}/results", UriKind.Relative), token)).StatusCode);
    }

    [Fact]
    public async Task Instructor_creates_own_session_and_sees_it()
    {
        var token = TestContext.Current.CancellationToken;
        using var instructor = CreateClientAs(Roles.Instructor, out var instructorId);
        var form = new Klf.Domain.Entities.FeedbackForm("Padrão", null, FeedbackSamples.Definition());
        _store.Forms.Add(form);

        var created = await (await instructor.PostAsJsonAsync(Sessions, SessionRequest(form.Id), Json, token))
            .Content.ReadFromJsonAsync<FeedbackSessionResponse>(Json, token);
        var get = await instructor.GetAsync(new Uri($"{Sessions}/{created!.Id}", UriKind.Relative), token);

        Assert.Equal(instructorId, created.OwnerId);
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
    }

    [Fact]
    public async Task Form_with_broken_structure_returns_400_with_field_path()
    {
        using var admin = CreateClientAs(Roles.Admin, out _);
        var broken = new FormDefinitionDto([new FormSectionDto("s", "Seção", null, FeedbackTopic.Training,
            [new FormQuestionDto("q", FeedbackQuestionType.MultipleChoice, "Qual?", null, true, [], null, null, null, null, null)])]);

        var response = await admin.PostAsJsonAsync(Forms, new CreateFeedbackFormRequest("Quebrado", null, broken), Json, TestContext.Current.CancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Definition.Sections[0].Questions[0].Options", problem!.Errors.Keys);
    }

    [Fact]
    public async Task Qr_code_and_poster_are_real_png_and_pdf_files()
    {
        var session = _store.SeedSession(Guid.CreateVersion7(), DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1));
        using var admin = CreateClientAs(Roles.Admin, out _);
        var token = TestContext.Current.CancellationToken;

        var qr = await admin.GetAsync(new Uri($"{Sessions}/{session.Id}/qrcode", UriKind.Relative), token);
        var poster = await admin.GetAsync(new Uri($"{Sessions}/{session.Id}/poster", UriKind.Relative), token);
        var png = await qr.Content.ReadAsByteArrayAsync(token);
        var pdf = await poster.Content.ReadAsByteArrayAsync(token);

        Assert.Equal("image/png", qr.Content.Headers.ContentType?.MediaType);
        Assert.Equal([0x89, 0x50, 0x4E, 0x47], png[..4]);
        Assert.Equal("application/pdf", poster.Content.Headers.ContentType?.MediaType);
        Assert.Equal("%PDF"u8.ToArray(), pdf[..4]);
        Assert.Contains(session.PublicCode, poster.Content.Headers.ContentDisposition?.FileName, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Replace_form_returns_409_after_first_response_and_close_blocks_new_answers()
    {
        var session = _store.SeedSession(Guid.CreateVersion7(), DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1));
        _store.SeedResponses(session, 10);
        using var admin = CreateClientAs(Roles.Admin, out _);
        using var respondent = CreateHttpsClient();
        var token = TestContext.Current.CancellationToken;

        var replace = await admin.PutAsJsonAsync(
            new Uri($"{Sessions}/{session.Id}/form", UriKind.Relative), new ReplaceSessionFormRequest("Novo", null, Definition()), Json, token);
        var close = await admin.PostAsync(new Uri($"{Sessions}/{session.Id}/close", UriKind.Relative), null, token);
        var late = await respondent.PostAsJsonAsync($"/api/v1/public/feedback/{session.PublicCode}/responses", Answers(9), Json, token);

        Assert.Equal(HttpStatusCode.Conflict, replace.StatusCode);
        Assert.Equal(HttpStatusCode.OK, close.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, late.StatusCode);
    }

    private static CreateFeedbackSessionRequest SessionRequest(Guid formId) =>
        new(formId, "Loja Centro — manhã", DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow.AddHours(3), null, null, null);

    private static SubmitFeedbackRequest Answers(int nps) => new(
    [
        new FeedbackAnswerDto("bom", "Equipe unida", null, null),
        new FeedbackAnswerDto("nota", null, 4, null),
        new FeedbackAnswerDto("nps", null, nps, null),
    ]);

    private static FormDefinitionDto Definition() => new(
    [
        new FormSectionDto("empresa", "Sobre a sua empresa", null, FeedbackTopic.Company,
            [new FormQuestionDto("bom", FeedbackQuestionType.LongText, "O que sua empresa faz bem?", null, true, null, null, null, null, null, null)]),
        new FormSectionDto("klf", "Sobre o treinamento", null, FeedbackTopic.Training,
        [
            new FormQuestionDto("nota", FeedbackQuestionType.Scale, "O que achou?", null, true, null, 1, 5, "Ruim", "Excelente", null),
            new FormQuestionDto("nps", FeedbackQuestionType.Nps, "Recomendaria a KLF?", null, true, null, null, null, null, null, null),
        ]),
    ]);

    private HttpClient CreateHttpsClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

    private HttpClient CreateClientAs(string role, out Guid userId)
    {
        userId = Guid.CreateVersion7();
        var token = _factory.Services.GetRequiredService<ITokenService>()
            .Generate(new UserAccount(userId, $"{role}@klf.test", role, [role]));
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        return client;
    }
}
