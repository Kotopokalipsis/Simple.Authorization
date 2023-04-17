using System.IO;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Integration.Fixtures
{
    public class CustomApplicationFactory<TTestStartup> : WebApplicationFactory<TTestStartup> where TTestStartup : class
    {
        protected override IHostBuilder CreateHostBuilder()
        {
            var host = Host
                .CreateDefaultBuilder()
                .ConfigureWebHost(builder =>
                {
                    builder.UseStartup<TTestStartup>()
                        .ConfigureAppConfiguration(conf =>
                        {
                            var projectDir = Directory.GetCurrentDirectory();
                            var configPath = Path.Combine(projectDir, "appsettings.IntegrationTests.json");

                            conf.AddJsonFile(configPath);
                        })
                        .ConfigureServices((context, collection) =>
                        {
                            collection.RemoveAll(typeof(ApplicationContext));
                            collection.AddDbContext<ApplicationContext>(options =>
                                options.UseNpgsql(context.Configuration.GetConnectionString("IdentityIntegrationTests")));
                            
                            var sp = collection.BuildServiceProvider();
                            
                            using var scope = sp.CreateScope();
                            
                            var scopedServices = scope.ServiceProvider;
                            
                            var db = scopedServices.GetRequiredService<ApplicationContext>();

                            db.Database.EnsureDeleted();
                            db.Database.EnsureCreated();
                        });
                });

            return host;
        }
    }
}