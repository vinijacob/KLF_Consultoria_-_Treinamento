using Klf.Application.Interfaces.Email;
using Klf.Application.Interfaces.Links;

namespace Klf.Application.Tests.Fakes;

internal sealed class FakeEmailSender : IEmailSender
{
    public List<EmailMessage> Sent { get; } = [];

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        Sent.Add(message);
        return Task.CompletedTask;
    }
}

internal sealed class FakeFrontendLinks : IFrontendLinks
{
    public Uri PasswordReset(string email, string token) =>
        new($"https://klf.test/painel/redefinir-senha?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}");

    public Uri FeedbackForm(string publicCode) => new($"https://klf.test/avaliar/{publicCode}");
}
