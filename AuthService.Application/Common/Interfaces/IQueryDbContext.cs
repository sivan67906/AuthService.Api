using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Domain.Interfaces;

public interface IQueryDbContext
{
    DbSet<ApplicationUser> ApplicationUsers { get; }
    DbSet<ApplicationRole> ApplicationRoles { get; }
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
    DbSet<IdentityUserRole<Guid>> UserRoles { get; }
    DbSet<UserAddress> UserAddresses { get; }
    DbSet<UserRefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}