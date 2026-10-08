using Klf.Application.Interfaces.Links;

using Microsoft.Extensions.Options;

namespace Klf.Infrastructure.Email;

internal sealed class FrontendLinks(IOptions<FrontendOptions> options) : IFrontendLinks
{
    public Uri PasswordReset(string email, string token) =>
        new($"{options.Value.BaseUrl.TrimEnd('/')}/painel/redefinir-senha?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}");

    public Uri FeedbackForm(string publicCode) =>
        new($"{options.Value.BaseUrl.TrimEnd('/')}/avaliar/{Uri.EscapeDataString(publicCode)}");
}
