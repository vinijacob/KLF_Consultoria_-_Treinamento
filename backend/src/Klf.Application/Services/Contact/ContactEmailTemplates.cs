using System.Net;

using Klf.Application.Interfaces.Email;
using Klf.Domain.Common;
using Klf.Domain.Entities;

namespace Klf.Application.Services.Contact;

internal static class ContactEmailTemplates
{
    public static EmailMessage NewMessage(string to, ContactMessage message, Uri panelLink)
    {
        var received = AppTimeZone.ToLocal(message.ReceivedAt).ToString("dd/MM/yyyy 'às' HH:mm", System.Globalization.CultureInfo.InvariantCulture);
        var details = new List<(string Label, string Value)>
        {
            ("Nome", message.Name),
            ("E-mail", message.Email),
        };

        if (message.Phone is not null)
        {
            details.Add(("Telefone", message.Phone));
        }

        if (message.Company is not null)
        {
            details.Add(("Empresa", message.Company));
        }

        if (message.ServiceTitle is not null)
        {
            details.Add(("Interesse", message.ServiceTitle));
        }

        details.Add(("Recebida em", $"{received} (horário de Manaus)"));

        var html = string.Concat(details.Select(item => $"<p><strong>{item.Label}:</strong> {WebUtility.HtmlEncode(item.Value)}</p>"))
            + $"<p><strong>Mensagem:</strong></p><p>{WebUtility.HtmlEncode(message.Text).ReplaceLineEndings("<br>")}</p>"
            + $"<p><a href=\"{WebUtility.HtmlEncode(panelLink.AbsoluteUri)}\">Abrir no painel</a> · Para responder, use “Responder” neste e-mail.</p>";

        var text = string.Join('\n', details.Select(item => $"{item.Label}: {item.Value}"))
            + $"\n\nMensagem:\n{message.Text}\n\nAbrir no painel: {panelLink.AbsoluteUri}\nPara responder, use “Responder” neste e-mail.";

        return new EmailMessage(to, $"Nova mensagem pelo site — {SingleLine(message.Name)}", html, text, ReplyTo: message.Email);
    }

    private static string SingleLine(string text) => string.Join(' ', text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries));
}
