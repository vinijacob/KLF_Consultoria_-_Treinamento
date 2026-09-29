using Klf.Domain.Common;

namespace Klf.Domain.Tests.Common;

public sealed class SoftDeletableEntityTests
{
    [Fact]
    public void Entity_is_marked_as_deleted_when_soft_deleted()
    {
        var entity = new FakeEntity();

        entity.SoftDelete();

        Assert.True(entity.IsDeleted);
        Assert.NotNull(entity.UpdatedAt);
    }

    [Fact]
    public void Entity_is_not_deleted_when_restored()
    {
        var entity = new FakeEntity();
        entity.SoftDelete();

        entity.Restore();

        Assert.False(entity.IsDeleted);
    }

    [Fact]
    public void Id_is_uuid_v7_when_entity_is_created()
    {
        var entity = new FakeEntity();

        Assert.Equal(7, entity.Id.Version);
    }

    private sealed class FakeEntity : SoftDeletableEntity;
}
