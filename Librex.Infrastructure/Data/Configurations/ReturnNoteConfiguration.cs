using Librex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Librex.Infrastructure.Data.Configurations;

public class ReturnNoteConfiguration : IEntityTypeConfiguration<ReturnNote>
{
    public void Configure(EntityTypeBuilder<ReturnNote> builder)
    {
        builder.ToTable("return_notes");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.FolioNumber).IsRequired();
        builder.Property(r => r.ReceivedBy).HasMaxLength(200);
        builder.Property(r => r.Discount).HasColumnType("numeric(10,2)");

        builder.HasIndex(r => r.FolioNumber).IsUnique();

        builder.HasOne(r => r.Customer)
               .WithMany()
               .HasForeignKey(r => r.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Remission)
               .WithMany()
               .HasForeignKey(r => r.RemissionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
