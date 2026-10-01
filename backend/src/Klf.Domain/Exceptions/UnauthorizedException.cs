namespace Klf.Domain.Exceptions;

/// <summary>
/// Thrown when the caller could not be authenticated, such as wrong credentials or a locked account. Returned as HTTP 401.
/// </summary>
/// <param name="message">User-facing error message.</param>
public sealed class UnauthorizedException(string message) : DomainException(message);
