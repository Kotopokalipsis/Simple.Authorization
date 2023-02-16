using Application.Common.Interfaces.Infrastructure.Repositories.Interfaces;
using Application.Common.Interfaces.Infrastructure.Services;
using Application.Common.Interfaces.Infrastructure.UnitOfWork;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Common;
using Infrastructure.Services.JwtGenerator;
using Infrastructure.Services.MailSender;
using Infrastructure.UoW;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Infrastructure;

public static class InfrastructureModule
{
    public static IServiceCollection AddInfrastructureModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("IdentityDB")));

        services.TryAddScoped<IUnitOfWork, UnitOfWork>();

        services.TryAddScoped<IRepository<RefreshTokenBlacklist>, Repository<RefreshTokenBlacklist>>();
        services.TryAddScoped<IRefreshTokenBlacklistRepository, RefreshTokenBlacklistRepository>();
        
        services.AddSingleton<IMailSender, MailSender>();
        services.TryAddScoped<IJwtGenerator, JwtGenerator>();
        
        return services;
    }
}