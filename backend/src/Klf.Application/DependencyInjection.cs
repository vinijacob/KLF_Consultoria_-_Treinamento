using Microsoft.Extensions.DependencyInjection;

namespace Klf.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
