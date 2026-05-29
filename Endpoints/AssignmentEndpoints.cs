using ConstructionAssetAPI.Data;
using ConstructionAssetAPI.Entities;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using ConstructionAssetAPI.Enums;
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

        
        // POST /assignments — create with FK checks, double-booking detection,
        // and equipment-status update (status flip lands in Step 4).
        group.MapPost("/", async (
            AssignmentInput input,
            IValidator<AssignmentInput> validator,
            AppDbContext db) =>
        {
            // 1. Shape validation
            var validation = await validator.ValidateAsync(input);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            // 2. Foreign key existence
            var equipment = await db.Equipment.FindAsync(input.EquipmentId);
            if (equipment is null)
                return Results.BadRequest(new { error = $"Equipment {input.EquipmentId} does not exist." });

            var jobSite = await db.JobSites.FindAsync(input.JobSiteId);
            if (jobSite is null)
                return Results.BadRequest(new { error = $"JobSite {input.JobSiteId} does not exist." });

            // 3. Double-booking detection — open assignment = ReturnDate IS NULL
            var openAssignment = await db.Assignments
                .Include(a => a.JobSite)
                .FirstOrDefaultAsync(a =>
                    a.EquipmentId == input.EquipmentId &&
                    a.ReturnDate == null);

            if (openAssignment is not null)
            {
                return Results.Conflict(new
                {
                    error = "EquipmentAlreadyAssigned",
                    message = $"Equipment '{equipment.Name}' (#{equipment.SerialNumber}) " +
                      $"is already assigned to site '{openAssignment.JobSite.Name}'.",
                    currentAssignmentId = openAssignment.Id
                });
            }

            // 4. Create
            var assignment = new Assignment
            {
                EquipmentId = input.EquipmentId,
                JobSiteId = input.JobSiteId,
                AssignedDate = input.AssignedDate,
                Notes = input.Notes
            };

            equipment.Status = EquipmentStatus.InUse;

            db.Assignments.Add(assignment);
            await db.SaveChangesAsync();

            var response = new
            {
                assignment.Id,
                assignment.EquipmentId,
                assignment.JobSiteId,
                assignment.AssignedDate,
                assignment.ReturnDate,
                assignment.Notes
            };

            return Results.Created($"/assignments/{assignment.Id}", response);
        });

        // DELETE /assignments/{id} — soft return:
        // mark ReturnDate = now and flip equipment status back to Available.
        // We keep the row instead of deleting it so we can show site history later.
        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var assignment = await db.Assignments
                .Include(a => a.Equipment)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment is null) return Results.NotFound();
            if (assignment.ReturnDate is not null)
                return Results.BadRequest(new { error = "Assignment already returned." });

            assignment.ReturnDate = DateTime.UtcNow;
            assignment.Equipment.Status = EquipmentStatus.Available;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}