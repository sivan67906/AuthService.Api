using Microsoft.AspNetCore.Identity;

namespace AuthService.Domain.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public Guid? DepartmentId { get; set; }

    // Navigation properties
    public Department? Department { get; set; }
    public ICollection<RolePermissionMapping> RolePermissions { get; init; } = [];
}
