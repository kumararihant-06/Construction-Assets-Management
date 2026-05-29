using ConstructionAssetAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ConstructionAssetAPI.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<JobSite> JobSites => Set<JobSite>();
    public DbSet<Assignment> Assignments => Set<Assignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IdentityUserPasskey<Guid>>().HasKey(p => new { p.UserId, p.CredentialId });
        modelBuilder.Entity<IdentityPasskeyData>().HasKey(p => p.AttestationObject);

        modelBuilder.Entity<Equipment>()
            .HasIndex(e => e.SerialNumber)
            .IsUnique();
    }
}