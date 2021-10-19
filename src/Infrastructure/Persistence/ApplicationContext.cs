using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence.Configurations;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Persistence
{
    public class ApplicationContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IUnitOfWork
    {
        public const string DEFAULT_SCHEMA = "application";
        
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
        public DbSet<RefreshTokenBlacklist> RefreshTokenBlacklists { get; set; }
        
        private readonly IMediator _mediator;
        private IDbContextTransaction ContextTransaction { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }
        
        public bool HasActiveTransaction => ContextTransaction != null;
        
        public ApplicationContext(DbContextOptions<ApplicationContext> options, IMediator mediator) : base(options)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserRefreshTokensConfiguration());
            modelBuilder.ApplyConfiguration(new RefreshTokenBlacklistConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            
            base.OnModelCreating(modelBuilder);
        }
        
        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            // Dispatch Domain Events collection. 
            // Choices:
            // A) Right BEFORE committing data (EF SaveChanges) into the DB will make a single transaction including  
            // side effects from the domain event handlers which are using the same DbContext with "InstancePerLifetimeScope" or "scoped" lifetime
            // B) Right AFTER committing data (EF SaveChanges) into the DB will make multiple transactions. 
            // You will need to handle eventual consistency and compensatory actions in case of failures in any of the Handlers. 
            await _mediator.DispatchDomainEventsAsync(this);

            // After executing this line all the changes (from the Command Handler and Domain Event Handlers) 
            // performed through the DbContext will be committed
            var result = await base.SaveChangesAsync(cancellationToken);

            return true;
        }
        
        public async Task BeginTransactionAsync()
        {
            ContextTransaction ??= await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        }

        public async Task CommitTransactionAsync()
        {
            if (ContextTransaction == null) throw new ArgumentNullException(nameof(ContextTransaction));

            try
            {
                await SaveChangesAsync();
                await ContextTransaction.CommitAsync();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
            finally
            {
                if (ContextTransaction != null)
                {
                    ContextTransaction.Dispose();
                    ContextTransaction = null;
                }
            }
        }

        public void RollbackTransaction()
        {
            try
            {
                ContextTransaction?.Rollback();
            }
            finally
            {
                if (ContextTransaction != null)
                {
                    ContextTransaction.Dispose();
                    ContextTransaction = null;
                }
            }
        }
    }
}