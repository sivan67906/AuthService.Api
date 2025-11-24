namespace AuthService.Domain.Entities;

public sealed class Department : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; init; } = true;

    // Navigation properties
    public ICollection<ApplicationRole> Roles { get; init; } = [];
}
