using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Persistence;

/// <summary>
/// Comprehensive seed data implementation for RBAC system
/// Implements the complete hierarchy with SuperAdmin, Department-based roles, and full permission mapping
/// </summary>
public static class ComprehensiveSeedData
{
    public static async Task SeedAllDataAsync(
        CommandDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        // Clear existing data
        await ClearExistingDataAsync(context, userManager, roleManager);

        // Seed in proper order as per requirements
        var departments = await SeedDepartmentsAsync(context);
        var roles = await SeedRolesAsync(roleManager, departments);
        var permissions = await SeedPermissionsAsync(context);
        var features = await SeedFeaturesAsync(context);
        var pages = await SeedPagesAsync(context);
        
        // Mappings
        await SeedPageFeatureMappingsAsync(context, features, pages);
        await SeedPagePermissionMappingsAsync(context, permissions, pages);
        await SeedRolePermissionMappingsAsync(context, roles, permissions);
        await SeedRoleDepartmentMappingsAsync(context, roles, departments);
        await SeedRoleHierarchyAsync(context, roles);
        
        // Users and their role assignments
        await SeedUsersAsync(userManager, context, departments, roles);
    }

    private static async Task ClearExistingDataAsync(
        CommandDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        // Remove mappings first (in dependency order)
        context.PageFeatureMappings.RemoveRange(context.PageFeatureMappings);
        context.PagePermissionMappings.RemoveRange(context.PagePermissionMappings);
        context.RolePermissionMappings.RemoveRange(context.RolePermissionMappings);
        context.RoleDepartmentMappings.RemoveRange(context.RoleDepartmentMappings);
        await context.SaveChangesAsync();

        context.UserRoleMappings.RemoveRange(context.UserRoleMappings);
        await context.SaveChangesAsync();

        context.RoleHierarchies.RemoveRange(context.RoleHierarchies);
        await context.SaveChangesAsync();

        // Remove users
        var users = await userManager.Users.ToListAsync();
        foreach (var user in users)
        {
            await userManager.DeleteAsync(user);
        }

        // Remove roles
        var roles = await roleManager.Roles.ToListAsync();
        foreach (var role in roles)
        {
            await roleManager.DeleteAsync(role);
        }

        // Remove entities
        context.Pages.RemoveRange(context.Pages);
        await context.SaveChangesAsync();

        context.Features.RemoveRange(context.Features);
        await context.SaveChangesAsync();

        context.Permissions.RemoveRange(context.Permissions);
        await context.SaveChangesAsync();

        context.Departments.RemoveRange(context.Departments);
        await context.SaveChangesAsync();
    }

