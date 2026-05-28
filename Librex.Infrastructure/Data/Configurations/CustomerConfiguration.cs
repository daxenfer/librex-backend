using Librex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Librex.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Address).IsRequired().HasColumnType("text");
        builder.Property(c => c.PostalCode).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Phone).IsRequired().HasMaxLength(50);
        builder.Property(c => c.City).IsRequired().HasMaxLength(100);
    }
}
