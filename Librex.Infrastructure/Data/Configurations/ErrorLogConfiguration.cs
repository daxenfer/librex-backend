using Librex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Librex.Infrastructure.Data.Configurations;

public class ErrorLogConfiguration : IEntityTypeConfiguration<ErrorLog>
{
    public void Configure(EntityTypeBuilder<ErrorLog> builder)
    {
        builder.ToTable("error_logs");
        builder.HasKey(e => e.Id);

        // Todas las columnas de texto son "text" (sin límite): el contenido viene de
        // excepciones y requests arbitrarios, y el middleware ya trunca en código antes
        // de insertar — el tipo sin límite es la segunda capa, no la primera.
        builder.Property(e => e.RequestId).HasColumnType("text").IsRequired();
        builder.Property(e => e.Method).HasColumnType("text").IsRequired();
        builder.Property(e => e.Path).HasColumnType("text").IsRequired();
        builder.Property(e => e.QueryString).HasColumnType("text");
        builder.Property(e => e.RouteValues).HasColumnType("text");
        builder.Property(e => e.RequestBody).HasColumnType("text");
        builder.Property(e => e.ExceptionType).HasColumnType("text").IsRequired();
        builder.Property(e => e.Message).HasColumnType("text").IsRequired();
        builder.Property(e => e.StackTrace).HasColumnType("text");
        builder.Property(e => e.Username).HasColumnType("text");

        builder.HasIndex(e => e.OccurredAt);
    }
}
