using Microsoft.EntityFrameworkCore;

namespace Company.Infrastructure.Persistence;

/// <summary>
/// Represents the Entity Framework Core database context for the company domain.
/// </summary>
public class CompanyDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompanyDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public CompanyDbContext(DbContextOptions<CompanyDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the companies table.
    /// </summary>
    public DbSet<Domain.Entities.Company> Companies { get; set; } = null!;

    /// <summary>
    /// Configures the entity mappings and constraints for the model.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Company entity
        modelBuilder.Entity<Domain.Entities.Company>(entity =>
        {
            entity.ToTable("Companies", tb =>
                tb.HasCheckConstraint("CK_ISIN_Format", "ISIN ~ '^[A-Z]{2}[A-Z0-9]{9}[0-9]$'"));

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Ticker)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(e => e.Exchange)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.ISIN)
                .IsRequired()
                .HasMaxLength(12);

            entity.HasIndex(e => e.ISIN)
                .IsUnique();

            entity.Property(e => e.Website)
                .HasMaxLength(255);
        });
    }
}