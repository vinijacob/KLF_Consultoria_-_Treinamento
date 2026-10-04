using Klf.Api.Extensions;
using Klf.Api.Middlewares;
using Klf.Application;
using Klf.Infrastructure;
using Klf.Infrastructure.Storage;

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.AddPreferredSecuritySchemes(OpenApiExtensions.BearerScheme));
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

var storage = app.Services.GetRequiredService<IOptions<StorageOptions>>().Value;

if (storage.Provider == StorageOptions.LocalProvider)
{
    var uploads = Path.Combine(app.Environment.ContentRootPath, storage.LocalDirectory);
    Directory.CreateDirectory(uploads);

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploads),
        RequestPath = "/uploads",
        OnPrepareResponse = context => context.Context.Response.Headers.XContentTypeOptions = "nosniff",
    });
}

app.UseCors(ServiceCollectionExtensions.FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

app.Run();
