namespace Klf.Domain.Exceptions;

public sealed class ValidationException : DomainException
{
    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Um ou mais campos são inválidos.")
    {
        Errors = errors;
    }

    public ValidationException(string field, string error)
        : this(new Dictionary<string, string[]> { [field] = [error] })
    {
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
