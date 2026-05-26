using Librex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Librex.Infrastructure.Data.Configurations;

public class ReturnNoteDetailConfiguration : IEntityTypeConfiguration<ReturnNoteDetail>
{
    public void Configure(EntityTypeBuilder<ReturnNoteDetail> builder)
    {
        builder.ToTable("return_note_details");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Quantity).HasColumnType("numeric(10,2)");
        builder.Property(d => d.UnitPrice).HasColumnType("numeric(10,2)");

        builder.HasOne(d => d.ReturnNote)
               .WithMany(r => r.Details)
               .HasForeignKey(d => d.ReturnNoteId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Product)
               .WithMany()
               .HasForeignKey(d => d.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
