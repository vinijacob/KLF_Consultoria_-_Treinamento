using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

using Klf.Application.Interfaces.Storage;

namespace Klf.Infrastructure.Storage;

internal sealed class R2FileStorage : IFileStorage, IDisposable
{
    private readonly AmazonS3Client _client;
    private readonly string _bucket;
    private readonly string _publicBaseUrl;

    public R2FileStorage(StorageOptions options)
    {
        var r2 = options.R2;

        if (string.IsNullOrWhiteSpace(r2.AccountId)
            || string.IsNullOrWhiteSpace(r2.AccessKeyId)
            || string.IsNullOrWhiteSpace(r2.SecretAccessKey)
            || string.IsNullOrWhiteSpace(r2.BucketName))
        {
            throw new InvalidOperationException(
                "Storage:R2 não configurado (AccountId, AccessKeyId, SecretAccessKey e BucketName). Use user-secrets ou variáveis de ambiente.");
        }

        _bucket = r2.BucketName;
        _publicBaseUrl = options.PublicBaseUrl.TrimEnd('/');

        _client = new AmazonS3Client(
            new BasicAWSCredentials(r2.AccessKeyId, r2.SecretAccessKey),
            new AmazonS3Config
            {
                ServiceURL = $"https://{r2.AccountId}.r2.cloudflarestorage.com",
                AuthenticationRegion = "auto",
                ForcePathStyle = true,
                RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
                ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED,
            });
    }

    public async Task SaveAsync(string key, Stream content, string contentType, CancellationToken cancellationToken)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = key,
            InputStream = content,
            ContentType = contentType,
        };
        request.Headers.CacheControl = "public, max-age=31536000, immutable";

        await _client.PutObjectAsync(request, cancellationToken);
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken) =>
        await _client.DeleteObjectAsync(_bucket, key, cancellationToken);

    public string GetPublicUrl(string key) => $"{_publicBaseUrl}/{key}";

    public void Dispose() => _client.Dispose();
}
