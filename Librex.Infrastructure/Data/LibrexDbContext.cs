using Librex.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Data;

public class LibrexDbContext : DbContext
{
    public LibrexDbContext(DbContextOptions<LibrexDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Remission> Remissions => Set<Remission>();
    public DbSet<RemissionDetail> RemissionDetails => Set<RemissionDetail>();
    public DbSet<ReturnNote> ReturnNotes => Set<ReturnNote>();
    public DbSet<ReturnNoteDetail> ReturnNoteDetails => Set<ReturnNoteDetail>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentAllocation> PaymentAllocations => Set<PaymentAllocation>();
    public DbSet<CompanySettings> CompanySettings => Set<CompanySettings>();
    public DbSet<ErrorLog> ErrorLogs => Set<ErrorLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LibrexDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Modified))
        {
            entry.Entity.ModifiedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
