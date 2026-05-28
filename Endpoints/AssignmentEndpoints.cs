using ConstructionAssetAPI.Data;
using ConstructionAssetAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ConstructionAssetAPI.Endpoints;
public record AssignmentInput
(
    int EquipmentId,
    int JobSiteId,
    DateTime AssignedDate,
    string? Notes
);

public static class AssignmentEndpoints
{
    public static void MapAssignmentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/assignments").WithTags("Assignments");

        // GET /assignments - lists all assignments with equipments and sites.
        group.MapGet("/", async(AppDbContext db) => 
            await db.Assignments
                .Include(a => a.Equipment)
                .Include(a => a.JobSite)
                .Select(a => new
                {
                    a.Id,
                    a.EquipmentId,
                    EquipmentName = a.Equipment.Name,
                    a.JobSiteId,
                    JobSiteName = a.JobSite.Name,
                    a.AssignedDate,
                    a.ReturnDate,
                    a.Notes
                })
                .ToListAsync());

        // POST /assignments - create new assignment
        group.MapPost("/", async (AssignmentInput input, AppDbContext db) =>
        {
            var assignment = new Assignment
            {
                EquipmentId = input.EquipmentId,
                JobSiteId = input.JobSiteId,
                AssignedDate = input.AssignedDate,
                Notes = input.Notes
            };

            db.Assignments.Add(assignment);
            await db.SaveChangesAsync();

            return Results.Created($"/assignments/{assignment.Id}", assignment);
        });

        // DELETE /assignments/{id} — equipment is returned, remove the record
        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var assignment = await db.Assignments.FindAsync(id);
            if (assignment is null) return Results.NotFound();

            db.Assignments.Remove(assignment);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}