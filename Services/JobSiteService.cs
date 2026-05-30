using ConstructionAssetAPI.Data;
using ConstructionAssetAPI.Endpoints;
using ConstructionAssetAPI.Entities;
using ConstructionAssetAPI.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ConstructionAssetAPI.Services;

public class JobSiteService
{
    private readonly AppDbContext _db;

    public JobSiteService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<JobSite>> GetAllAsync()
        => await _db.JobSites.ToListAsync();

    public async Task<JobSite> GetByIdAsync(int id)
    {
        var site = await _db.JobSites.FindAsync(id)
            ?? throw new NotFoundException($"JobSite {id} not found.");
        return site;
    }

    public async Task<JobSite> CreateAsync(JobSiteInput input)
    {
        var site = new JobSite
        {
            Name = input.Name,
            Location = input.Location,
            StartDate = input.StartDate,
            EndDate = input.EndDate
        };

        _db.JobSites.Add(site);
        await _db.SaveChangesAsync();
        return site;
    }

    public async Task UpdateAsync(int id, JobSiteInput input)
    {
        var site = await _db.JobSites.FindAsync(id)
            ?? throw new NotFoundException($"JobSite {id} not found.");

        site.Name = input.Name;
        site.Location = input.Location;
        site.StartDate = input.StartDate;
        site.EndDate = input.EndDate;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var site = await _db.JobSites.FindAsync(id)
            ?? throw new NotFoundException($"JobSite {id} not found.");

        _db.JobSites.Remove(site);
        await _db.SaveChangesAsync();
    }
}