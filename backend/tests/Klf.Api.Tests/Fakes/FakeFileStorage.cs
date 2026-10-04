using Klf.Application.Interfaces.Storage;

namespace Klf.Api.Tests.Fakes;

public sealed class FakeFileStorage : IFileStorage
{
    public Dictionary<string, (byte[] Content, string ContentType)> Files { get; } = [];

    public Task SaveAsync(string key, Stream content, string contentType, CancellationToken cancellationToken)
    {
        using var copy = new MemoryStream();
        content.CopyTo(copy);
        Files[key] = (copy.ToArray(), contentType);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken)
    {
        Files.Remove(key);
        return Task.CompletedTask;
    }

    public string GetPublicUrl(string key) => "https://files.test/" + key;
}
