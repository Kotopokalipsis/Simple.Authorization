using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class RefreshTokenBlacklistConfiguration : IEntityTypeConfiguration<RefreshTokenBlacklist>
    {
        public void Configure(EntityTypeBuilder<RefreshTokenBlacklist> builder)
        {
            builder.ToTable("RefreshTokenBlacklist");
            builder.HasKey(x => x.Id);
            builder.Ignore(x => x.DomainEvents);
            builder.Property(x => x.RefreshToken).IsRequired();
            builder.HasIndex(x => x.RefreshToken).IsUnique();
        }
    }
}