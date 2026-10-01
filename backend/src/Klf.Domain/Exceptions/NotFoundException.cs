namespace Klf.Domain.Exceptions;

/// <summary>
/// Thrown when a requested resource does not exist (or is soft-deleted). Returned as HTTP 404.
/// </summary>
/// <param name="message">User-facing error message.</param>
public sealed class NotFoundException(string message) : DomainException(message)
{
    /// <summary>Creates the exception with a standard message for the given resource and identifier.</summary>
    /// <param name="resource">Resource name shown to the user, in Portuguese (e.g. "Post").</param>
    /// <param name="id">Identifier that was not found.</param>
    public static NotFoundException For(string resource, object id) =>
        new($"{resource} '{id}' não foi encontrado(a).");
}
