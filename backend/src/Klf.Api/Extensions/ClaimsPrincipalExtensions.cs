using System.Security.Claims;

using Klf.Domain.Exceptions;
using Klf.Infrastructure.Identity;

namespace Klf.Api.Extensions;

/// <summary>Helpers to read the authenticated user from the JWT claims.</summary>
internal static class ClaimsPrincipalExtensions
{
    /// <summary>Returns the user id from the <c>sub</c> claim.</summary>
    /// <exception cref="UnauthorizedException">The claim is missing or is not a valid id.</exception>
    public static Guid GetUserId(this ClaimsPrincipal principal) =>
        Guid.TryParse(principal.FindFirstValue(ClaimTypeNames.Subject), out var userId)
            ? userId
            : throw new UnauthorizedException("Sessão inválida. Entre novamente.");
}
