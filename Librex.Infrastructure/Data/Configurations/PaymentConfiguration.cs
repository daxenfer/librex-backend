using Librex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Librex.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.FolioNumber).IsRequired();
        builder.Property(p => p.Amount).HasColumnType("numeric(10,2)");
        builder.Property(p => p.PaymentMethod).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Reference).HasMaxLength(200);
        builder.Property(p => p.ReceivedFrom).HasMaxLength(200);
        builder.Property(p => p.Concept).HasMaxLength(500);
        builder.Property(p => p.CollectedBy).HasMaxLength(200);
        builder.Property(p => p.City).HasMaxLength(100);

        builder.HasIndex(p => p.FolioNumber).IsUnique();

        builder.HasOne(p => p.Customer)
               .WithMany()
               .HasForeignKey(p => p.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
