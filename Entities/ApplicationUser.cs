using Microsoft.AspNetCore.Identity;

namespace ConstructionAssetAPI.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
}