using Klf.Api.Extensions;
using Klf.Application.DTOs.Auth;
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
    public async Task<ActionResult<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken) =>
        StartSession(await authService.LoginAsync(request, cancellationToken));

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

    /// <summary>Returns the signed-in user.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CurrentUserResponse>> GetCurrentUserAsync(CancellationToken cancellationToken) =>
        Ok(await authService.GetCurrentUserAsync(User.GetUserId(), cancellationToken));

    private OkObjectResult StartSession(AuthSession session)
    {
        RefreshTokenCookie.Append(Response, session.RefreshToken, session.SessionExpiresAt);

        return Ok(new LoginResponse(session.AccessToken, session.AccessTokenExpiresAt));
    }
}
