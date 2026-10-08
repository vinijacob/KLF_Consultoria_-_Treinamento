using System.Net;

using Klf.Application.Interfaces.Email;

namespace Klf.Application.Services.Auth;

internal static class PasswordEmailTemplates
{
    public static EmailMessage ResetLink(string email, string fullName, Uri link, int validForMinutes)
    {
        var name = WebUtility.HtmlEncode(fullName);
        var href = WebUtility.HtmlEncode(link.AbsoluteUri);

        return new EmailMessage(
            email,
            "Redefinição de senha — KLF Consultoria",
            $"<p>Olá, {name}.</p>"
            + "<p>Recebemos um pedido para redefinir a senha do painel da KLF Consultoria. "
            + $"Clique no link abaixo para escolher uma nova senha. Ele vale por {validForMinutes} minutos e só pode ser usado uma vez.</p>"
            + $"<p><a href=\"{href}\">Redefinir minha senha</a></p>"
            + "<p>Se você não pediu isso, ignore este e-mail: sua senha continua a mesma.</p>",
            $"Olá, {fullName}.\n\n"
            + $"Para redefinir a senha do painel da KLF Consultoria, abra o link abaixo. Ele vale por {validForMinutes} minutos e só pode ser usado uma vez.\n\n"
            + $"{link.AbsoluteUri}\n\n"
            + "Se você não pediu isso, ignore este e-mail: sua senha continua a mesma.");
    }

    public static EmailMessage PasswordChanged(string email, string fullName)
    {
        var name = WebUtility.HtmlEncode(fullName);

        return new EmailMessage(
            email,
            "Sua senha foi alterada — KLF Consultoria",
            $"<p>Olá, {name}.</p>"
            + "<p>A senha do seu acesso ao painel da KLF Consultoria acabou de ser alterada e todas as sessões abertas foram encerradas.</p>"
            + "<p>Se não foi você, entre em contato com a KLF imediatamente.</p>",
            $"Olá, {fullName}.\n\n"
            + "A senha do seu acesso ao painel da KLF Consultoria acabou de ser alterada e todas as sessões abertas foram encerradas.\n\n"
            + "Se não foi você, entre em contato com a KLF imediatamente.");
    }
}
