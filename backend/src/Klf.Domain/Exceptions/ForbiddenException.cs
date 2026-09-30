namespace Klf.Domain.Exceptions;

public sealed class ForbiddenException(string message = "Você não tem permissão para realizar esta ação.")
    : DomainException(message);
