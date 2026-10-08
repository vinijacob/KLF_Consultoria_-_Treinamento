namespace Klf.Application.Interfaces.Links;

/// <summary>Builds addresses of frontend pages that go into e-mails.</summary>
public interface IFrontendLinks
{
    /// <summary>Address of the "choose a new password" page, carrying the e-mail and the single-use token.</summary>
    /// <param name="email">Account e-mail.</param>
    /// <param name="token">Password reset token.</param>
    Uri PasswordReset(string email, string token);

    /// <summary>Address of the anonymous feedback page of a session (the one encoded in the QR Code).</summary>
    /// <param name="publicCode">Public code of the session.</param>
    Uri FeedbackForm(string publicCode);
}
