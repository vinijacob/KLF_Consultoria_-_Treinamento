using System.Globalization;
using System.Net;

using Klf.Application.Interfaces.Email;

namespace Klf.Application.Services.Auth;

internal static class SecurityEmailTemplates
{
    private static readonly CultureInfo Brazil = CultureInfo.GetCultureInfo("pt-BR");

    public static EmailMessage NewDeviceSignIn(string email, string fullName, string device, DateTime localTime, Uri changePasswordLink)
    {
        var name = WebUtility.HtmlEncode(fullName);
        var deviceHtml = WebUtility.HtmlEncode(device);
        var href = WebUtility.HtmlEncode(changePasswordLink.AbsoluteUri);
        var when = localTime.ToString("dd/MM/yyyy 'às' HH:mm", Brazil);

        return new EmailMessage(
            email,
            "Novo acesso ao painel — KLF Consultoria",
            $"<p>Olá, {name}.</p>"
            + "<p>Sua conta do painel da KLF Consultoria acabou de ser acessada de um aparelho que ainda não conhecíamos: "
            + $"<strong>{deviceHtml}</strong>, em {when} (horário de Manaus).</p>"
            + "<p>Se foi você, não precisa fazer nada.</p>"
            + "<p>Se não foi você, troque a senha agora pelo link abaixo: isso encerra todas as sessões abertas. Depois, avise a administração da KLF.</p>"
            + $"<p><a href=\"{href}\">Trocar minha senha</a></p>",
            $"Olá, {fullName}.\n\n"
            + $"Sua conta do painel da KLF Consultoria acabou de ser acessada de um aparelho que ainda não conhecíamos: {device}, em {when} (horário de Manaus).\n\n"
            + "Se foi você, não precisa fazer nada.\n\n"
            + "Se não foi você, troque a senha agora pelo link abaixo: isso encerra todas as sessões abertas. Depois, avise a administração da KLF.\n\n"
            + changePasswordLink.AbsoluteUri);
    }
}
