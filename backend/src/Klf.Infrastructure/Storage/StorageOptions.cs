namespace Klf.Infrastructure.Storage;

/// <summary>File storage settings, bound from the <c>Storage</c> configuration section.</summary>
public sealed class StorageOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Storage";

    /// <summary>Provider that holds <see cref="Provider"/> when files go to a local folder (development).</summary>
    public const string LocalProvider = "Local";

    /// <summary>Provider that holds <see cref="Provider"/> when files go to Cloudflare R2 (production).</summary>
    public const string R2Provider = "R2";

    /// <summary><c>Local</c> (a folder served by the API, for development) or <c>R2</c> (Cloudflare R2).</summary>
    public string Provider { get; init; } = LocalProvider;

    /// <summary>Address that prefixes every file key in public URLs (e.g. the R2 custom domain, or <c>http://localhost:5004/uploads</c>).</summary>
    public string PublicBaseUrl { get; init; } = "/uploads";

    /// <summary>Folder for the <c>Local</c> provider, relative to the API content root.</summary>
    public string LocalDirectory { get; init; } = "uploads";

    /// <summary>Cloudflare R2 credentials and bucket; only read when <see cref="Provider"/> is <c>R2</c>.</summary>
    public R2Options R2 { get; init; } = new();
}

/// <summary>Cloudflare R2 connection settings. Keep the secrets in user-secrets or environment variables.</summary>
public sealed class R2Options
{
    /// <summary>Cloudflare account id.</summary>
    public string AccountId { get; init; } = string.Empty;

    /// <summary>R2 API token access key id.</summary>
    public string AccessKeyId { get; init; } = string.Empty;

    /// <summary>R2 API token secret access key.</summary>
    public string SecretAccessKey { get; init; } = string.Empty;

    /// <summary>Bucket name.</summary>
    public string BucketName { get; init; } = string.Empty;
}
