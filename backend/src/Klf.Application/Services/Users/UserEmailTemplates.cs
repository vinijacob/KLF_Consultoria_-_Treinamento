using System.Net;

using Klf.Application.Interfaces.Email;

namespace Klf.Application.Services.Users;

internal static class UserEmailTemplates
{
    public static EmailMessage Invite(string email, string fullName, string invitedBy, string roleLabel, Uri link, int validForDays)
    {
        var name = WebUtility.HtmlEncode(fullName);
        var inviter = WebUtility.HtmlEncode(invitedBy);
        var href = WebUtility.HtmlEncode(link.AbsoluteUri);

        return new EmailMessage(
            email,
            "Convite para o painel da KLF Consultoria",
            $"<p>Olá, {name}.</p>"
            + $"<p>{inviter} convidou você para o painel da KLF Consultoria, com o perfil <strong>{roleLabel}</strong>.</p>"
            + $"<p>Para começar, escolha sua senha pelo link abaixo. Ele vale por {validForDays} dias e só pode ser usado uma vez.</p>"
            + $"<p><a href=\"{href}\">Aceitar o convite</a></p>"
            + "<p>No primeiro acesso, o painel pede para configurar a verificação em duas etapas com um aplicativo autenticador.</p>",
            $"Olá, {fullName}.\n\n"
            + $"{invitedBy} convidou você para o painel da KLF Consultoria, com o perfil {roleLabel}.\n\n"
            + $"Para começar, escolha sua senha pelo link abaixo. Ele vale por {validForDays} dias e só pode ser usado uma vez.\n\n"
            + $"{link.AbsoluteUri}\n\n"
            + "No primeiro acesso, o painel pede para configurar a verificação em duas etapas com um aplicativo autenticador.");
    }

    public static EmailMessage TwoFactorReset(string email, string fullName)
    {
        var name = WebUtility.HtmlEncode(fullName);

        return new EmailMessage(
            email,
            "Verificação em duas etapas redefinida — KLF Consultoria",
            $"<p>Olá, {name}.</p>"
            + "<p>A administração redefiniu a verificação em duas etapas da sua conta no painel da KLF Consultoria e encerrou as sessões abertas. "
            + "No próximo acesso, configure o aplicativo autenticador de novo.</p>"
            + "<p>Se você não pediu isso, fale com a administração imediatamente.</p>",
            $"Olá, {fullName}.\n\n"
            + "A administração redefiniu a verificação em duas etapas da sua conta no painel da KLF Consultoria e encerrou as sessões abertas. "
            + "No próximo acesso, configure o aplicativo autenticador de novo.\n\n"
            + "Se você não pediu isso, fale com a administração imediatamente.");
    }
}
