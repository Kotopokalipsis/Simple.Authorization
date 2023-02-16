using Application.Common.Interfaces.Application.Services;
using Application.Common.Services.CookieHelper;
using Application.Common.Services.TokenHelper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Application;

public static class ApplicationModule
{
    public static IServiceCollection AddApplicationModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddScoped<ICookieHelper, CookieHelper>();
        services.TryAddScoped<ITokenHelper, TokenHelper>();
        
        return services;
    }
}