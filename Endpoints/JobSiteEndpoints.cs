using ConstructionAssetAPI.Services;
using FluentValidation;

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
        var group = app.MapGroup("/api/v1/jobsites")
            .WithTags("JobSites")
            .RequireAuthorization("RequireAnyAuthenticatedUser");

        group.MapGet("/", async (JobSiteService svc) =>
            Results.Ok(await svc.GetAllAsync()));

        group.MapGet("/{id:int}", async (int id, JobSiteService svc) =>
            Results.Ok(await svc.GetByIdAsync(id)));

        group.MapPost("/", async (
            JobSiteInput input,
            IValidator<JobSiteInput> validator,
            JobSiteService svc) =>
        {
            var validation = await validator.ValidateAsync(input);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var created = await svc.CreateAsync(input);
            return Results.Created($"/api/v1/jobsites/{created.Id}", created);
        }).RequireAuthorization("RequireManagerOrAbove");

        group.MapPut("/{id:int}", async (
            int id,
            JobSiteInput input,
            IValidator<JobSiteInput> validator,
            JobSiteService svc) =>
        {
            var validation = await validator.ValidateAsync(input);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            await svc.UpdateAsync(id, input);
            return Results.NoContent();
        }).RequireAuthorization("RequireManagerOrAbove");

        group.MapDelete("/{id:int}", async (int id, JobSiteService svc) =>
        {
            await svc.DeleteAsync(id);
            return Results.NoContent();
        }).RequireAuthorization("RequireAdmin");
    }
}