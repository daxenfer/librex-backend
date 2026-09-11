using Librex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Librex.Infrastructure.Data.Configurations;

public class LoginAttemptConfiguration : IEntityTypeConfiguration<LoginAttempt>
{
    public void Configure(EntityTypeBuilder<LoginAttempt> builder)
    {
        builder.ToTable("login_attempts");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Username).HasMaxLength(100).IsRequired();

        // Como string y no como int: una bitácora se lee a mano con psql, y "BadPassword" dice
        // más que un 3 que hay que ir a buscar al enum.
        builder.Property(a => a.Outcome).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(a => a.IpAddress).HasMaxLength(45);   // 45 = IPv6 con mapeo IPv4
        builder.Property(a => a.UserAgent).HasColumnType("text");

        // Las dos preguntas que se le hacen a esta tabla: "qué pasó en la última hora" y
        // "cuántos intentos ha habido contra esta cuenta".
        builder.HasIndex(a => a.OccurredAt);
        builder.HasIndex(a => new { a.Username, a.OccurredAt });
    }
}
