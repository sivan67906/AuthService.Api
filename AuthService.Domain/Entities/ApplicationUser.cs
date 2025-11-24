using Microsoft.AspNetCore.Identity;

namespace AuthService.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
    public ICollection<UserRefreshToken> RefreshTokens { get; set; } = new List<UserRefreshToken>();
}
