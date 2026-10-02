using Klf.Application.Interfaces.Content;
using Klf.Application.Interfaces.Repositories;
using Klf.Infrastructure.Persistence;

using Microsoft.Extensions.DependencyInjection;

namespace Klf.Api.Tests.DependencyInjection;

public sealed class PersistenceRegistrationTests(RealPersistenceApiFactory factory) : IClassFixture<RealPersistenceApiFactory>
{
    [Fact]
    public void Unit_of_work_is_the_same_db_context_when_resolved_in_one_request()
    {
        using var scope = factory.Services.CreateScope();

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Same(dbContext, unitOfWork);
    }

    [Fact]
    public void Career_entry_repository_is_registered_when_app_starts()
    {
        using var scope = factory.Services.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<ICareerEntryRepository>());
    }

    [Fact]
    public void Post_repository_and_sanitizer_are_registered_when_app_starts()
    {
        using var scope = factory.Services.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<IPostRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<IHtmlContentSanitizer>());
    }

    [Fact]
    public void Refresh_token_repository_is_registered_when_app_starts()
    {
        using var scope = factory.Services.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<IRefreshTokenRepository>());
    }
}
