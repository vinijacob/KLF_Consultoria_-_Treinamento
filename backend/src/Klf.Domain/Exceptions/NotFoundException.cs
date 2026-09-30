namespace Klf.Domain.Exceptions;

public sealed class NotFoundException(string message) : DomainException(message)
{
    public static NotFoundException For(string resource, object id) =>
        new($"{resource} '{id}' não foi encontrado(a).");
}
