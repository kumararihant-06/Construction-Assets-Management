using ConstructionAssetAPI.Data;
using ConstructionAssetAPI.Entities;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace ConstructionAssetAPI.Endpoints;

public record JobSiteInput
(
    string Name,
    string Location,
    DateTime StartDate,
    DateTime? EndDate
);

public static class JobSiteEndpoints
{
    public static void MapJobSiteEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/jobsites").WithTags("JobSites").RequireAuthorization("RequireAnyAuthenticatedUser");

        group.MapGet("/", async (AppDbContext db) => await db.JobSites.ToListAsync());

        group.MapGet("/{id:int}", async(int id, AppDbContext db) =>
            await db.JobSites.FindAsync(id) is JobSite site
                ? Results.Ok(site) : Results.NotFound());
        
        group.MapPost("/", async (JobSiteInput input, AppDbContext db) =>
        {
            var site = new JobSite
            {
                Name = input.Name,
                Location = input.Location,
                StartDate = input.StartDate,
                EndDate = input.EndDate
            };

            db.JobSites.Add(site);
            await db.SaveChangesAsync();

            return Results.Created($"/api/v1/jobsites/{site.Id}",site);
        }).RequireAuthorization("RequireManagerOrAbove");

        group.MapPut("/{id:int}", async (int id, JobSiteInput input, AppDbContext db) =>
        {
            var site = await db.JobSites.FindAsync(id);
            if (site is null) return Results.NotFound();
            
            site.Name = input.Name;
            site.Location = input.Location;
            site.StartDate = input.StartDate;
            site.EndDate = input.EndDate;

            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization("RequireManagerOrAbove");

        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var site = await db.JobSites.FindAsync(id);
            if(site is null) return Results.NotFound();

            db.JobSites.Remove(site);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization("RequireAdmin");
    }
}