using System.Diagnostics;
using System.Linq;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Services;

public interface IUserAuthorizationService
{
    Task<bool> UserHasPermissionAsync(Guid userId, string permissionName);
    Task<bool> UserHasAccessToPageAsync(Guid userId, string pageName);
    Task<bool> UserHasAccessToDepartmentAsync(Guid userId, Guid? departmentId);
    Task<List<MenuItemDto>> GetUserMenusAsync(Guid userId);
    Task<List<string>> GetUserRolesAsync(Guid userId);
    Task<Guid?> GetUserDepartmentAsync(Guid userId);
}

public class UserAuthorizationService : IUserAuthorizationService
{
    private readonly IQueryDbContext _queryContext;
    private readonly ILogger<UserAuthorizationService> _logger;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public UserAuthorizationService(
        IQueryDbContext queryContext, 
        ILogger<UserAuthorizationService> logger,
        IMemoryCache cache)
    {
        _queryContext = queryContext;
        _logger = logger;
        _cache = cache;
    }

    public async Task<bool> UserHasPermissionAsync(Guid userId, string permissionName)
    {
        try
        {
            var userRoles = await _queryContext.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            if (!userRoles.Any())
                return false;

            var isSuperAdmin = await _queryContext.ApplicationRoles
                .AnyAsync(r => userRoles.Contains(r.Id) && r.Name == "SuperAdmin");

            if (isSuperAdmin)
                return true;

            var hasPermission = await _queryContext.RolePermissionMappings
                .Where(rpm => userRoles.Contains(rpm.RoleId) && rpm.IsActive)
                .Join(_queryContext.Permissions,
                    rpm => rpm.PermissionId,
                    p => p.Id,
                    (rpm, p) => p.Name)
                .AnyAsync(name => name == permissionName);

            return hasPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {PermissionName} for user {UserId}", permissionName, userId);
            return false;
        }
    }

    public async Task<bool> UserHasAccessToPageAsync(Guid userId, string pageName)
    {
        try
        {
            var userRoles = await _queryContext.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            if (!userRoles.Any())
                return false;

            var isSuperAdmin = await _queryContext.ApplicationRoles
                .AnyAsync(r => userRoles.Contains(r.Id) && r.Name == "SuperAdmin");

            if (isSuperAdmin)
                return true;

            var page = await _queryContext.Pages
                .Where(p => p.Name == pageName && p.IsActive)
                .FirstOrDefaultAsync();

            if (page == null)
                return false;

            var requiredPermissions = await _queryContext.PagePermissionMappings
                .Where(ppm => ppm.PageId == page.Id && ppm.IsActive)
                .Join(_queryContext.Permissions,
                    ppm => ppm.PermissionId,
                    p => p.Id,
                    (ppm, p) => p.Name)
                .ToListAsync();

            if (!requiredPermissions.Any())
                return true;

            var userPermissions = await _queryContext.RolePermissionMappings
                .Where(rpm => userRoles.Contains(rpm.RoleId) && rpm.IsActive)
                .Join(_queryContext.Permissions,
                    rpm => rpm.PermissionId,
                    p => p.Id,
                    (rpm, p) => p.Name)
                .Distinct()
                .ToListAsync();

            return requiredPermissions.Any(rp => userPermissions.Contains(rp));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking page access for {PageName} for user {UserId}", pageName, userId);
            return false;
        }
    }

