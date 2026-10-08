namespace Klf.Infrastructure.Email;

/// <summary>E-mail settings, bound from the <c>Email</c> configuration section.</summary>
public sealed class EmailOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Email";

    /// <summary>Provider that only writes the e-mails to the application log (development).</summary>
    public const string LogProvider = "Log";

    /// <summary>Provider that sends through Resend (production).</summary>
    public const string ResendProvider = "Resend";

    /// <summary><c>Log</c> (development: the e-mail, link included, goes to the log) or <c>Resend</c>.</summary>
    public string Provider { get; init; } = LogProvider;

    /// <summary>Sender, e.g. <c>KLF Consultoria &lt;no-reply@seudominio.com.br&gt;</c>. The domain must be verified in Resend.</summary>
    public string From { get; init; } = string.Empty;

    /// <summary>Resend API key. Keep it in user-secrets or environment variables.</summary>
    public string ResendApiKey { get; init; } = string.Empty;
}

/// <summary>Frontend settings used to build links in e-mails, bound from the <c>Frontend</c> section.</summary>
public sealed class FrontendOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Frontend";

    /// <summary>Public address of the frontend, without a trailing slash (e.g. <c>https://klf.com.br</c>).</summary>
    public string BaseUrl { get; init; } = "http://localhost:3000";
}
