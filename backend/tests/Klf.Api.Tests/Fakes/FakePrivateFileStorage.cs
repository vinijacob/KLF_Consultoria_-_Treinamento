using Klf.Application.Interfaces.Storage;

namespace Klf.Api.Tests.Fakes;

public sealed class FakePrivateFileStorage : IPrivateFileStorage
{
    public Dictionary<string, byte[]> Files { get; } = [];

    public Task SaveAsync(string key, Stream content, string contentType, CancellationToken cancellationToken)
    {
        using var copy = new MemoryStream();
        content.CopyTo(copy);
        Files[key] = copy.ToArray();
        return Task.CompletedTask;
    }

    public Task<byte[]?> ReadAsync(string key, CancellationToken cancellationToken) =>
        Task.FromResult(Files.TryGetValue(key, out var content) ? content : null);

    public Task DeleteAsync(string key, CancellationToken cancellationToken)
    {
        Files.Remove(key);
        return Task.CompletedTask;
    }
}
