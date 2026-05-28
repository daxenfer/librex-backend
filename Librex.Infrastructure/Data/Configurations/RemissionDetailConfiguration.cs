using Librex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Librex.Infrastructure.Data.Configurations;

public class RemissionDetailConfiguration : IEntityTypeConfiguration<RemissionDetail>
{
    public void Configure(EntityTypeBuilder<RemissionDetail> builder)
    {
        builder.ToTable("remission_details");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Teacher).HasMaxLength(200);
        builder.Property(d => d.Quantity).HasColumnType("numeric(10,2)");
        builder.Property(d => d.UnitPrice).HasColumnType("numeric(10,2)");

        builder.HasOne(d => d.Remission)
               .WithMany(r => r.Details)
               .HasForeignKey(d => d.RemissionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Product)
               .WithMany()
               .HasForeignKey(d => d.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
