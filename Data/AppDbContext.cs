using ConstructionAssetAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConstructionAssetAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<JobSite> JobSites => Set<JobSite>();
    public DbSet<Assignment> Assignments => Set<Assignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Enforce SerialNumber uniqueness at the DB level.
        // EF Core translates this into a UNIQUE INDEX in the SQLite schema.
        modelBuilder.Entity<Equipment>()
            .HasIndex(e => e.SerialNumber)
            .IsUnique();
    }
}