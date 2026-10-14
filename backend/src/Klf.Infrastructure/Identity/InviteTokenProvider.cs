using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Klf.Infrastructure.Identity;

/// <summary>Lifetime of invitation tokens, separate from the 30 minutes of password reset tokens.</summary>
internal sealed class InviteTokenProviderOptions : DataProtectionTokenProviderOptions
{
    public InviteTokenProviderOptions()
    {
        Name = InviteTokenProvider.ProviderName;
        TokenLifespan = TimeSpan.FromDays(Klf.Application.Interfaces.Identity.IUserDirectory.InviteLifetimeDays);
    }
}

/// <summary>
/// Data Protection tokens for invitations. Like the reset tokens, they carry the security stamp, so choosing the password
/// (which changes the stamp) or deactivating the account makes them stop working.
/// </summary>
internal sealed class InviteTokenProvider(
    IDataProtectionProvider dataProtectionProvider,
    IOptions<InviteTokenProviderOptions> options,
    ILogger<DataProtectorTokenProvider<ApplicationUser>> logger)
    : DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider, options, logger)
{
    public const string ProviderName = "KlfInvite";

    public const string Purpose = "invite";
}
