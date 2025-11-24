using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Common.Interfaces;

public interface ICommandDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<ApplicationRole> Roles { get; }
    DbSet<Department> Departments { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<Feature> Features { get; }
    DbSet<Page> Pages { get; }
    DbSet<RolePermissionMapping> RolePermissionMappings { get; }
    DbSet<PagePermissionMapping> PagePermissionMappings { get; }
    DbSet<PageFeatureMapping> PageFeatureMappings { get; }
    DbSet<RoleHierarchy> RoleHierarchies { get; }
    DbSet<UserRoleMapping> UserRoleMappings { get; }
    DbSet<RoleDepartmentMapping> RoleDepartmentMappings { get; }

    DbSet<T> Set<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
