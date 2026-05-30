using ConstructionAssetAPI.Enums;
using ConstructionAssetAPI.Services;
using FluentValidation;

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
        group.MapGet("/", async (EquipmentService svc) => Results.Ok(await svc.GetAllAsync()));

        // GET /equipment/{id}
        group.MapGet("/{id:int}", async (int id, EquipmentService svc) => Results.Ok(await svc.GetByIdAsync(id)) );
        
        // POST /equipment
        group.MapPost("/", async (EquipmentInput input, IValidator<EquipmentInput> validator, EquipmentService svc) =>
        {
            var validation = await validator.ValidateAsync(input);
            if(!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

            var created = await svc.CreateAsync(input);
            return Results.Created($"/api/v1/equipments/{created.Id}", created);
        }).RequireAuthorization("RequireManagerOrAbove");

        //PUT /equipment/{id}
        group.MapPut("/{id:int}", async (int id, EquipmentInput input, IValidator<EquipmentInput> validator, EquipmentService svc) =>
        {
            var validation = await validator.ValidateAsync(input);
            if(!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            await svc.UpdateAsync(id, input);
            return Results.NoContent();
        }).RequireAuthorization("RequireManagerOrAbove");

        // DELETE /equipment/{id}
        group.MapDelete("{id:int}", async (int id, EquipmentService svc) =>
        {
            await svc.DeleteAsync(id);
            return Results.NoContent();
        }).RequireAuthorization("RequireAdmin");

        // GET /equipment/available — only currently-available equipment.
        // Operationally: "what can I assign to a site right now?"
        group.MapGet("/available", async (EquipmentService svc) =>
            Results.Ok(await svc.GetAvailableAsync()));
        
        group.MapGet("/maintenance-due", async (EquipmentService svc) =>
        Results.Ok(await svc.GetMaintenanceDueAsync()))
        .RequireAuthorization("RequireManagerOrAbove");

        group.MapPatch("/{id:int}/status", async (int id, UpdateStatusInput input, EquipmentService svc) =>
        {
            await svc.UpdateStatusAsync(id, input.Status);
            return Results.NoContent();
        });


    }       
}