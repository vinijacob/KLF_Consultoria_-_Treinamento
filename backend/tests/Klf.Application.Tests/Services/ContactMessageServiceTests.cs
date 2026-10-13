using Klf.Application.DTOs.Contact;
using Klf.Application.Services.Contact;
using Klf.Application.Tests.Fakes;
using Klf.Domain.Common;
using Klf.Domain.Entities;
using Klf.Domain.Enums;
using Klf.Domain.Exceptions;

namespace Klf.Application.Tests.Services;

public sealed class ContactMessageServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 12, 19, 30, 0, TimeSpan.Zero);

    private readonly InMemoryContactMessageRepository _messages = new();
    private readonly InMemoryServiceRepository _services = new();
    private readonly InMemorySiteSettingRepository _settings = new();
    private readonly FakeEmailSender _email = new();
    private readonly MutableTimeProvider _clock = new(Now);
    private readonly ContactMessageService _service;

    public ContactMessageServiceTests()
    {
        _service = new ContactMessageService(_messages, _services, _settings, _email, new FakeFrontendLinks(), _messages, _clock);
    }

    private static CreateContactMessageRequest Request(Guid? serviceId = null, string? website = null) => new(
        "  Ana Souza ",
        "ana@loja.com.br",
        "(92) 99999-0000",
        "  ",
        serviceId,
        "  Quero levar o treinamento para a minha equipe.  ",
        true,
        "2026-10-12",
        website);

    private void SetContactEmail(string email) =>
        _settings.Settings.Add(new SiteSetting(SiteSettingKeys.Contact, $$"""{"email":"{{email}}"}"""));

    [Fact]
    public async Task Submit_saves_trimmed_message_and_notifies_the_contact_email_with_reply_to_the_sender()
    {
        SetContactEmail("contato@klf.com.br");

        await _service.SubmitAsync(Request(), TestContext.Current.CancellationToken);

        var saved = Assert.Single(_messages.Messages);
        Assert.Equal("Ana Souza", saved.Name);
        Assert.Null(saved.Company);
        Assert.Equal("Quero levar o treinamento para a minha equipe.", saved.Text);
        Assert.Equal(Now.UtcDateTime, saved.ReceivedAt);
        Assert.Equal("2026-10-12", saved.PrivacyPolicyVersion);
        var email = Assert.Single(_email.Sent);
        Assert.Equal("contato@klf.com.br", email.To);
        Assert.Equal("ana@loja.com.br", email.ReplyTo);
        Assert.Contains($"/painel/mensagens/{saved.Id}", email.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Submit_saves_without_e_mail_when_the_site_has_no_contact_email()
    {
        await _service.SubmitAsync(Request(), TestContext.Current.CancellationToken);

        Assert.Single(_messages.Messages);
        Assert.Empty(_email.Sent);
    }

    [Fact]
    public async Task Submit_html_encodes_what_the_visitor_wrote_in_the_notification()
    {
        SetContactEmail("contato@klf.com.br");
        var request = Request() with { Name = "<script>alert(1)</script>", Message = "<b>oi</b> tudo bem? quero proposta" };

        await _service.SubmitAsync(request, TestContext.Current.CancellationToken);

        var html = Assert.Single(_email.Sent).HtmlBody;
        Assert.DoesNotContain("<script>", html, StringComparison.Ordinal);
        Assert.Contains("&lt;b&gt;oi&lt;/b&gt;", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Submit_drops_the_message_silently_when_the_bot_trap_is_filled()
    {
        SetContactEmail("contato@klf.com.br");

        await _service.SubmitAsync(Request(website: "https://spam.example"), TestContext.Current.CancellationToken);

        Assert.Empty(_messages.Messages);
        Assert.Empty(_email.Sent);
    }

    [Fact]
    public async Task Submit_keeps_the_title_of_the_chosen_service_and_rejects_an_inactive_one()
    {
        var active = new Service("Vendas consultivas", "vendas", null, "<p>x</p>", "Todos", 8, ServiceFormat.InCompany, null, 0, true);
        var hidden = new Service("Antigo", "antigo", null, "<p>x</p>", "Todos", 8, ServiceFormat.InCompany, null, 1, false);
        _services.Services.AddRange([active, hidden]);

        await _service.SubmitAsync(Request(active.Id), TestContext.Current.CancellationToken);
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.SubmitAsync(Request(hidden.Id), TestContext.Current.CancellationToken));

        Assert.Equal("Vendas consultivas", Assert.Single(_messages.Messages).ServiceTitle);
        Assert.Contains("ServiceId", exception.Errors.Keys);
    }

    [Fact]
    public async Task Status_changes_and_summary_counts_only_unread_messages()
    {
        await _service.SubmitAsync(Request(), TestContext.Current.CancellationToken);
        await _service.SubmitAsync(Request(), TestContext.Current.CancellationToken);
        var first = _messages.Messages[0];

        var answered = await _service.UpdateStatusAsync(first.Id, new UpdateContactMessageStatusRequest(ContactMessageStatus.Answered), TestContext.Current.CancellationToken);
        var summary = await _service.SummaryAsync(TestContext.Current.CancellationToken);

        Assert.Equal(ContactMessageStatus.Answered, answered.Status);
        Assert.Equal(Now.UtcDateTime.AddYears(2), answered.DeleteAfter);
        Assert.Equal(1, summary.NewCount);
    }

    [Fact]
    public async Task Delete_removes_the_message_for_good_and_unknown_id_is_not_found()
    {
        await _service.SubmitAsync(Request(), TestContext.Current.CancellationToken);
        var id = _messages.Messages[0].Id;

        await _service.DeleteAsync(id, TestContext.Current.CancellationToken);

        Assert.Empty(_messages.Messages);
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(id, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Purge_deletes_only_messages_whose_last_contact_is_older_than_two_years()
    {
        await _service.SubmitAsync(Request(), TestContext.Current.CancellationToken);
        await _service.SubmitAsync(Request() with { Name = "Respondida" }, TestContext.Current.CancellationToken);
        var answeredLater = _messages.Messages[1];
        _clock.Now = Now.AddYears(1);
        answeredLater.ChangeStatus(ContactMessageStatus.Answered, _clock.GetUtcNow().UtcDateTime);
        _clock.Now = Now.AddYears(2).AddDays(1);

        var deleted = await _service.PurgeExpiredAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, deleted);
        Assert.Equal("Respondida", Assert.Single(_messages.Messages).Name);
    }

    [Fact]
    public async Task List_filters_by_status_and_search_newest_first()
    {
        await _service.SubmitAsync(Request() with { Name = "Bruno" }, TestContext.Current.CancellationToken);
        _clock.Now = Now.AddMinutes(5);
        await _service.SubmitAsync(Request() with { Name = "Carla", Company = "Ótica Carla" }, TestContext.Current.CancellationToken);

        var all = await _service.ListAsync(new ContactMessageListRequest(), TestContext.Current.CancellationToken);
        var search = await _service.ListAsync(new ContactMessageListRequest { Search = "ótica" }, TestContext.Current.CancellationToken);

        Assert.Equal(["Carla", "Bruno"], all.Items.Select(x => x.Name));
        Assert.Equal("Carla", Assert.Single(search.Items).Name);
    }
}
