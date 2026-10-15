using Klf.Application.Interfaces.Storage;

namespace Klf.Infrastructure.Storage;

internal sealed class LocalPrivateFileStorage(string rootDirectory) : IPrivateFileStorage
{
    private readonly string _root = Path.GetFullPath(rootDirectory);

    public async Task SaveAsync(string key, Stream content, string contentType, CancellationToken cancellationToken)
    {
        var path = LocalFileStorage.ResolvePath(_root, key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var file = File.Create(path);
        await content.CopyToAsync(file, cancellationToken);
    }

    public async Task<byte[]?> ReadAsync(string key, CancellationToken cancellationToken)
    {
        var path = LocalFileStorage.ResolvePath(_root, key);

        return File.Exists(path) ? await File.ReadAllBytesAsync(path, cancellationToken) : null;
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken)
    {
        File.Delete(LocalFileStorage.ResolvePath(_root, key));

        return Task.CompletedTask;
    }
}
