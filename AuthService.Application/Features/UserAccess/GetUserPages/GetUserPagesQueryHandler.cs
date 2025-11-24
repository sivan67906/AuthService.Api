using Microsoft.AspNetCore.Identity;
using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using AuthService.Domain.Interfaces;

namespace AuthService.Application.Features.UserAccess.GetUserPages;

public sealed class GetUserPagesQueryHandler : IRequestHandler<GetUserPagesQuery, List<UserPageDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IQueryDbContext _context;

    public GetUserPagesQueryHandler(UserManager<ApplicationUser> userManager, IQueryDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<List<UserPageDto>> Handle(GetUserPagesQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID '{request.UserId}' not found");
        }

        // Get user's roles
        var userRoles = await _userManager.GetRolesAsync(user);
        if (!userRoles.Any())
        {
            return new List<UserPageDto>();
        }

        // Get all roles with their IDs
        var roles = await _context.ApplicationRoles
            .Where(r => userRoles.Contains(r.Name!))
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        if (!roles.Any())
        {
            return new List<UserPageDto>();
        }

        // Get permissions for these roles
        var permissionIds = await _context.RolePermissionMappings
            .Where(rpm => roles.Contains(rpm.RoleId))
            .Select(rpm => rpm.PermissionId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (!permissionIds.Any())
        {
            return new List<UserPageDto>();
        }

        // Get pages that require these permissions
        var pageIds = await _context.PagePermissionMappings
            .Where(ppm => permissionIds.Contains(ppm.PermissionId))
            .Select(ppm => ppm.PageId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (!pageIds.Any())
        {
            return new List<UserPageDto>();
        }

        // Get page details with their permissions and features
        var pages = await _context.Pages
            .Where(p => pageIds.Contains(p.Id) && p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Url,
                p.Description,
                p.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        var result = new List<UserPageDto>();

        foreach (var page in pages)
        {
            // Get permissions for this page
            var pagePermissions = await _context.PagePermissionMappings
                .Where(ppm => ppm.PageId == page.Id)
                .Join(_context.Permissions,
                    ppm => ppm.PermissionId,
                    p => p.Id,
                    (ppm, p) => p.Name)
                .ToListAsync(cancellationToken);

            // Get features for this page
            var pageFeatures = await _context.PageFeatureMappings
                .Where(pfm => pfm.PageId == page.Id)
                .Join(_context.Features,
                    pfm => pfm.FeatureId,
                    f => f.Id,
                    (pfm, f) => f.Name)
                .ToListAsync(cancellationToken);

            result.Add(new UserPageDto
            {
                PageId = page.Id,
                Name = page.Name,
                Url = page.Url,
                Description = page.Description,
                DisplayOrder = page.DisplayOrder,
                RequiredPermissions = pagePermissions,
                Features = pageFeatures
            });
        }

        return result.OrderBy(p => p.DisplayOrder).ToList();
    }
}
