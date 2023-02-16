using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces.Infrastructure.Repositories.Interfaces;
using Application.Common.Interfaces.Infrastructure.UnitOfWork;
using Infrastructure.Persistence;

namespace Infrastructure.UoW;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationContext _context;
    private readonly IServiceProvider _serviceProvider;
    
    private IRefreshTokenBlacklistRepository _refreshTokenBlacklistRepository;
    public IRefreshTokenBlacklistRepository RefreshTokenBlacklistRepository => _refreshTokenBlacklistRepository ??= GetService<IRefreshTokenBlacklistRepository>();

    public UnitOfWork(ApplicationContext context, IServiceProvider serviceProvider)
    {
        _context = context;
        _serviceProvider = serviceProvider;
    }

    private T GetService<T>() => (T)_serviceProvider.GetService(typeof(T));
    
    public Task Commit(CancellationToken ct = default)
    {
        return _context.SaveChangesAsync(ct);
    }
}