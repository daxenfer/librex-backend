using Librex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Librex.Infrastructure.Data.Configurations;

public class RemissionConfiguration : IEntityTypeConfiguration<Remission>
{
    public void Configure(EntityTypeBuilder<Remission> builder)
    {
        builder.ToTable("remissions");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.FolioNumber).IsRequired();
        builder.Property(r => r.SalesPerson).HasMaxLength(200);
        builder.Property(r => r.RecipientName).HasMaxLength(200);
        builder.Property(r => r.Discount).HasColumnType("numeric(10,2)");
        builder.Property(r => r.ReturnPercentage).HasColumnType("numeric(5,2)");

        builder.HasIndex(r => r.FolioNumber).IsUnique();

        builder.HasOne(r => r.Customer)
               .WithMany()
               .HasForeignKey(r => r.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
