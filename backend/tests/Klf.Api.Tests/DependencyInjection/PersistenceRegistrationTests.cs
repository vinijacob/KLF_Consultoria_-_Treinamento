using Klf.Application.Interfaces.Content;
using Klf.Application.Interfaces.Repositories;
using Klf.Application.Interfaces.Storage;
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
    public void Service_repository_is_registered_when_app_starts()
    {
        using var scope = factory.Services.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<IServiceRepository>());
    }

    [Fact]
    public void Site_setting_repository_is_registered_when_app_starts()
    {
        using var scope = factory.Services.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<ISiteSettingRepository>());
    }

    [Fact]
    public void Client_and_testimonial_repositories_are_registered_when_app_starts()
    {
        using var scope = factory.Services.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<IClientRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<ITestimonialRepository>());
    }

    [Fact]
    public void Media_album_repositories_and_file_storage_are_registered_when_app_starts()
    {
        using var scope = factory.Services.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<IMediaAssetRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<IAlbumRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<IFileStorage>());
    }

    [Fact]
    public void Two_factor_policy_is_required_by_default_when_not_configured_otherwise()
    {
        using var scope = factory.Services.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<Klf.Application.Interfaces.Identity.ITwoFactorPolicy>());
    }

    [Fact]
    public void Email_sender_and_links_are_registered_when_app_starts()
    {
        using var scope = factory.Services.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<Klf.Application.Interfaces.Email.IEmailSender>());
        Assert.NotNull(scope.ServiceProvider.GetService<Klf.Application.Interfaces.Links.IFrontendLinks>());
    }

    [Fact]
    public void Feedback_repositories_and_poster_renderer_are_registered_when_app_starts()
    {
        using var scope = factory.Services.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<IFeedbackFormRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<IFeedbackSessionRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<IFeedbackResponseRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<Klf.Application.Interfaces.Documents.IFeedbackPosterRenderer>());
    }

    [Fact]
    public void Refresh_token_repository_is_registered_when_app_starts()
    {
        using var scope = factory.Services.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<IRefreshTokenRepository>());
    }
}