    private static async Task<Dictionary<string, Guid>> SeedDepartmentsAsync(CommandDbContext context)
    {
        var departments = new Dictionary<string, Guid>();

        var deptList = new List<(string Name, string Description)>
        {
            ("Finance", "Finance Department - Manages financial operations, budgeting, accounting, and financial reporting"),
            ("Human Resources", "Human Resources Department - Manages employee relations, recruitment, training, payroll, and HR policies")
        };

        foreach (var (name, desc) in deptList)
        {
            var deptId = Guid.NewGuid();
            departments[name] = deptId;

            context.Departments.Add(new Department
            {
                Id = deptId,
                Name = name,
                Description = desc,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        }

        await context.SaveChangesAsync();
        return departments;
    }

    private static async Task<Dictionary<string, Guid>> SeedRolesAsync(
        RoleManager<ApplicationRole> roleManager,
        Dictionary<string, Guid> departments)
    {
        var roles = new Dictionary<string, Guid>();

        // SuperAdmin - no department
        var superAdminId = Guid.NewGuid();
        roles["SuperAdmin"] = superAdminId;
        await roleManager.CreateAsync(new ApplicationRole
        {
            Id = superAdminId,
            Name = "SuperAdmin",
            Description = "Super Administrator with full system access",
            DepartmentId = null,
            NormalizedName = "SUPERADMIN"
        });

        // Finance Department Roles
        var financeRoles = new List<(string Name, string Description, Guid DeptId)>
        {
            ("FinanceAdmin", "Finance Department Administrator", departments["Finance"]),
            ("FinanceManager", "Finance Department Manager", departments["Finance"]),
            ("FinanceAccountant", "Finance Department Accountant", departments["Finance"]),
            ("FinanceAudit", "Finance Department Auditor", departments["Finance"]),
            ("FinanceStaff", "Finance Department Staff", departments["Finance"])
        };

        foreach (var (name, desc, deptId) in financeRoles)
        {
            var roleId = Guid.NewGuid();
            roles[name] = roleId;
            await roleManager.CreateAsync(new ApplicationRole
            {
                Id = roleId,
                Name = name,
                Description = desc,
                DepartmentId = deptId,
                NormalizedName = name.ToUpperInvariant()
            });
        }

        // HR Department Roles
        var hrRoles = new List<(string Name, string Description, Guid DeptId)>
        {
            ("HRAdmin", "Human Resources Department Administrator", departments["Human Resources"]),
            ("HRManager", "Human Resources Department Manager", departments["Human Resources"]),
            ("HRAccountant", "Human Resources Department Accountant", departments["Human Resources"]),
            ("HRAudit", "Human Resources Department Auditor", departments["Human Resources"]),
            ("HRStaff", "Human Resources Department Staff", departments["Human Resources"])
        };

        foreach (var (name, desc, deptId) in hrRoles)
        {
            var roleId = Guid.NewGuid();
            roles[name] = roleId;
            await roleManager.CreateAsync(new ApplicationRole
            {
                Id = roleId,
                Name = name,
                Description = desc,
                DepartmentId = deptId,
                NormalizedName = name.ToUpperInvariant()
            });
        }

        return roles;
    }

    private static async Task<Dictionary<string, Guid>> SeedPermissionsAsync(CommandDbContext context)
    {
        var permissions = new Dictionary<string, Guid>();

        var permList = new List<(string Name, string Description)>
        {
            ("Create", "Permission to create new records"),
            ("Edit", "Permission to modify existing records"),
            ("Delete", "Permission to delete records"),
            ("View", "Permission to view and read data")
        };

        foreach (var (name, desc) in permList)
        {
            var permId = Guid.NewGuid();
            permissions[name] = permId;

            context.Permissions.Add(new Permission
            {
                Id = permId,
                Name = name,
                Description = desc,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        }

        await context.SaveChangesAsync();
        return permissions;
    }

    private static async Task<Dictionary<string, Guid>> SeedFeaturesAsync(CommandDbContext context)
    {
        var features = new Dictionary<string, Guid>();

        // Main Menu 1: Module Management
        var moduleManagementId = Guid.NewGuid();
        features["ModuleManagement"] = moduleManagementId;
        context.Features.Add(new Feature
        {
            Id = moduleManagementId,
            Name = "Module Management",
            Description = "Main menu for managing core system modules",
            IsMainMenu = true,
            ParentFeatureId = null,
            DisplayOrder = 1,
            Icon = "fa-cubes",
            IsActive = true,
            RouteUrl = null,
            Level = 0,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        });

        // Main Menu 2: Mapping Management
        var mappingManagementId = Guid.NewGuid();
        features["MappingManagement"] = mappingManagementId;
        context.Features.Add(new Feature
        {
            Id = mappingManagementId,
            Name = "Mapping Management",
            Description = "Main menu for managing system mappings",
            IsMainMenu = true,
            ParentFeatureId = null,
            DisplayOrder = 2,
            Icon = "fa-sitemap",
            IsActive = true,
            RouteUrl = null,
            Level = 0,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        });

        await context.SaveChangesAsync();

        // Sub Menu 1: UserRole Management (under Mapping Management)
        var userRoleManagementId = Guid.NewGuid();
        features["UserRoleManagement"] = userRoleManagementId;
        context.Features.Add(new Feature
        {
            Id = userRoleManagementId,
            Name = "UserRole Management",
            Description = "Sub menu for user role related mappings",
            IsMainMenu = false,
            ParentFeatureId = mappingManagementId,
            DisplayOrder = 1,
            Icon = "fa-users-cog",
            IsActive = true,
            RouteUrl = null,
            Level = 1,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        });

        // Sub Menu 2: RoleAccess Management (under Mapping Management)
        var roleAccessManagementId = Guid.NewGuid();
        features["RoleAccessManagement"] = roleAccessManagementId;
        context.Features.Add(new Feature
        {
            Id = roleAccessManagementId,
            Name = "RoleAccess Management",
            Description = "Sub menu for role access and permission mappings",
            IsMainMenu = false,
            ParentFeatureId = mappingManagementId,
            DisplayOrder = 2,
            Icon = "fa-lock",
            IsActive = true,
            RouteUrl = null,
            Level = 1,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        });

        await context.SaveChangesAsync();
        return features;
    }

    private static async Task<Dictionary<string, Guid>> SeedPagesAsync(CommandDbContext context)
    {
        var pages = new Dictionary<string, Guid>();

        // Pages under Module Management main menu
        var modulePages = new List<(string Name, string Url, string Description, string MenuContext, string ApiEndpoint)>
        {
            ("Department", "/department", "Department management page", "ModuleManagement", "/api/department"),
            ("Role", "/role", "Role management page", "ModuleManagement", "/api/role"),
            ("Feature", "/feature", "Feature management page", "ModuleManagement", "/api/feature"),
            ("Page", "/page", "Page management page", "ModuleManagement", "/api/page"),
            ("Permission", "/permission", "Permission management page", "ModuleManagement", "/api/permission")
        };

        int order = 1;
        foreach (var (name, url, desc, menuContext, apiEndpoint) in modulePages)
        {
            var pageId = Guid.NewGuid();
            pages[name] = pageId;

            context.Pages.Add(new Page
            {
                Id = pageId,
                Name = name,
                Url = url,
                Description = desc,
                IsActive = true,
                DisplayOrder = order++,
                MenuContext = menuContext,
                ApiEndpoint = apiEndpoint,
                HttpMethod = "GET",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        }

        // Pages under UserRole Management submenu
        var userRolePages = new List<(string Name, string Url, string Description, string MenuContext, string ApiEndpoint)>
        {
            ("UserRoleMapping", "/userrolemapping", "User role mapping management page", "MappingManagement->UserRoleManagement", "/api/userrolemapping"),
            ("RoleDepartmentMapping", "/roledepartmentmapping", "Role department mapping management page", "MappingManagement->UserRoleManagement", "/api/roledepartmentmapping"),
            ("RoleHierarchyMapping", "/rolehierarchymapping", "Role hierarchy mapping management page", "MappingManagement->UserRoleManagement", "/api/rolehierarchymapping")
        };

        order = 1;
        foreach (var (name, url, desc, menuContext, apiEndpoint) in userRolePages)
        {
            var pageId = Guid.NewGuid();
            pages[name] = pageId;

            context.Pages.Add(new Page
            {
                Id = pageId,
                Name = name,
                Url = url,
                Description = desc,
                IsActive = true,
                DisplayOrder = order++,
                MenuContext = menuContext,
                ApiEndpoint = apiEndpoint,
                HttpMethod = "GET",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        }

        // Pages under RoleAccess Management submenu
        var roleAccessPages = new List<(string Name, string Url, string Description, string MenuContext, string ApiEndpoint)>
        {
            ("RolePermissionMapping", "/rolepermissionmapping", "Role permission mapping management page", "MappingManagement->RoleAccessManagement", "/api/rolepermissionmapping"),
            ("PagePermissionMapping", "/pagepermissionmapping", "Page permission mapping management page", "MappingManagement->RoleAccessManagement", "/api/pagepermissionmapping"),
            ("PageFeatureMapping", "/pagefeaturemapping", "Page feature mapping management page", "MappingManagement->RoleAccessManagement", "/api/pagefeaturemapping")
        };

        order = 1;
        foreach (var (name, url, desc, menuContext, apiEndpoint) in roleAccessPages)
        {
            var pageId = Guid.NewGuid();
            pages[name] = pageId;

            context.Pages.Add(new Page
            {
                Id = pageId,
                Name = name,
                Url = url,
                Description = desc,
                IsActive = true,
                DisplayOrder = order++,
                MenuContext = menuContext,
                ApiEndpoint = apiEndpoint,
                HttpMethod = "GET",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        }

        await context.SaveChangesAsync();
        return pages;
    }

    private static async Task SeedPageFeatureMappingsAsync(
        CommandDbContext context,
        Dictionary<string, Guid> features,
        Dictionary<string, Guid> pages)
    {
        // Map Module Management pages to Module Management feature
        var moduleManagementPages = new[] { "Department", "Role", "Feature", "Page", "Permission" };
        foreach (var pageName in moduleManagementPages)
        {
            context.PageFeatureMappings.Add(new PageFeatureMapping
            {
                Id = Guid.NewGuid(),
                PageId = pages[pageName],
                FeatureId = features["ModuleManagement"],
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        }

        // Map UserRole Management pages to UserRole Management feature
        var userRolePages = new[] { "UserRoleMapping", "RoleDepartmentMapping", "RoleHierarchyMapping" };
        foreach (var pageName in userRolePages)
        {
            context.PageFeatureMappings.Add(new PageFeatureMapping
            {
                Id = Guid.NewGuid(),
                PageId = pages[pageName],
                FeatureId = features["UserRoleManagement"],
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        }

        // Map RoleAccess Management pages to RoleAccess Management feature
        var roleAccessPages = new[] { "RolePermissionMapping", "PagePermissionMapping", "PageFeatureMapping" };
        foreach (var pageName in roleAccessPages)
        {
            context.PageFeatureMappings.Add(new PageFeatureMapping
            {
                Id = Guid.NewGuid(),
                PageId = pages[pageName],
                FeatureId = features["RoleAccessManagement"],
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedPagePermissionMappingsAsync(
        CommandDbContext context,
        Dictionary<string, Guid> permissions,
        Dictionary<string, Guid> pages)
    {
        // All pages get all 4 permissions (Create, Edit, Delete, View)
        foreach (var page in pages)
        {
            foreach (var permission in permissions)
            {
                context.PagePermissionMappings.Add(new PagePermissionMapping
                {
                    Id = Guid.NewGuid(),
                    PageId = page.Value,
                    PermissionId = permission.Value,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedRolePermissionMappingsAsync(
        CommandDbContext context,
        Dictionary<string, Guid> roles,
        Dictionary<string, Guid> permissions)
    {
        // SuperAdmin gets all permissions
        foreach (var permission in permissions)
        {
            context.RolePermissionMappings.Add(new RolePermissionMapping
            {
                Id = Guid.NewGuid(),
                RoleId = roles["SuperAdmin"],
                PermissionId = permission.Value,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        }

        // FinanceAdmin gets all permissions
        foreach (var permission in permissions)
        {
            context.RolePermissionMappings.Add(new RolePermissionMapping
            {
                Id = Guid.NewGuid(),
                RoleId = roles["FinanceAdmin"],
                PermissionId = permission.Value,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        }

        // HRAdmin gets all permissions
        foreach (var permission in permissions)
        {
            context.RolePermissionMappings.Add(new RolePermissionMapping
            {
                Id = Guid.NewGuid(),
                RoleId = roles["HRAdmin"],
                PermissionId = permission.Value,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        }

        // FinanceManager gets View, Create, Edit
        var managerPermissions = new[] { "View", "Create", "Edit" };
        foreach (var perm in managerPermissions)
        {
            context.RolePermissionMappings.Add(new RolePermissionMapping
            {
                Id = Guid.NewGuid(),
                RoleId = roles["FinanceManager"],
                PermissionId = permissions[perm],
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        }

        // HR-Manager gets View, Create, Edit
        foreach (var perm in managerPermissions)
        {
            context.RolePermissionMappings.Add(new RolePermissionMapping
            {
                Id = Guid.NewGuid(),
                RoleId = roles["HRManager"],
                PermissionId = permissions[perm],
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        }

        // Finance-Accountant gets View, Create
        var accountantPermissions = new[] { "View", "Create" };
        foreach (var perm in accountantPermissions)
        {
            context.RolePermissionMappings.Add(new RolePermissionMapping
            {
                Id = Guid.NewGuid(),
                RoleId = roles["FinanceAccountant"],
                PermissionId = permissions[perm],
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        }

        // HR-Accountant gets View, Create
        foreach (var perm in accountantPermissions)
        {
            context.RolePermissionMappings.Add(new RolePermissionMapping
            {
                Id = Guid.NewGuid(),
                RoleId = roles["HRAccountant"],
                PermissionId = permissions[perm],
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        }

        // Finance-Audit gets View only
        context.RolePermissionMappings.Add(new RolePermissionMapping
        {
            Id = Guid.NewGuid(),
            RoleId = roles["FinanceAudit"],
            PermissionId = permissions["View"],
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        });

        // HR-Audit gets View only
        context.RolePermissionMappings.Add(new RolePermissionMapping
        {
            Id = Guid.NewGuid(),
            RoleId = roles["HRAudit"],
            PermissionId = permissions["View"],
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        });

        // Finance-Staff gets View only
        context.RolePermissionMappings.Add(new RolePermissionMapping
        {
            Id = Guid.NewGuid(),
            RoleId = roles["FinanceStaff"],
            PermissionId = permissions["View"],
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        });

        // HR-Staff gets View only
        context.RolePermissionMappings.Add(new RolePermissionMapping
        {
            Id = Guid.NewGuid(),
            RoleId = roles["HRStaff"],
            PermissionId = permissions["View"],
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        });

        await context.SaveChangesAsync();
    }

    private static async Task SeedRoleDepartmentMappingsAsync(
        CommandDbContext context,
        Dictionary<string, Guid> roles,
        Dictionary<string, Guid> departments)
    {
        // SuperAdmin - No department mapping (explicitly excluded as per requirements)

        // Finance Department Role Mappings
        var financeRoles = new[] { "FinanceAdmin", "FinanceManager", "FinanceAccountant", "FinanceAudit", "FinanceStaff" };
        foreach (var roleName in financeRoles)
        {
            context.RoleDepartmentMappings.Add(new RoleDepartmentMapping
            {
                Id = Guid.NewGuid(),
                RoleId = roles[roleName],
                DepartmentId = departments["Finance"],
                IsPrimary = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        }

        // HR Department Role Mappings
        var hrRoles = new[] { "HRAdmin", "HRManager", "HRAccountant", "HRAudit", "HRStaff" };
        foreach (var roleName in hrRoles)
        {
            context.RoleDepartmentMappings.Add(new RoleDepartmentMapping
            {
                Id = Guid.NewGuid(),
                RoleId = roles[roleName],
                DepartmentId = departments["Human Resources"],
                IsPrimary = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedRoleHierarchyAsync(
        CommandDbContext context,
        Dictionary<string, Guid> roles)
    {
        // SuperAdmin is at the top - no parent

        // Finance Department Hierarchy
        // Finance-Admin -> Finance-Manager -> Finance-Accountant -> Finance-Audit -> Finance-Staff
        context.RoleHierarchies.Add(new RoleHierarchy
        {
            Id = Guid.NewGuid(),
            ParentRoleId = roles["FinanceAdmin"],
            ChildRoleId = roles["FinanceManager"],
            Level = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        });

        context.RoleHierarchies.Add(new RoleHierarchy
        {
            Id = Guid.NewGuid(),
            ParentRoleId = roles["FinanceManager"],
            ChildRoleId = roles["FinanceAccountant"],
            Level = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        });

        context.RoleHierarchies.Add(new RoleHierarchy
        {
            Id = Guid.NewGuid(),
            ParentRoleId = roles["FinanceAccountant"],
            ChildRoleId = roles["FinanceAudit"],
            Level = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        });

        context.RoleHierarchies.Add(new RoleHierarchy
        {
            Id = Guid.NewGuid(),
            ParentRoleId = roles["FinanceAudit"],
            ChildRoleId = roles["FinanceStaff"],
            Level = 4,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        });

        // HR Department Hierarchy
        // HR-Admin -> HR-Manager -> HR-Accountant -> HR-Audit -> HR-Staff
        context.RoleHierarchies.Add(new RoleHierarchy
        {
            Id = Guid.NewGuid(),
            ParentRoleId = roles["HRAdmin"],
            ChildRoleId = roles["HRManager"],
            Level = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        });

        context.RoleHierarchies.Add(new RoleHierarchy
        {
            Id = Guid.NewGuid(),
            ParentRoleId = roles["HRManager"],
            ChildRoleId = roles["HRAccountant"],
            Level = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        });

        context.RoleHierarchies.Add(new RoleHierarchy
        {
            Id = Guid.NewGuid(),
            ParentRoleId = roles["HRAccountant"],
            ChildRoleId = roles["HRAudit"],
            Level = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        });

        context.RoleHierarchies.Add(new RoleHierarchy
        {
            Id = Guid.NewGuid(),
            ParentRoleId = roles["HRAudit"],
            ChildRoleId = roles["HRStaff"],
            Level = 4,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        });

        await context.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(
        UserManager<ApplicationUser> userManager,
        CommandDbContext context,
        Dictionary<string, Guid> departments,
        Dictionary<string, Guid> roles)
    {
        var users = new List<(string Email, string Password, string FirstName, string LastName, string RoleName, string? DepartmentName)>
        {
            // SuperAdmin - No department as per requirements
            ("sivan@office.com", "Admin@123", "Sivan", "Kumar", "SuperAdmin", null),
            
            // Finance Department Users
            ("finance.admin@office.com", "Admin@123", "John", "Smith", "FinanceAdmin", "Finance"),
            ("finance.manager@office.com", "Manager@123", "Sarah", "Johnson", "FinanceManager", "Finance"),
            ("finance.accountant@office.com", "Accountant@123", "Michael", "Brown", "FinanceAccountant", "Finance"),
            ("finance.audit@office.com", "Audit@123", "Emily", "Davis", "FinanceAudit", "Finance"),
            ("finance.staff@office.com", "Staff@123", "David", "Wilson", "FinanceStaff", "Finance"),
            
            // HR Department Users
            ("hr.admin@office.com", "Admin@123", "Jennifer", "Martinez", "HRAdmin", "Human Resources"),
            ("hr.manager@office.com", "Manager@123", "Robert", "Garcia", "HRManager", "Human Resources"),
            ("hr.accountant@office.com", "Accountant@123", "Lisa", "Rodriguez", "HRAccountant", "Human Resources"),
            ("hr.audit@office.com", "Audit@123", "James", "Anderson", "HRAudit", "Human Resources"),
            ("hr.staff@office.com", "Staff@123", "Maria", "Thomas", "HRStaff", "Human Resources")
        };

        foreach (var (email, password, firstName, lastName, roleName, deptName) in users)
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true,
                EmailConfirmed = true,
                NormalizedUserName = email.ToUpperInvariant(),
                NormalizedEmail = email.ToUpperInvariant()
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                // Add to Identity role
                var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role != null)
                {
                    await userManager.AddToRoleAsync(user, roleName);

                    // Add UserRoleMapping
                    var userRoleMapping = new UserRoleMapping
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        RoleId = role.Id,
                        DepartmentId = deptName != null ? departments[deptName] : null,
                        AssignedByEmail = "system@office.com",
                        AssignedAt = DateTime.UtcNow,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    context.UserRoleMappings.Add(userRoleMapping);

                    // Add user address
                    var address = new UserAddress
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Line1 = $"{firstName} {lastName} Street",
                        Line2 = "Building A",
                        City = "New Delhi",
                        State = "Delhi",
                        PostalCode = "110001",
                        Country = "India",
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    context.UserAddresses.Add(address);
                }
            }
        }

        await context.SaveChangesAsync();
    }
}
