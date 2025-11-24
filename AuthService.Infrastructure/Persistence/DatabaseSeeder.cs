using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AuthService.Domain.Entities;

namespace AuthService.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    /// <summary>
    /// Converts a DateTime to UTC for PostgreSQL compatibility.
    /// PostgreSQL requires DateTime with DateTimeKind.Utc.
    /// </summary>
    private static DateTime ToUtc(DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Utc)
            return dateTime;

        if (dateTime.Kind == DateTimeKind.Unspecified)
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

        return dateTime.ToUniversalTime();
    }

    /// <summary>
    /// Converts a nullable DateTime to UTC for PostgreSQL compatibility.
    /// </summary>
    private static DateTime? ToUtcNullable(DateTime? dateTime)
    {
        return dateTime.HasValue ? ToUtc(dateTime.Value) : null;
    }

    /// <summary>
    /// Converts a nullable DateTimeOffset to UTC DateTime for PostgreSQL compatibility.
    /// </summary>
    private static DateTime? ToUtcFromOffset(DateTimeOffset? dateTimeOffset)
    {
        return dateTimeOffset?.UtcDateTime;
    }

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CommandDbContext>>();

        try
        {
            var commandContext = scope.ServiceProvider.GetRequiredService<CommandDbContext>();
            var queryContext = scope.ServiceProvider.GetRequiredService<QueryDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            logger.LogInformation("Starting database migration and seeding process");

            // Ensure databases are created and migrations are applied
            await commandContext.Database.MigrateAsync();
            await queryContext.Database.MigrateAsync();

            logger.LogInformation("Database migrations completed successfully");

            // Use comprehensive seeder for command database
            await ComprehensiveSeedData.SeedAllDataAsync(commandContext, userManager, roleManager);

            logger.LogInformation("Command database seeded with comprehensive data");

            // Synchronize to query database
            await SynchronizeToQueryDatabaseAsync(commandContext, queryContext, logger);

            logger.LogInformation("Database seeded and synchronized successfully with comprehensive data");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private static async Task SynchronizeToQueryDatabaseAsync(
        CommandDbContext commandContext,
        QueryDbContext queryContext,
        ILogger logger)
    {
        logger.LogInformation("Beginning query database synchronization");

        // Clear existing query database data in reverse dependency order
        // This ensures that child records are deleted before parent records to avoid foreign key violations
        logger.LogInformation("Clearing existing query database data");

        queryContext.PageFeatureMappings.RemoveRange(queryContext.PageFeatureMappings);
        queryContext.PagePermissionMappings.RemoveRange(queryContext.PagePermissionMappings);
        queryContext.RolePermissionMappings.RemoveRange(queryContext.RolePermissionMappings);
        queryContext.RoleDepartmentMappings.RemoveRange(queryContext.RoleDepartmentMappings);
        queryContext.UserRoleMappings.RemoveRange(queryContext.UserRoleMappings);
        queryContext.UserRoles.RemoveRange(queryContext.UserRoles);
        queryContext.RoleHierarchies.RemoveRange(queryContext.RoleHierarchies);
        queryContext.ApplicationRoles.RemoveRange(queryContext.ApplicationRoles);
        queryContext.ApplicationUsers.RemoveRange(queryContext.ApplicationUsers);
        queryContext.Pages.RemoveRange(queryContext.Pages);
        queryContext.Features.RemoveRange(queryContext.Features);
        queryContext.Permissions.RemoveRange(queryContext.Permissions);
        queryContext.Departments.RemoveRange(queryContext.Departments);

        await queryContext.SaveChangesAsync();
        logger.LogInformation("Query database cleared successfully");

        // Synchronize departments (no dependencies)
        logger.LogInformation("Synchronizing departments");
        var departments = await commandContext.Departments.AsNoTracking().ToListAsync();
        foreach (var dept in departments)
        {
            queryContext.Departments.Add(new Department
            {
                Id = dept.Id,
                Name = dept.Name,
                Description = dept.Description,
                IsActive = dept.IsActive,
                CreatedAt = ToUtc(dept.CreatedAt),
                UpdatedAt = ToUtcNullable(dept.UpdatedAt),
                IsDeleted = dept.IsDeleted
            });
        }
        await queryContext.SaveChangesAsync();
        logger.LogInformation($"Synchronized {departments.Count} departments");

        // Synchronize permissions (no dependencies)
        logger.LogInformation("Synchronizing permissions");
        var permissions = await commandContext.Permissions.AsNoTracking().ToListAsync();
        foreach (var perm in permissions)
        {
            queryContext.Permissions.Add(new Permission
            {
                Id = perm.Id,
                Name = perm.Name,
                Description = perm.Description,
                IsActive = perm.IsActive,
                CreatedAt = ToUtc(perm.CreatedAt),
                UpdatedAt = ToUtcNullable(perm.UpdatedAt),
                IsDeleted = perm.IsDeleted
            });
        }
        await queryContext.SaveChangesAsync();
        logger.LogInformation($"Synchronized {permissions.Count} permissions");

        // Synchronize features (self-referencing)
        logger.LogInformation("Synchronizing features");
        var features = await commandContext.Features.AsNoTracking().ToListAsync();
        foreach (var feat in features)
        {
            queryContext.Features.Add(new Feature
            {
                Id = feat.Id,
                Name = feat.Name,
                Description = feat.Description,
                IsMainMenu = feat.IsMainMenu,
                ParentFeatureId = feat.ParentFeatureId,
                DisplayOrder = feat.DisplayOrder,
                Icon = feat.Icon,
                IsActive = feat.IsActive,
                CreatedAt = ToUtc(feat.CreatedAt),
                UpdatedAt = ToUtcNullable(feat.UpdatedAt),
                IsDeleted = feat.IsDeleted
            });
        }
        await queryContext.SaveChangesAsync();
        logger.LogInformation($"Synchronized {features.Count} features");

        // Synchronize pages (no dependencies)
        logger.LogInformation("Synchronizing pages");
        var pages = await commandContext.Pages.AsNoTracking().ToListAsync();
        foreach (var page in pages)
        {
            queryContext.Pages.Add(new Page
            {
                Id = page.Id,
                Name = page.Name,
                Url = page.Url,
                Description = page.Description,
                DisplayOrder = page.DisplayOrder,
                IsActive = page.IsActive,
                CreatedAt = ToUtc(page.CreatedAt),
                UpdatedAt = ToUtcNullable(page.UpdatedAt),
                IsDeleted = page.IsDeleted
            });
        }
        await queryContext.SaveChangesAsync();
        logger.LogInformation($"Synchronized {pages.Count} pages");

        // Synchronize users (must come before UserRoleMappings and UserRoles)
        logger.LogInformation("Synchronizing application users");
        var users = await commandContext.Users.AsNoTracking().ToListAsync();
        foreach (var user in users)
        {
            queryContext.ApplicationUsers.Add(new ApplicationUser
            {
                Id = user.Id,
                UserName = user.UserName,
                NormalizedUserName = user.NormalizedUserName,
                Email = user.Email,
                NormalizedEmail = user.NormalizedEmail,
                EmailConfirmed = user.EmailConfirmed,
                PasswordHash = user.PasswordHash,
                SecurityStamp = user.SecurityStamp,
                ConcurrencyStamp = user.ConcurrencyStamp,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LockoutEnd = ToUtcFromOffset(user.LockoutEnd),
                LockoutEnabled = user.LockoutEnabled,
                AccessFailedCount = user.AccessFailedCount,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive
            });
        }
        await queryContext.SaveChangesAsync();
        logger.LogInformation($"Synchronized {users.Count} application users");

        // Synchronize roles (depends on departments)
        logger.LogInformation("Synchronizing application roles");
        var roles = await commandContext.Roles.AsNoTracking().ToListAsync();
        foreach (var role in roles)
        {
            queryContext.ApplicationRoles.Add(new ApplicationRole
            {
                Id = role.Id,
                Name = role.Name,
                NormalizedName = role.NormalizedName,
                Description = role.Description,
                DepartmentId = role.DepartmentId,
                ConcurrencyStamp = role.ConcurrencyStamp
            });
        }
        await queryContext.SaveChangesAsync();
        logger.LogInformation($"Synchronized {roles.Count} application roles");

        // Synchronize Identity user roles (depends on users and roles)
        logger.LogInformation("Synchronizing Identity user role assignments");
        var userRoles = await commandContext.UserRoles.AsNoTracking().ToListAsync();
        foreach (var userRole in userRoles)
        {
            queryContext.UserRoles.Add(new IdentityUserRole<Guid>
            {
                UserId = userRole.UserId,
                RoleId = userRole.RoleId
            });
        }
        await queryContext.SaveChangesAsync();
        logger.LogInformation($"Synchronized {userRoles.Count} Identity user role assignments");

        // Synchronize role hierarchies (depends on roles)
        logger.LogInformation("Synchronizing role hierarchies");
        var roleHierarchies = await commandContext.RoleHierarchies.AsNoTracking().ToListAsync();
        foreach (var rh in roleHierarchies)
        {
            queryContext.RoleHierarchies.Add(new RoleHierarchy
            {
                Id = rh.Id,
                ParentRoleId = rh.ParentRoleId,
                ChildRoleId = rh.ChildRoleId,
                Level = rh.Level,
                IsActive = rh.IsActive,
                CreatedAt = ToUtc(rh.CreatedAt),
                UpdatedAt = ToUtcNullable(rh.UpdatedAt),
                IsDeleted = rh.IsDeleted
            });
        }
        await queryContext.SaveChangesAsync();
        logger.LogInformation($"Synchronized {roleHierarchies.Count} role hierarchies");

        // Synchronize user role mappings (depends on users, roles, and departments)
        logger.LogInformation("Synchronizing user role mappings");
        var userRoleMappings = await commandContext.UserRoleMappings.AsNoTracking().ToListAsync();
        foreach (var urm in userRoleMappings)
        {
            queryContext.UserRoleMappings.Add(new UserRoleMapping
            {
                Id = urm.Id,
                UserId = urm.UserId,
                RoleId = urm.RoleId,
                DepartmentId = urm.DepartmentId,
                AssignedByEmail = urm.AssignedByEmail,
                AssignedAt = ToUtc(urm.AssignedAt),
                IsActive = urm.IsActive,
                CreatedAt = ToUtc(urm.CreatedAt),
                UpdatedAt = ToUtcNullable(urm.UpdatedAt),
                IsDeleted = urm.IsDeleted
            });
        }
        await queryContext.SaveChangesAsync();
        logger.LogInformation($"Synchronized {userRoleMappings.Count} user role mappings");

        // Synchronize role permission mappings (depends on roles and permissions)
        logger.LogInformation("Synchronizing role permission mappings");
        var rolePermMappings = await commandContext.RolePermissionMappings.AsNoTracking().ToListAsync();
        foreach (var rpm in rolePermMappings)
        {
            queryContext.RolePermissionMappings.Add(new RolePermissionMapping
            {
                Id = rpm.Id,
                RoleId = rpm.RoleId,
                PermissionId = rpm.PermissionId,
                IsActive = rpm.IsActive,
                CreatedAt = ToUtc(rpm.CreatedAt),
                UpdatedAt = ToUtcNullable(rpm.UpdatedAt),
                IsDeleted = rpm.IsDeleted
            });
        }
        await queryContext.SaveChangesAsync();
        logger.LogInformation($"Synchronized {rolePermMappings.Count} role permission mappings");

        // Synchronize role department mappings (depends on roles and departments)
        logger.LogInformation("Synchronizing role department mappings");
        var roleDeptMappings = await commandContext.RoleDepartmentMappings.AsNoTracking().ToListAsync();
        foreach (var rdm in roleDeptMappings)
        {
            queryContext.RoleDepartmentMappings.Add(new RoleDepartmentMapping
            {
                Id = rdm.Id,
                RoleId = rdm.RoleId,
                DepartmentId = rdm.DepartmentId,
                IsPrimary = rdm.IsPrimary,
                IsActive = rdm.IsActive,
                CreatedAt = ToUtc(rdm.CreatedAt),
                UpdatedAt = ToUtcNullable(rdm.UpdatedAt),
                IsDeleted = rdm.IsDeleted
            });
        }
        await queryContext.SaveChangesAsync();
        logger.LogInformation($"Synchronized {roleDeptMappings.Count} role department mappings");

        // Synchronize page permission mappings (depends on pages and permissions)
        logger.LogInformation("Synchronizing page permission mappings");
        var pagePermMappings = await commandContext.PagePermissionMappings.AsNoTracking().ToListAsync();
        foreach (var ppm in pagePermMappings)
        {
            queryContext.PagePermissionMappings.Add(new PagePermissionMapping
            {
                Id = ppm.Id,
                PageId = ppm.PageId,
                PermissionId = ppm.PermissionId,
                IsActive = ppm.IsActive,
                CreatedAt = ToUtc(ppm.CreatedAt),
                UpdatedAt = ToUtcNullable(ppm.UpdatedAt),
                IsDeleted = ppm.IsDeleted
            });
        }
        await queryContext.SaveChangesAsync();
        logger.LogInformation($"Synchronized {pagePermMappings.Count} page permission mappings");

        // Synchronize page feature mappings (depends on pages and features)
        logger.LogInformation("Synchronizing page feature mappings");
        var pageFeatureMappings = await commandContext.PageFeatureMappings.AsNoTracking().ToListAsync();
        foreach (var pfm in pageFeatureMappings)
        {
            queryContext.PageFeatureMappings.Add(new PageFeatureMapping
            {
                Id = pfm.Id,
                PageId = pfm.PageId,
                FeatureId = pfm.FeatureId,
                IsActive = pfm.IsActive,
                CreatedAt = ToUtc(pfm.CreatedAt),
                UpdatedAt = ToUtcNullable(pfm.UpdatedAt),
                IsDeleted = pfm.IsDeleted
            });
        }
        await queryContext.SaveChangesAsync();
        logger.LogInformation($"Synchronized {pageFeatureMappings.Count} page feature mappings");

        logger.LogInformation("Query database synchronization completed successfully");
    }
}