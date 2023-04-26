using Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Web;

public static class WebApplicationExtension
{
    /// <summary>
    /// Запускает миграции.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        var serviceScopeFactory = app.ApplicationServices.GetService<IServiceScopeFactory>();
        if (serviceScopeFactory != null)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetService<ApplicationContext>();
            context.Database.Migrate();
        }
    }
}