    public async Task<bool> UserHasAccessToDepartmentAsync(Guid userId, Guid? departmentId)
    {
        try
        {
            var userRoles = await _queryContext.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            if (!userRoles.Any())
                return false;

            var isSuperAdmin = await _queryContext.ApplicationRoles
                .AnyAsync(r => userRoles.Contains(r.Id) && r.Name == "SuperAdmin");

            if (isSuperAdmin)
                return true;

            if (departmentId == null)
                return true;

            var userRoleMappings = await _queryContext.UserRoleMappings
                .Where(urm => urm.UserId == userId && urm.IsActive)
                .ToListAsync();

            return userRoleMappings.Any(urm => urm.DepartmentId == departmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking department access for user {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// OPTIMIZED: Gets user menus with efficient database queries and memory caching
    /// Reduces database roundtrips and uses optimized query patterns
    /// Expected performance: < 500ms instead of 18 seconds
    /// </summary>
    public async Task<List<MenuItemDto>> GetUserMenusAsync(Guid userId)
    {
        var totalStopwatch = Stopwatch.StartNew();
        _logger.LogInformation("=== MENU LOADING START for User: {UserId} ===", userId);

        // Check cache first
        var cacheKey = $"UserMenu_{userId}";
        if (_cache.TryGetValue<List<MenuItemDto>>(cacheKey, out var cachedMenu))
        {
            _logger.LogInformation("✓ Menu loaded from cache in {Elapsed}ms", totalStopwatch.ElapsedMilliseconds);
            return cachedMenu!;
        }

        try
        {
            // STEP 1: Get user roles
            var step1Stopwatch = Stopwatch.StartNew();
            var userRoleIds = await _queryContext.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();
            
            _logger.LogInformation("Step 1: User roles loaded in {Elapsed}ms - Found {Count} roles", 
                step1Stopwatch.ElapsedMilliseconds, userRoleIds.Count);

            if (!userRoleIds.Any())
            {
                _logger.LogWarning("No roles found for user {UserId}", userId);
                return new List<MenuItemDto>();
            }

            // STEP 2: Get role names and check admin status
            var step2Stopwatch = Stopwatch.StartNew();
            var roleNames = await _queryContext.ApplicationRoles
                .Where(r => userRoleIds.Contains(r.Id))
                .Select(r => r.Name)
                .ToListAsync();
            
            var isSuperAdmin = roleNames.Contains("SuperAdmin");
            var isAdmin = roleNames.Contains("Admin");
            
            _logger.LogInformation("Step 2: Role names loaded in {Elapsed}ms - IsSuperAdmin: {IsSuperAdmin}, IsAdmin: {IsAdmin}", 
                step2Stopwatch.ElapsedMilliseconds, isSuperAdmin, isAdmin);

            // STEP 3: Load ALL menu data efficiently
            var step3Stopwatch = Stopwatch.StartNew();
            
            var allFeatures = await _queryContext.Features
                .Where(f => f.IsActive)
                .AsNoTracking()
                .ToListAsync();
            
            var allPages = await _queryContext.Pages
                .Where(p => p.IsActive)
                .AsNoTracking()
                .ToListAsync();
            
            var allPageFeatureMappings = await _queryContext.PageFeatureMappings
                .AsNoTracking()
                .ToListAsync();
            
            _logger.LogInformation("Step 3: Menu data loaded in {Elapsed}ms - Features: {Features}, Pages: {Pages}, Mappings: {Mappings}", 
                step3Stopwatch.ElapsedMilliseconds, allFeatures.Count, allPages.Count, allPageFeatureMappings.Count);

            // STEP 4: Determine accessible pages
            var step4Stopwatch = Stopwatch.StartNew();
            HashSet<Guid> accessiblePageIds;

            if (isSuperAdmin)
            {
                // SuperAdmin gets ALL pages
                accessiblePageIds = allPages.Select(p => p.Id).ToHashSet();
                _logger.LogInformation("Step 4: SuperAdmin - All {Count} pages accessible", accessiblePageIds.Count);
            }
            else if (isAdmin)
            {
                // Admin gets all except Department management
                accessiblePageIds = allPages
                    .Where(p => p.MenuContext != "Department Management" && !p.Name.Contains("Department"))
                    .Select(p => p.Id)
                    .ToHashSet();
                _logger.LogInformation("Step 4: Admin - {Count} pages accessible (excluding Department management)", accessiblePageIds.Count);
            }
            else
            {
                // Regular user - filter by permissions
                var userPermissionIds = await _queryContext.RolePermissionMappings
                    .Where(rpm => userRoleIds.Contains(rpm.RoleId) && rpm.IsActive)
                    .Select(rpm => rpm.PermissionId)
                    .Distinct()
                    .ToListAsync();
                
                var pagePermissionMap = await _queryContext.PagePermissionMappings
                    .Where(ppm => ppm.IsActive)
                    .AsNoTracking()
                    .ToListAsync();

                accessiblePageIds = new HashSet<Guid>();
                foreach (var page in allPages)
                {
                    var pagePermissionIds = pagePermissionMap
                        .Where(ppm => ppm.PageId == page.Id)
                        .Select(ppm => ppm.PermissionId)
                        .ToList();

                    // If page has no permissions OR user has at least one required permission
                    if (!pagePermissionIds.Any() || pagePermissionIds.Any(pid => userPermissionIds.Contains(pid)))
                    {
                        accessiblePageIds.Add(page.Id);
                    }
                }
                
                _logger.LogInformation("Step 4: Regular user - {Count} accessible pages out of {Total}", 
                    accessiblePageIds.Count, allPages.Count);
            }
            step4Stopwatch.Stop();

            // STEP 5: Build menu hierarchy in memory
            var step5Stopwatch = Stopwatch.StartNew();
            var menuItems = BuildMenuHierarchy(allFeatures, allPages, allPageFeatureMappings, accessiblePageIds);
            step5Stopwatch.Stop();
            
            _logger.LogInformation("Step 5: Menu hierarchy built in {Elapsed}ms - {Count} top-level menu items", 
                step5Stopwatch.ElapsedMilliseconds, menuItems.Count);

            // Cache the result
            _cache.Set(cacheKey, menuItems, CacheDuration);

            totalStopwatch.Stop();
            _logger.LogInformation("=== MENU LOADING COMPLETE: TOTAL TIME = {Elapsed}ms ===", 
                totalStopwatch.ElapsedMilliseconds);

            return menuItems;
        }
        catch (Exception ex)
        {
            totalStopwatch.Stop();
            _logger.LogError(ex, "!!! ERROR loading menu after {Elapsed}ms: {Message}", 
                totalStopwatch.ElapsedMilliseconds, ex.Message);
            
            // Return empty list instead of throwing to prevent UI crash
            return new List<MenuItemDto>();
        }
    }

    private List<MenuItemDto> BuildMenuHierarchy(
        List<Feature> allFeatures,
        List<Page> allPages,
        List<PageFeatureMapping> allPageFeatureMappings,
        HashSet<Guid> accessiblePageIds)
    {
        var menuItems = new List<MenuItemDto>();

        try
        {
            // Get main menus (top level)
            var mainMenus = allFeatures
                .Where(f => f.IsMainMenu && f.ParentFeatureId == null)
                .OrderBy(f => f.DisplayOrder)
                .ToList();

            foreach (var mainMenu in mainMenus)
            {
                var menuItem = BuildMenuItemRecursive(mainMenu, allFeatures, allPages, allPageFeatureMappings, accessiblePageIds);
                
                if (menuItem != null && (menuItem.SubMenus.Any() || menuItem.Pages.Any()))
                {
                    menuItems.Add(menuItem);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building menu hierarchy: {Message}", ex.Message);
        }

        return menuItems;
    }

    private MenuItemDto? BuildMenuItemRecursive(
        Feature feature,
        List<Feature> allFeatures,
        List<Page> allPages,
        List<PageFeatureMapping> allPageFeatureMappings,
        HashSet<Guid> accessiblePageIds)
    {
        try
        {
            var menuItem = new MenuItemDto
            {
                Id = feature.Id,
                Name = feature.Name ?? string.Empty,
                Description = feature.Description,
                Icon = feature.Icon,
                DisplayOrder = feature.DisplayOrder,
                Level = feature.Level,
                SubMenus = new List<MenuItemDto>(),
                Pages = new List<PageDto>()
            };

            // Get child features (submenus)
            var childFeatures = allFeatures
                .Where(f => f.ParentFeatureId == feature.Id)
                .OrderBy(f => f.DisplayOrder)
                .ToList();

            foreach (var childFeature in childFeatures)
            {
                var subMenuItem = BuildMenuItemRecursive(childFeature, allFeatures, allPages, allPageFeatureMappings, accessiblePageIds);
                if (subMenuItem != null && (subMenuItem.SubMenus.Any() || subMenuItem.Pages.Any()))
                {
                    menuItem.SubMenus.Add(subMenuItem);
                }
            }

            // Get pages for this feature
            var featurePageIds = allPageFeatureMappings
                .Where(pf => pf.FeatureId == feature.Id)
                .Select(pf => pf.PageId)
                .ToHashSet();

            var featurePages = allPages
                .Where(p => featurePageIds.Contains(p.Id) && accessiblePageIds.Contains(p.Id))
                .OrderBy(p => p.DisplayOrder)
                .Select(p => new PageDto
                {
                    Id = p.Id,
                    PageId = p.Id, // Set both Id and PageId to the same value
                    Name = p.Name ?? string.Empty,
                    Url = p.Url ?? string.Empty,
                    Description = p.Description,
                    DisplayOrder = p.DisplayOrder,
                    ApiEndpoint = p.ApiEndpoint,
                    HttpMethod = p.HttpMethod
                })
                .ToList();

            menuItem.Pages = featurePages;

            return menuItem;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building menu item for feature {FeatureId}: {Message}", feature.Id, ex.Message);
            return null;
        }
    }

    public async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        try
        {
            var roles = await _queryContext.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_queryContext.ApplicationRoles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => r.Name)
                .ToListAsync();

            return roles ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user roles for user {UserId}", userId);
            return new List<string>();
        }
    }

    public async Task<Guid?> GetUserDepartmentAsync(Guid userId)
    {
        try
        {
            var userRoleMapping = await _queryContext.UserRoleMappings
                .Where(urm => urm.UserId == userId && urm.IsActive)
                .FirstOrDefaultAsync();

            return userRoleMapping?.DepartmentId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving department for user {UserId}", userId);
            return null;
        }
    }
}

// Public DTOs
public class MenuItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public int Level { get; set; }
    public List<MenuItemDto> SubMenus { get; set; } = new();
    public List<PageDto> Pages { get; set; } = new();
}

public class PageDto
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; } // Added for client compatibility
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public string? ApiEndpoint { get; set; }
    public string? HttpMethod { get; set; }
}
