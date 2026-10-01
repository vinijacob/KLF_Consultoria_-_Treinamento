namespace Klf.Domain.Exceptions;

/// <summary>
/// Thrown when a business rule rejects the input after the request already passed FluentValidation
/// (for example, a rule that depends on the database). Returned as HTTP 400 with the errors grouped by field.
/// </summary>
public sealed class ValidationException : DomainException
{
    /// <summary>Creates the exception with errors for one or more fields.</summary>
    /// <param name="errors">Error messages grouped by field name.</param>
    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Um ou mais campos são inválidos.")
    {
        Errors = errors;
    }

    /// <summary>Creates the exception with a single error for one field.</summary>
    /// <param name="field">Field name, matching the request DTO property.</param>
    /// <param name="error">User-facing error message.</param>
    public ValidationException(string field, string error)
        : this(new Dictionary<string, string[]> { [field] = [error] })
    {
    }

    /// <summary>Error messages grouped by field name.</summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
