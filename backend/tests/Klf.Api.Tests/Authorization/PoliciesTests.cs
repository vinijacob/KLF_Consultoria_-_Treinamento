using System.Security.Claims;

using Klf.Api.Authorization;
using Klf.Domain.Common;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Klf.Api.Tests.Authorization;

public sealed class PoliciesTests(KlfApiFactory factory) : IClassFixture<KlfApiFactory>
{
    public static TheoryData<string, string, bool> Cases => new()
    {
        { Policies.ManageUsers, Roles.Admin, true },
        { Policies.ManageUsers, Roles.Editor, false },
        { Policies.ManageUsers, Roles.Instructor, false },
        { Policies.ManageContent, Roles.Admin, true },
        { Policies.ManageContent, Roles.Editor, true },
        { Policies.ManageContent, Roles.Instructor, false },
        { Policies.ManageFeedback, Roles.Admin, true },
        { Policies.ManageFeedback, Roles.Editor, false },
        { Policies.ManageFeedback, Roles.Instructor, true },
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task Access_matches_role_when_policy_is_evaluated(string policy, string role, bool expected)
    {
        var authorization = factory.Services.GetRequiredService<IAuthorizationService>();
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim("role", role)], "Test", "name", "role"));

        var result = await authorization.AuthorizeAsync(user, policy);

        Assert.Equal(expected, result.Succeeded);
    }
}
