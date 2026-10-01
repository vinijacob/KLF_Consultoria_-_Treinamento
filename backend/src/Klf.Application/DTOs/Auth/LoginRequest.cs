namespace Klf.Application.DTOs.Auth;

/// <summary>Credentials used to sign in to the admin panel.</summary>
/// <param name="Email">Account e-mail.</param>
/// <param name="Password">Account password.</param>
public sealed record LoginRequest(string Email, string Password);
