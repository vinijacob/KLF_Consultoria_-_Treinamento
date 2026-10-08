using Klf.Api.Extensions;
using Klf.Application.DTOs.Auth;
using Klf.Application.Interfaces.Identity;
using Klf.Application.Services.Auth;
using Klf.Domain.Exceptions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Klf.Api.Controllers.Auth;

/// <summary>Sign-in and session management for the admin panel.</summary>
[Route($"{RoutePrefix}/auth")]
[Tags("Auth")]
public sealed class AuthController(IAuthService authService) : ApiControllerBase
{
    /// <summary>Signs in with e-mail and password.</summary>
    /// <remarks>
    /// Returns a short-lived access token in the body and sets the refresh token in the <c>klf_refresh</c> HttpOnly cookie.
    /// After 5 wrong passwords the account is locked for 15 minutes. Limited to 10 requests per minute per IP.
    /// </remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthenticationExtensions.LoginRateLimitPolicy)]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);

        return result.Session is { } session
            ? StartSession(session)
            : Ok(new LoginResponse(
                null,
                null,
                TwoFactorRequired: !result.Challenge!.SetupRequired,
                TwoFactorSetupRequired: result.Challenge.SetupRequired,
                TwoFactorToken: result.Challenge.Token));
    }

    /// <summary>Second step of the sign-in: sends the 6-digit code of the authenticator app.</summary>
    /// <remarks>
    /// Use the <c>twoFactorToken</c> returned by the login when <c>twoFactorRequired</c> is true. On success, behaves like a login:
    /// returns the access token and sets the refresh token cookie. Wrong codes count toward the account lockout.
    /// </remarks>
    [HttpPost("2fa/verify")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthenticationExtensions.LoginRateLimitPolicy)]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LoginResponse>> VerifyTwoFactorAsync(TwoFactorCodeRequest request, CancellationToken cancellationToken) =>
        StartSession(await authService.VerifyTwoFactorAsync(request, cancellationToken));

    /// <summary>Second step of the sign-in with a one-time recovery code, for who lost the authenticator app.</summary>
    /// <remarks>Each recovery code works once. Behaves like <c>2fa/verify</c> on success.</remarks>
    [HttpPost("2fa/recover")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthenticationExtensions.LoginRateLimitPolicy)]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LoginResponse>> RecoverWithCodeAsync(TwoFactorRecoveryRequest request, CancellationToken cancellationToken) =>
        StartSession(await authService.RecoverWithCodeAsync(request, cancellationToken));

    /// <summary>Starts registering the authenticator app (first sign-in, when <c>twoFactorSetupRequired</c> is true).</summary>
    /// <remarks>
    /// Returns the secret key and the <c>otpauth://</c> address to render as a QR Code. Each call creates a new key
    /// (the previous one stops working). After scanning, confirm with <c>2fa/enable</c>.
    /// </remarks>
    [HttpPost("2fa/setup")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthenticationExtensions.LoginRateLimitPolicy)]
    [ProducesResponseType<TwoFactorSetupResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TwoFactorSetupResponse>> StartTwoFactorSetupAsync(TwoFactorSetupRequest request, CancellationToken cancellationToken)
    {
        var setup = await authService.StartTwoFactorSetupAsync(request, cancellationToken);

        return Ok(new TwoFactorSetupResponse(setup.SharedKey, setup.OtpAuthUri));
    }

    /// <summary>Finishes the enrollment: confirms the first code, turns two-factor on and starts the session.</summary>
    /// <remarks>
    /// The response carries the ten recovery codes, shown only this once. Sets the refresh token cookie.
    /// </remarks>
    [HttpPost("2fa/enable")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthenticationExtensions.LoginRateLimitPolicy)]
    [ProducesResponseType<TwoFactorEnabledResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TwoFactorEnabledResponse>> EnableTwoFactorAsync(TwoFactorCodeRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.EnableTwoFactorAsync(request, cancellationToken);
        RefreshTokenCookie.Append(Response, result.Session.RefreshToken, result.Session.SessionExpiresAt);

        return Ok(new TwoFactorEnabledResponse(result.Session.AccessToken, result.Session.AccessTokenExpiresAt, result.RecoveryCodes));
    }

    /// <summary>Replaces the recovery codes of the signed-in user.</summary>
    /// <remarks>Requires the current code of the authenticator app. The previous recovery codes stop working.</remarks>
    [HttpPost("2fa/recovery-codes")]
    [Authorize]
    [EnableRateLimiting(AuthenticationExtensions.LoginRateLimitPolicy)]
    [ProducesResponseType<RecoveryCodesResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<RecoveryCodesResponse>> RegenerateRecoveryCodesAsync(RegenerateRecoveryCodesRequest request, CancellationToken cancellationToken) =>
        Ok(new RecoveryCodesResponse(await authService.RegenerateRecoveryCodesAsync(User.GetUserId(), request, cancellationToken)));

    /// <summary>Exchanges the refresh token cookie for a new access token.</summary>
    /// <remarks>
    /// The browser sends the <c>klf_refresh</c> cookie automatically (use <c>credentials: "include"</c>).
    /// The cookie is replaced on every call; reusing an old one ends every session of the user.
    /// </remarks>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthenticationExtensions.LoginRateLimitPolicy)]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LoginResponse>> RefreshAsync(CancellationToken cancellationToken)
    {
        var refreshToken = RefreshTokenCookie.Read(Request)
            ?? throw new UnauthorizedException("Sua sessão expirou. Entre novamente.");

        return StartSession(await authService.RefreshAsync(refreshToken, cancellationToken));
    }

    /// <summary>Signs out: ends the session and deletes the refresh token cookie.</summary>
    /// <remarks>
    /// Always returns 204, even without a valid cookie, so it is safe to call twice. The access token already issued
    /// keeps working until it expires (at most 15 minutes); the frontend must discard it.
    /// </remarks>
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken)
    {
        var refreshToken = RefreshTokenCookie.Read(Request);

        if (refreshToken is not null)
        {
            await authService.LogoutAsync(refreshToken, cancellationToken);
        }

        RefreshTokenCookie.Delete(Response);

        return NoContent();
    }

    /// <summary>Asks for a password reset link by e-mail.</summary>
    /// <remarks>
    /// Always answers 202, whether the e-mail has an account or not, so the endpoint does not reveal which e-mails are registered.
    /// The link is valid for 30 minutes and works once. Limited to 10 requests per minute per IP.
    /// </remarks>
    [HttpPost("forgot")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthenticationExtensions.LoginRateLimitPolicy)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await authService.ForgotPasswordAsync(request, cancellationToken);

        return Accepted();
    }

    /// <summary>Chooses a new password with the token from the e-mail link.</summary>
    /// <remarks>
    /// Ends every session of the user, clears the account lockout and sends a "password changed" e-mail.
    /// Two-factor authentication is still required on the next sign-in.
    /// </remarks>
    [HttpPost("reset")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthenticationExtensions.LoginRateLimitPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await authService.ResetPasswordAsync(request, cancellationToken);

        return NoContent();
    }

    /// <summary>Returns the signed-in user.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CurrentUserResponse>> GetCurrentUserAsync(CancellationToken cancellationToken) =>
        Ok(await authService.GetCurrentUserAsync(User.GetUserId(), cancellationToken));

    private ActionResult<LoginResponse> StartSession(AuthSession session)
    {
        RefreshTokenCookie.Append(Response, session.RefreshToken, session.SessionExpiresAt);

        return Ok(new LoginResponse(session.AccessToken, session.AccessTokenExpiresAt));
    }
}
