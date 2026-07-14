using Librex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Librex.Infrastructure.Data.Configurations;

public class PaymentAllocationConfiguration : IEntityTypeConfiguration<PaymentAllocation>
{
    public void Configure(EntityTypeBuilder<PaymentAllocation> builder)
    {
        builder.ToTable("payment_allocations");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Amount).HasColumnType("numeric(10,2)");

        builder.HasOne(a => a.Payment)
               .WithMany(p => p.Allocations)
               .HasForeignKey(a => a.PaymentId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Remission)
               .WithMany()
               .HasForeignKey(a => a.RemissionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
