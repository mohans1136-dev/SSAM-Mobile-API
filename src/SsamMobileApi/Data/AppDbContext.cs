using SsamMobileApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace SsamMobileApi.Data;

/// <summary>
/// EF Core database context - the gateway to the MS SQL database.
///
/// Because the database already exists, you will REPLACE most of this file by
/// running the scaffold command (see README "Generate entities from the DB").
/// The scaffolder regenerates the DbSet properties and OnModelCreating mapping
/// from the real schema. Keep this hand-written version until then so the
/// project compiles.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Distributor> Distributors => Set<Distributor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Distributor>(entity =>
        {
            entity.ToTable("Distributors");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Region).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });
    }
}
