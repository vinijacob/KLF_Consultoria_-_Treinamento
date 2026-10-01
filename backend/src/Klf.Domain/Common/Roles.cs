namespace Klf.Domain.Common;

/// <summary>Names of the user roles, stored by Identity and sent in the JWT <c>role</c> claim.</summary>
public static class Roles
{
    /// <summary>Main administrator: full access, including managing other users.</summary>
    public const string Admin = "Admin";

    /// <summary>Publishes site content: projects, blog posts, photos and services.</summary>
    public const string Editor = "Editor";

    /// <summary>Creates feedback sessions and sees the responses of their own classes only.</summary>
    public const string Instructor = "Instructor";

    /// <summary>Every role, used to create them in the database on startup.</summary>
    public static IReadOnlyList<string> All { get; } = [Admin, Editor, Instructor];
}
