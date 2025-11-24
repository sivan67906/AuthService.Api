namespace AuthService.Domain.Entities;

public sealed class RolePermissionMapping : BaseEntity
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public bool IsActive { get; init; } = true;

    // Navigation properties
    public ApplicationRole Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}