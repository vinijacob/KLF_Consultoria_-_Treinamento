namespace Klf.Application.DTOs.Auth;

/// <summary>Request to receive a password reset link by e-mail.</summary>
/// <param name="Email">Account e-mail.</param>
public sealed record ForgotPasswordRequest(string Email);

/// <summary>Request to choose a new password with the token received by e-mail.</summary>
/// <param name="Email">Account e-mail, as in the link.</param>
/// <param name="Token">Token from the link (30 minutes, single use).</param>
/// <param name="NewPassword">The new password: at least 10 characters, with upper and lower case letters, a digit and a symbol.</param>
public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);
