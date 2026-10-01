namespace Klf.Domain.Exceptions;

/// <summary>
/// Thrown when the operation conflicts with the current state, such as a duplicated slug or an already closed session.
/// Returned as HTTP 409.
/// </summary>
/// <param name="message">User-facing error message.</param>
public sealed class ConflictException(string message) : DomainException(message);
