namespace AuthService.Domain.Entities;

public sealed class RoleHierarchy : BaseEntity
{
    public Guid ParentRoleId { get; set; }
    public Guid ChildRoleId { get; set; }
    public int Level { get; set; }
    public bool IsActive { get; init; } = true;

    // Navigation properties
    public ApplicationRole ParentRole { get; set; } = null!;
    public ApplicationRole ChildRole { get; set; } = null!;
}
