using ConstructionAssetAPI.Auth;
using ConstructionAssetAPI.Entities;
using ConstructionAssetAPI.Enums;
using ConstructionAssetAPI.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace ConstructionAssetAPI.Services;

public class AuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtTokenGenerator _tokenGenerator;

    public AuthService(UserManager<ApplicationUser> userManager, JwtTokenGenerator tokenGenerator)
    {
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<object> RegisterAsync(string email, string password, string fullName, UserRole role)
    {
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
            throw new ConflictException($"User with email {email} already exists.");

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, role.ToString());

        return new { message = $"User {email} registered successfully with role {role}." };
    }

    public async Task<object> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, password))
            throw new ForbiddenException("Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _tokenGenerator.GenerateToken(user, roles);

        return new
        {
            accessToken = token,
            expiresAt,
            role = roles.FirstOrDefault()
        };
    }
}