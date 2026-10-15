using System.Net;

using Amazon.S3;
using Amazon.S3.Model;

using Klf.Application.Interfaces.Storage;

namespace Klf.Infrastructure.Storage;

internal sealed class R2PrivateFileStorage : IPrivateFileStorage, IDisposable
{
    private readonly AmazonS3Client _client;
    private readonly string _bucket;

    public R2PrivateFileStorage(StorageOptions options)
    {
        var r2 = options.R2;

        if (string.IsNullOrWhiteSpace(r2.AccountId)
            || string.IsNullOrWhiteSpace(r2.AccessKeyId)
            || string.IsNullOrWhiteSpace(r2.SecretAccessKey)
            || string.IsNullOrWhiteSpace(r2.PrivateBucketName))
        {
            throw new InvalidOperationException(
                "Storage:R2 privado não configurado (AccountId, AccessKeyId, SecretAccessKey e PrivateBucketName). Use user-secrets ou variáveis de ambiente.");
        }

        if (string.Equals(r2.PrivateBucketName, r2.BucketName, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Storage:R2:PrivateBucketName precisa ser um bucket diferente do público (BucketName).");
        }

        _bucket = r2.PrivateBucketName;
        _client = R2FileStorage.CreateClient(r2);
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
        request.Headers.CacheControl = "private, no-store";

        await _client.PutObjectAsync(request, cancellationToken);
    }

    public async Task<byte[]?> ReadAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _client.GetObjectAsync(_bucket, key, cancellationToken);
            using var buffer = new MemoryStream();
            await response.ResponseStream.CopyToAsync(buffer, cancellationToken);

            return buffer.ToArray();
        }
        catch (AmazonS3Exception error) when (error.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken) =>
        await _client.DeleteObjectAsync(_bucket, key, cancellationToken);

    public void Dispose() => _client.Dispose();
}
