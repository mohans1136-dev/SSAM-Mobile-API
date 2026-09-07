using SSAMMobileApp.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace SSAMMobileApp.Data;

/// <summary>
/// EF Core database context - the gateway to the MS SQL database.
///
/// Entities under Data/Entities replicate the production [MTL] schema exactly
/// (see MR.cs / MRDetail.cs). This context is database-first: it never runs
/// migrations against the real database.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<MR> MR => Set<MR>();
    public DbSet<MRDetail> MRDetail => Set<MRDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Most mapping is via data annotations on the entities. Only the bits
        // that annotations can't express live here.
        modelBuilder.Entity<MRDetail>()
            .HasOne(d => d.MR)
            .WithMany(m => m.MRDetails)
            .HasForeignKey(d => d.MRId)
            .HasConstraintName("FK_MRDetail_MR")
            .OnDelete(DeleteBehavior.Restrict); // no cascade
    }
}
