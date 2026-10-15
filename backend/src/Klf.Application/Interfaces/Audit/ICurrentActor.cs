namespace Klf.Application.Interfaces.Audit;

/// <summary>Who is doing the current request, for the audit log. Empty for site visitors and background jobs.</summary>
public interface ICurrentActor
{
    /// <summary>Signed-in user id, or <see langword="null"/>.</summary>
    Guid? UserId { get; }

    /// <summary>Signed-in user name, or <see langword="null"/>.</summary>
    string? Name { get; }
}
