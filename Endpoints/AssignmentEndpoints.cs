using ConstructionAssetAPI.Services;
using FluentValidation;

namespace ConstructionAssetAPI.Endpoints;

public record AssignmentInput
(
    int EquipmentId,
    int JobSiteId,
    DateTime AssignedDate,
    string? Notes
);

public record AssignmentResponse
(
    int Id,
    int EquipmentId,
    string EquipmentName,
    int JobSiteId,
    string JobSiteName,
    DateTime AssignedDate,
    DateTime? ReturnDate,
    string? Notes
);

public static class AssignmentEndpoints
{
    public static void MapAssignmentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/assignments")
            .WithTags("Assignments")
            .RequireAuthorization("RequireAnyAuthenticatedUser");

        group.MapGet("/", async (AssignmentService svc) =>
            Results.Ok(await svc.GetAllAsync()));

        group.MapPost("/", async (
            AssignmentInput input,
            IValidator<AssignmentInput> validator,
            AssignmentService svc) =>
        {
            var validation = await validator.ValidateAsync(input);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var created = await svc.CreateAsync(input);
            return Results.Created($"/api/v1/assignments/{created.Id}", created);
        }).RequireAuthorization("RequireManagerOrAbove");

        group.MapDelete("/{id:int}", async (int id, AssignmentService svc) =>
        {
            await svc.ReturnAsync(id);
            return Results.NoContent();
        }).RequireAuthorization("RequireManagerOrAbove");
    }
}