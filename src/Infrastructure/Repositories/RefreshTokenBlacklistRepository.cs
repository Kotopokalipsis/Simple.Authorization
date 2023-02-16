using Application.Common.Interfaces.Infrastructure.Repositories.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Repositories.Common;

namespace Infrastructure.Repositories;

public class RefreshTokenBlacklistRepository : Repository<RefreshTokenBlacklist>, IRefreshTokenBlacklistRepository
{
    public RefreshTokenBlacklistRepository(ApplicationContext context) : base(context)
    {
    }
}