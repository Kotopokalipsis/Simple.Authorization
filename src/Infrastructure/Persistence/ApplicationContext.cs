using System;
using System.Data;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Persistence.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence
{
    public class ApplicationContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public DbSet<RefreshTokenBlacklist> RefreshTokenBlacklists { get; set; }
        private IDbContextTransaction ContextTransaction { get; set; }
        public bool HasActiveTransaction => ContextTransaction != null;
        
        public ApplicationContext()
        {
        }
        
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var builder = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.Migrations.json");
                var cfg = builder.Build();

                var connectionString = cfg["ConnectionStrings:MigrationConnection"];
                optionsBuilder.UseNpgsql(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new RefreshTokenBlacklistConfiguration());

            base.OnModelCreating(modelBuilder);
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