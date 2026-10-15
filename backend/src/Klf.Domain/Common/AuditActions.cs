namespace Klf.Domain.Common;

/// <summary>
/// Names of the actions in the audit log (<c>area.verb</c>). The area is the prefix used by the filters of the panel.
/// Content changes are named automatically from the entity (<c>post.created</c>, <c>client.deleted</c>…); the ones below
/// are recorded explicitly because they do not come from a content entity.
/// </summary>
public static class AuditActions
{
    /// <summary>Signed in to the panel (password and, when required, two-factor).</summary>
    public const string LoginSucceeded = "login.succeeded";

    /// <summary>Signed in with a one-time recovery code instead of the authenticator app.</summary>
    public const string RecoveryCodeUsed = "auth.recovery-code-used";

    /// <summary>Finished enrolling the authenticator app.</summary>
    public const string TwoFactorEnabled = "auth.two-factor-enabled";

    /// <summary>Generated a new set of recovery codes.</summary>
    public const string RecoveryCodesRegenerated = "auth.recovery-codes-regenerated";

    /// <summary>Chose a new password through the reset link.</summary>
    public const string PasswordReset = "auth.password-reset";

    /// <summary>Invited a person to the panel.</summary>
    public const string UserInvited = "user.invited";

    /// <summary>Generated a new invitation link.</summary>
    public const string UserInviteResent = "user.invite-resent";

    /// <summary>Accepted the invitation and chose the first password.</summary>
    public const string UserInviteAccepted = "user.invite-accepted";

    /// <summary>Changed someone's name.</summary>
    public const string UserUpdated = "user.updated";

    /// <summary>Changed someone's role.</summary>
    public const string UserRoleChanged = "user.role-changed";

    /// <summary>Deactivated an account.</summary>
    public const string UserDeactivated = "user.deactivated";

    /// <summary>Reactivated an account.</summary>
    public const string UserReactivated = "user.reactivated";

    /// <summary>Reset someone else's two-factor.</summary>
    public const string UserTwoFactorReset = "user.two-factor-reset";

    /// <summary>The daily job deleted contact messages past the retention period.</summary>
    public const string ContactPurged = "contact.purged";

    /// <summary>Areas used by the filter of the panel, with the action prefixes of each one.</summary>
    public static IReadOnlyDictionary<string, string[]> Areas { get; } = new Dictionary<string, string[]>
    {
        ["content"] = ["service.", "post.", "career.", "testimonial.", "client.", "media.", "album.", "settings."],
        ["contact"] = ["contact."],
        ["feedback"] = ["feedback-form.", "feedback-session."],
        ["team"] = ["user."],
        ["access"] = ["login.", "auth."],
    };
}
