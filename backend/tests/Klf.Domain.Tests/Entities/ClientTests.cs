using Klf.Domain.Entities;

namespace Klf.Domain.Tests.Entities;

public sealed class ClientTests
{
    [Fact]
    public void Every_field_is_replaced_when_client_is_updated()
    {
        var logoId = Guid.CreateVersion7();
        var client = new Client("Antiga", null, null, 0, true);

        client.Update("Nova", "https://nova.com.br", logoId, 4, false);

        Assert.Equal("Nova", client.Name);
        Assert.Equal("https://nova.com.br", client.WebsiteUrl);
        Assert.Equal(logoId, client.LogoId);
        Assert.Equal(4, client.DisplayOrder);
        Assert.False(client.IsActive);
    }
}
