using Ganss.Xss;

using Klf.Application.Interfaces.Content;

namespace Klf.Infrastructure.Services;

internal sealed class HtmlContentSanitizer : IHtmlContentSanitizer
{
    public string Sanitize(string html) => new HtmlSanitizer().Sanitize(html);
}
