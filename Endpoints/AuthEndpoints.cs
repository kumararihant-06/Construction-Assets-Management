using ConstructionAssetAPI.Enums;
using ConstructionAssetAPI.Services;

namespace ConstructionAssetAPI.Endpoints;

public record RegisterRequest(string Email, string Password, string FullName, UserRole Role);
public record LoginRequest(string Email, string Password);

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/auth").WithTags("Auth");

        group.MapPost("/login", async (LoginRequest req, AuthService authService) =>
        {
            var result = await authService.LoginAsync(req.Email, req.Password);
            return Results.Ok(result);
        });

        group.MapPost("/register", async (RegisterRequest req, AuthService authService) =>
        {
            var result = await authService.RegisterAsync(req.Email, req.Password, req.FullName, req.Role);
            return Results.Ok(result);
        }).RequireAuthorization("RequireAdmin");
    }
}