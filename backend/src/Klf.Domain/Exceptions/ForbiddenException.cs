namespace Klf.Domain.Exceptions;

/// <summary>
/// Thrown when an authenticated user is not allowed to perform the operation. Returned as HTTP 403.
/// </summary>
/// <param name="message">User-facing error message.</param>
public sealed class ForbiddenException(string message = "Você não tem permissão para realizar esta ação.")
    : DomainException(message);
