using Klf.Domain.Common;

using Microsoft.AspNetCore.Authorization;

namespace Klf.Api.Authorization;

/// <summary>
/// Named authorization policies. Controllers reference them with <c>[Authorize(Policy = Policies.X)]</c>
/// so the role rules live in one place.
/// </summary>
internal static class Policies
{
    /// <summary>Create, edit and deactivate users. Admin only.</summary>
    public const string ManageUsers = nameof(ManageUsers);

    /// <summary>Manage site content (services, posts, gallery, testimonials). Admin or Editor.</summary>
    public const string ManageContent = nameof(ManageContent);

    /// <summary>Manage feedback forms and sessions and see responses. Admin only.</summary>
    public const string ManageFeedback = nameof(ManageFeedback);

    /// <summary>Registers every policy.</summary>
    public static AuthorizationBuilder AddKlfPolicies(this AuthorizationBuilder builder) => builder
        .AddPolicy(ManageUsers, policy => policy.RequireRole(Roles.Admin))
        .AddPolicy(ManageContent, policy => policy.RequireRole(Roles.Admin, Roles.Editor))
        .AddPolicy(ManageFeedback, policy => policy.RequireRole(Roles.Admin));
}
