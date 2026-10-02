using Klf.Domain.Entities;
using Klf.Domain.Enums;

namespace Klf.Domain.Tests.Entities;

public sealed class ServiceTests
{
    [Fact]
    public void Every_field_is_replaced_when_service_is_updated()
    {
        var coverId = Guid.CreateVersion7();
        var service = new Service("Antigo", "antigo", null, "<p>a</p>", "Todos", 4, ServiceFormat.Online, null, 0, true);

        service.Update("Novo", "novo", "Resumo", "<p>b</p>", "Gestores", 16, ServiceFormat.InCompany, coverId, 3, false);

        Assert.Equal("Novo", service.Title);
        Assert.Equal("novo", service.Slug);
        Assert.Equal("Resumo", service.Summary);
        Assert.Equal("<p>b</p>", service.ContentHtml);
        Assert.Equal("Gestores", service.Audience);
        Assert.Equal(16, service.WorkloadHours);
        Assert.Equal(ServiceFormat.InCompany, service.Format);
        Assert.Equal(coverId, service.CoverId);
        Assert.Equal(3, service.DisplayOrder);
        Assert.False(service.IsActive);
    }
}
