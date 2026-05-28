using Librex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Librex.Infrastructure.Data.Configurations;

public class CompanySettingsConfiguration : IEntityTypeConfiguration<CompanySettings>
{
    public void Configure(EntityTypeBuilder<CompanySettings> builder)
    {
        builder.ToTable("company_settings");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.CompanyName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.BrandName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Rfc).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Phone1).HasMaxLength(50);
        builder.Property(c => c.Phone2).HasMaxLength(50);
        builder.Property(c => c.Email).HasMaxLength(200);
        builder.Property(c => c.Address).HasMaxLength(300);
        builder.Property(c => c.PostalCode).HasMaxLength(10);
        builder.Property(c => c.City).HasMaxLength(100);
        builder.Property(c => c.State).HasMaxLength(100);
    }
}
