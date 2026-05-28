using ConstructionAssetAPI.Data;
using ConstructionAssetAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ConstructionAssetAPI.Endpoints;

//Input DTO - What the client sends on POST/PUT

public record EquipmentInput
(
    string Name,
    string Type,
    string SerialNumber,
    string Status,
    DateTime? NextMaintenanceDate
);

public static class EquipmentEndpoints
{
    public static void MapEquipmentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/equipment").WithTags("Equipment");

        //GET /equipment 
        group.MapGet("/", async(AppDbContext db) => await db.Equipment.ToListAsync());

        // GET /equipment/{id}
        group.MapGet("/{id:int}", async (int id, AppDbContext db) => 
            await db.Equipment.FindAsync(id) is Equipment eq
                ? Results.Ok(eq) : Results.NotFound());
        
        // POST /equipment
        group.MapPost("/", async (EquipmentInput input, AppDbContext db) =>
        {
            var equipment = new Equipment
            {
                Name = input.Name,
                Type = input.Type,
                SerialNumber = input.SerialNumber,
                Status = input.Status,
                NextMaintenanceDate = input.NextMaintenanceDate
            };

            db.Equipment.Add(equipment);
            await db.SaveChangesAsync();

            return Results.Created($"/equipment/{equipment.Id}", equipment);
        });

        //PUT /equipment/{id}
        group.MapPut("/{id:int}", async (int id, EquipmentInput input, AppDbContext db) =>
        {
            var equipment = await db.Equipment.FindAsync(id);
            if(equipment is null) return Results.NotFound();

            equipment.Name = input.Name;
            equipment.Type = input.Type;
            equipment.SerialNumber = input.SerialNumber;
            equipment.Status = input.Status;
            equipment.NextMaintenanceDate = input.NextMaintenanceDate;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // DELETE /equipment/{id}
        group.MapDelete("{id:int}", async (int id, AppDbContext db) =>
        {
            var equipment = await db.Equipment.FindAsync(id);
            if(equipment is null) return Results.NotFound();

            db.Equipment.Remove(equipment);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}