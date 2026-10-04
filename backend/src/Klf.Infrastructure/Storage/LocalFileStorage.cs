using Klf.Application.Interfaces.Storage;

namespace Klf.Infrastructure.Storage;

internal sealed class LocalFileStorage(string rootDirectory, string publicBaseUrl) : IFileStorage
{
    private readonly string _root = Path.GetFullPath(rootDirectory);

    public async Task SaveAsync(string key, Stream content, string contentType, CancellationToken cancellationToken)
    {
        var path = ResolvePath(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var file = File.Create(path);
        await content.CopyToAsync(file, cancellationToken);
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken)
    {
        File.Delete(ResolvePath(key));

        return Task.CompletedTask;
    }

    public string GetPublicUrl(string key) => $"{publicBaseUrl.TrimEnd('/')}/{key}";

    private string ResolvePath(string key)
    {
        var path = Path.GetFullPath(Path.Combine(_root, key));

        if (!path.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new ArgumentException("Chave de arquivo inválida.", nameof(key));
        }

        return path;
    }
}
