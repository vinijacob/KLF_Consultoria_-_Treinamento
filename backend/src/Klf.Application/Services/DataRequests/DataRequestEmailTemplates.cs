using System.Globalization;
using System.Net;

using Klf.Application.Interfaces.Email;
using Klf.Domain.Common;
using Klf.Domain.Entities;

namespace Klf.Application.Services.DataRequests;

internal static class DataRequestEmailTemplates
{
    public static EmailMessage NewRequest(string to, DataRequest request, string kindLabel, Uri panelLink)
    {
        var received = AppTimeZone.ToLocal(request.ReceivedAt).ToString("dd/MM/yyyy 'às' HH:mm", CultureInfo.InvariantCulture);
        var due = request.DueOn.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        var details = new List<(string Label, string Value)>
        {
            ("Protocolo", request.Protocol),
            ("Pedido", kindLabel),
            ("Nome", request.Name),
            ("E-mail", request.Email),
            ("Recebido em", $"{received} (horário de Manaus)"),
            ("Responder até", $"{due} (15 dias, LGPD art. 19)"),
        };

        var html = string.Concat(details.Select(item => $"<p><strong>{item.Label}:</strong> {WebUtility.HtmlEncode(item.Value)}</p>"))
            + (request.Details is null ? string.Empty : $"<p><strong>Detalhes:</strong></p><p>{WebUtility.HtmlEncode(request.Details).ReplaceLineEndings("<br>")}</p>")
            + "<p>Antes de entregar ou apagar dados, confirme que o pedido é mesmo da pessoa: responda para o e-mail informado.</p>"
            + $"<p><a href=\"{WebUtility.HtmlEncode(panelLink.AbsoluteUri)}\">Abrir no painel</a></p>";

        var text = string.Join('\n', details.Select(item => $"{item.Label}: {item.Value}"))
            + (request.Details is null ? string.Empty : $"\n\nDetalhes:\n{request.Details}")
            + "\n\nAntes de entregar ou apagar dados, confirme que o pedido é mesmo da pessoa: responda para o e-mail informado."
            + $"\n\nAbrir no painel: {panelLink.AbsoluteUri}";

        return new EmailMessage(to, $"Pedido sobre dados pessoais (LGPD) — protocolo {request.Protocol}", html, text, ReplyTo: request.Email);
    }
}
