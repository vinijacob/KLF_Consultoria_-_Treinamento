using FluentValidation;

using Klf.Application.DTOs;
using Klf.Application.Interfaces.Audit;
using Klf.Application.Services.Albums;
using Klf.Application.Services.Audit;
using Klf.Application.Services.Auth;
using Klf.Application.Services.Career;
using Klf.Application.Services.Catalog;
using Klf.Application.Services.Clients;
using Klf.Application.Services.Contact;
using Klf.Application.Services.DataRequests;
using Klf.Application.Services.FeaturedQuote;
using Klf.Application.Services.Feedback;
using Klf.Application.Services.Media;
using Klf.Application.Services.Posts;
using Klf.Application.Services.Settings;
using Klf.Application.Services.Testimonials;
using Klf.Application.Services.Users;

using Microsoft.Extensions.DependencyInjection;

namespace Klf.Application;

/// <summary>Registers the Application layer services.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers application services and every FluentValidation validator found in this assembly.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ILoginDeviceService, LoginDeviceService>();
        services.AddScoped<ICareerEntryService, CareerEntryService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
        services.AddScoped<ISiteSettingService, SiteSettingService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<ITestimonialService, TestimonialService>();
        services.AddScoped<ITestimonialConsentDocumentService, TestimonialConsentDocumentService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<IAlbumService, AlbumService>();
        services.AddScoped<IFeedbackFormService, FeedbackFormService>();
        services.AddScoped<IFeedbackSessionService, FeedbackSessionService>();
        services.AddScoped<IPublicFeedbackService, PublicFeedbackService>();
        services.AddScoped<IContactMessageService, ContactMessageService>();
        services.AddScoped<IDataRequestService, DataRequestService>();
        services.AddScoped<IFeaturedQuoteService, FeaturedQuoteService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IAuditTrail, AuditTrail>();
        services.AddScoped<IAuditLogService, AuditLogService>();

        return services;
    }
}
