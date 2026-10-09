namespace Klf.Domain.Common;

/// <summary>Names of the user roles, stored by Identity and sent in the JWT <c>role</c> claim.</summary>
public static class Roles
{
    /// <summary>Main administrator: full access, including managing other users.</summary>
    public const string Admin = "Admin";

    /// <summary>Publishes site content: projects, blog posts, photos and services.</summary>
    public const string Editor = "Editor";

    /// <summary>Every role, used to create them in the database on startup. Only accounts with one of them can sign in.</summary>
    public static IReadOnlyList<string> All { get; } = [Admin, Editor];

    /// <summary>Whether an account with these roles may sign in to the admin panel.</summary>
    public static bool CanSignIn(IEnumerable<string> roles) => roles.Any(All.Contains);
}
