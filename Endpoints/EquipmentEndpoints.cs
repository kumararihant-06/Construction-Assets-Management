using System.ComponentModel.DataAnnotations;
using ConstructionAssetAPI.Data;
using ConstructionAssetAPI.Entities;
using ConstructionAssetAPI.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ConstructionAssetAPI.Endpoints;

//Input DTO - What the client sends on POST/PUT

public record EquipmentInput
(
    string Name,
    string Type,
    string SerialNumber,
    EquipmentStatus Status,
    DateTime? NextMaintenanceDate
);

public record UpdateStatusInput(EquipmentStatus Status);

public static class EquipmentEndpoints
{
    public static void MapEquipmentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/equipments").WithTags("Equipment").RequireAuthorization("RequireAnyAuthenticatedUser");

        //GET /equipment 
        group.MapGet("/", async(AppDbContext db) => await db.Equipment.ToListAsync());

        // GET /equipment/{id}
        group.MapGet("/{id:int}", async (int id, AppDbContext db) => 
            await db.Equipment.FindAsync(id) is Equipment eq
                ? Results.Ok(eq) : Results.NotFound());
        
        // POST /equipment
        group.MapPost("/", async (EquipmentInput input, IValidator<EquipmentInput> validator, AppDbContext db) =>
        {
            var validation = await validator.ValidateAsync(input);
            if(!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

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

            return Results.Created($"/api/v1/equipments/{equipment.Id}", equipment);
        }).RequireAuthorization("RequireManagerOrAbove");

        //PUT /equipment/{id}
        group.MapPut("/{id:int}", async (int id, EquipmentInput input, IValidator<EquipmentInput> validator, AppDbContext db) =>
        {
            var validation = await validator.ValidateAsync(input);
            if(!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var equipment = await db.Equipment.FindAsync(id);
            if(equipment is null) return Results.NotFound();

            equipment.Name = input.Name;
            equipment.Type = input.Type;
            equipment.SerialNumber = input.SerialNumber;
            equipment.Status = input.Status;
            equipment.NextMaintenanceDate = input.NextMaintenanceDate;

            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization("RequireManagerOrAbove");

        // DELETE /equipment/{id}
        group.MapDelete("{id:int}", async (int id, AppDbContext db) =>
        {
            var equipment = await db.Equipment.FindAsync(id);
            if(equipment is null) return Results.NotFound();

            db.Equipment.Remove(equipment);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization("RequireAdmin");

        // GET /equipment/available — only currently-available equipment.
        // Operationally: "what can I assign to a site right now?"
        group.MapGet("/available", async (AppDbContext db) =>
            await db.Equipment
                .Where(e => e.Status == EquipmentStatus.Available)
                .ToListAsync());
        
        group.MapGet("/maintenance-due", async (AppDbContext db) =>
        await db.Equipment
            .Where(e => e.NextMaintenanceDate <= DateTime.UtcNow.AddDays(7))
            .ToListAsync())
        .RequireAuthorization("RequireManagerOrAbove");

        group.MapPatch("/{id:int}/status", async (int id, UpdateStatusInput input, AppDbContext db) =>
        {
            var equipment = await db.Equipment.FindAsync(id);
            if (equipment is null) return Results.NotFound();

            equipment.Status = input.Status;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });


    }       
}