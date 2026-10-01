namespace Klf.Domain.Exceptions;

/// <summary>
/// Base class for expected business errors. The API's exception middleware maps each subclass to an HTTP status code,
/// so the message must be safe to show to the end user (in Portuguese).
/// </summary>
/// <param name="message">User-facing error message.</param>
public abstract class DomainException(string message) : Exception(message);
