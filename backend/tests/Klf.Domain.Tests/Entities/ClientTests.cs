using Klf.Domain.Entities;

namespace Klf.Domain.Tests.Entities;

public sealed class ClientTests
{
    [Fact]
    public void Every_field_but_the_position_is_replaced_when_client_is_updated()
    {
        var logoId = Guid.CreateVersion7();
        var client = new Client("Antiga", null, null, 0, true);

        client.Update("Nova", "https://nova.com.br", logoId, false);

        Assert.Equal("Nova", client.Name);
        Assert.Equal("https://nova.com.br", client.WebsiteUrl);
        Assert.Equal(logoId, client.LogoId);
        Assert.Equal(0, client.DisplayOrder);
        Assert.False(client.IsActive);
    }
}
