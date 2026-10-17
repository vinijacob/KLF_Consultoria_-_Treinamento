using Klf.Application.DTOs.DataRequests;
using Klf.Application.Interfaces.Identity;
using Klf.Application.Interfaces.Repositories;
using Klf.Application.Services.DataRequests;
using Klf.Application.Tests.Fakes;
using Klf.Application.Validators.DataRequests;
using Klf.Domain.Common;
using Klf.Domain.Entities;
using Klf.Domain.Enums;
using Klf.Domain.Exceptions;

namespace Klf.Application.Tests.Services;

public sealed class DataRequestServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 15, 14, 0, 0, TimeSpan.Zero);

    private readonly InMemoryDataRequestRepository _requests = new();
    private readonly InMemoryContactMessageRepository _messages = new();
    private readonly InMemoryTestimonialRepository _testimonials = new();
    private readonly InMemoryMediaAssetRepository _media = new();
    private readonly FakeFileStorage _files = new();
    private readonly FakePrivateFileStorage _privateFiles = new();
    private readonly InMemoryUserDirectory _users = new();
    private readonly InMemorySiteSettingRepository _settings = new();
    private readonly FakeEmailSender _email = new();
    private readonly FakeAuditTrail _audit = new();
    private readonly MutableTimeProvider _clock = new(Now);
    private readonly DataRequestService _service;

    public DataRequestServiceTests()
    {
        _service = new DataRequestService(
            _requests, _messages, _testimonials, _media, _files, _privateFiles, _users, _settings, _email, new FakeFrontendLinks(), _audit, _settings, _clock);
    }

    [Fact]
    public async Task Submit_records_the_request_with_protocol_and_15_day_deadline_and_notifies_the_data_protection_officer()
    {
        _settings.Add(new SiteSetting(SiteSettingKeys.Legal, """{"dpoEmail":"dpo@klf.test"}"""));
        _settings.Add(new SiteSetting(SiteSettingKeys.Contact, """{"email":"contato@klf.test"}"""));

        var receipt = await SubmitAsync(DataRequestKind.Deletion);

        var saved = Assert.Single(_requests.Requests);
        Assert.Matches("^[A-Z2-9]{4}-[A-Z2-9]{4}$", receipt.Protocol);
        Assert.Equal(new DateOnly(2026, 10, 30), receipt.DueOn);
        Assert.Equal("Pedido recebido pelo site.", Assert.Single(saved.History).Text);
        var message = Assert.Single(_email.Sent);
        Assert.Equal("dpo@klf.test", message.To);
        Assert.Equal("ana@exemplo.com", message.ReplyTo);
        Assert.Contains(receipt.Protocol, message.Subject, StringComparison.Ordinal);
        Assert.Contains("Apagar os dados", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Submit_falls_back_to_the_contact_email_and_discards_robots_silently()
    {
        _settings.Add(new SiteSetting(SiteSettingKeys.Contact, """{"email":"contato@klf.test"}"""));

        await SubmitAsync(DataRequestKind.Access);
        var robot = await _service.SubmitAsync(
            new CreateDataRequestRequest("Robô", "robo@spam.test", DataRequestKind.Access, null, Website: "http://spam"),
            TestContext.Current.CancellationToken);

        Assert.Single(_requests.Requests);
        Assert.Equal("contato@klf.test", Assert.Single(_email.Sent).To);
        Assert.Matches("^[A-Z2-9]{4}-[A-Z2-9]{4}$", robot.Protocol);
    }

    [Fact]
    public void Details_are_required_to_ask_for_a_correction()
    {
        var validator = new CreateDataRequestRequestValidator();

        Assert.False(validator.Validate(new CreateDataRequestRequest("Ana", "ana@exemplo.com", DataRequestKind.Correction, " ")).IsValid);
        Assert.True(validator.Validate(new CreateDataRequestRequest("Ana", "ana@exemplo.com", DataRequestKind.Deletion, null)).IsValid);
    }

    [Fact]
    public async Task Declining_requires_a_reason_and_completing_closes_the_request()
    {
        await SubmitAsync(DataRequestKind.Deletion);
        var id = _requests.Requests[0].Id;

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.UpdateStatusAsync(id, new UpdateDataRequestStatusRequest(DataRequestStatus.Declined, " "), TestContext.Current.CancellationToken));
        var done = await _service.UpdateStatusAsync(id, new UpdateDataRequestStatusRequest(DataRequestStatus.Completed, "Dados apagados."), TestContext.Current.CancellationToken);
        var reopened = await _service.UpdateStatusAsync(id, new UpdateDataRequestStatusRequest(DataRequestStatus.Open, null), TestContext.Current.CancellationToken);

        Assert.Equal(Now.UtcDateTime, done.ClosedAt);
        Assert.Equal(Now.UtcDateTime.AddYears(5), done.DeleteAfter);
        Assert.Null(reopened.ClosedAt);
        Assert.Null(reopened.Resolution);
        Assert.Equal(["Pedido recebido pelo site.", "Pedido concluído.", "Pedido reaberto."], reopened.History.Select(item => item.Text));
    }

    [Fact]
    public async Task Summary_counts_open_and_overdue_requests()
    {
        await SubmitAsync(DataRequestKind.Access);
        _clock.Now = Now.AddDays(16);
        await SubmitAsync(DataRequestKind.Access);

        var summary = await _service.SummaryAsync(TestContext.Current.CancellationToken);
        var list = await _service.ListAsync(new DataRequestListRequest(), TestContext.Current.CancellationToken);

        Assert.Equal(new DataRequestsSummaryResponse(2, 1), summary);
        Assert.True(list.Items[0].IsOverdue);
        Assert.False(list.Items[1].IsOverdue);
    }

    [Fact]
    public async Task Findings_list_messages_by_email_and_testimonials_by_name_ignoring_case_and_accents()
    {
        await SubmitAsync(DataRequestKind.Deletion, name: "Ana Souza");
        _messages.Messages.Add(Message("ANA@exemplo.com"));
        _messages.Messages.Add(Message("outra@exemplo.com"));
        var deleted = new Testimonial("Ána SOUZA", null, "Loja", "ok", null, Now.UtcDateTime, false, false, 0);
        deleted.SoftDelete();
        _testimonials.Testimonials.Add(deleted);
        _testimonials.Testimonials.Add(new Testimonial("Bruno Lima", null, null, "ok", null, Now.UtcDateTime, false, true, 1));
        _users.Users.Add(new ManagedUser(Guid.CreateVersion7(), "ana@exemplo.com", "Ana", "Editor", true, true, false, Now.UtcDateTime));

        var findings = await _service.FindDataAsync(_requests.Requests[0].Id, new DataRequestFindingsRequest(), TestContext.Current.CancellationToken);

        Assert.Single(findings.ContactMessages);
        var testimonial = Assert.Single(findings.Testimonials);
        Assert.True(testimonial.IsDeleted);
        Assert.True(findings.HasPanelAccount);
    }

    [Fact]
    public async Task Erasing_contact_messages_deletes_only_that_email_and_records_it()
    {
        await SubmitAsync(DataRequestKind.Deletion);
        _messages.Messages.Add(Message("ana@exemplo.com"));
        _messages.Messages.Add(Message("Ana@Exemplo.com"));
        _messages.Messages.Add(Message("outra@exemplo.com"));

        var response = await _service.EraseContactMessagesAsync(_requests.Requests[0].Id, TestContext.Current.CancellationToken);

        Assert.Equal("outra@exemplo.com", Assert.Single(_messages.Messages).Email);
        Assert.Equal("Apagadas de vez: 2 mensagens de contato do e-mail do pedido.", response.History[^1].Text);
        var entry = Assert.Single(_audit.Entries);
        Assert.Equal(AuditActions.DataRequestContactErased, entry.Action);
        Assert.Equal("2 mensagens de contato", entry.TargetLabel);
    }

    [Fact]
    public async Task Erasing_a_testimonial_deletes_it_with_its_signed_term_and_unused_photo()
    {
        await SubmitAsync(DataRequestKind.ConsentWithdrawal);
        var photo = new MediaAsset("media/ana.jpg", "ana.jpg", "image/jpeg", 10, null);
        _media.Media.Add(photo);
        _files.Files["media/ana.jpg"] = ([1], "image/jpeg");
        var testimonial = new Testimonial("Ana Souza", null, null, "ok", photo.Id, Now.UtcDateTime, true, true, 0);
        testimonial.AttachConsentDocument("consent/ana.pdf", "termo.pdf", 10, Now.UtcDateTime);
        _privateFiles.Files["consent/ana.pdf"] = [1];
        _testimonials.Testimonials.Add(testimonial);

        var response = await _service.EraseTestimonialAsync(_requests.Requests[0].Id, testimonial.Id, TestContext.Current.CancellationToken);

        Assert.Empty(_testimonials.Testimonials);
        Assert.Empty(_privateFiles.Files);
        Assert.Empty(_media.Media);
        Assert.Empty(_files.Files);
        Assert.Equal("Apagados de vez: depoimento, termo assinado, foto de “Ana Souza”.", response.History[^1].Text);
        Assert.Equal(AuditActions.DataRequestTestimonialErased, Assert.Single(_audit.Entries).Action);
    }

    [Fact]
    public async Task Erasing_a_testimonial_keeps_a_photo_used_elsewhere()
    {
        await SubmitAsync(DataRequestKind.Deletion);
        var photo = new MediaAsset("media/ana.jpg", "ana.jpg", "image/jpeg", 10, null);
        _media.Media.Add(photo);
        _media.Referenced.Add(photo.Id);
        var testimonial = new Testimonial("Ana Souza", null, null, "ok", photo.Id, Now.UtcDateTime, true, false, 0);
        _testimonials.Testimonials.Add(testimonial);

        await _service.EraseTestimonialAsync(_requests.Requests[0].Id, testimonial.Id, TestContext.Current.CancellationToken);

        Assert.Single(_media.Media);
    }

    [Fact]
    public async Task Purge_deletes_requests_closed_more_than_five_years_ago_and_keeps_open_ones()
    {
        await SubmitAsync(DataRequestKind.Access);
        await SubmitAsync(DataRequestKind.Access);
        _requests.Requests[0].ChangeStatus(DataRequestStatus.Completed, null, Now.UtcDateTime);
        _clock.Now = Now.AddYears(5).AddDays(1);

        var deleted = await _service.PurgeExpiredAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, deleted);
        Assert.Equal(DataRequestStatus.Open, Assert.Single(_requests.Requests).Status);
    }

    private static ContactMessage Message(string email) =>
        new("Ana", email, null, null, null, null, "Quero saber dos meus dados.", "2026-10-15", Now.UtcDateTime);

    private Task<DataRequestReceiptResponse> SubmitAsync(DataRequestKind kind, string name = "Ana") =>
        _service.SubmitAsync(new CreateDataRequestRequest(name, " ana@exemplo.com ", kind, null), TestContext.Current.CancellationToken);
}
