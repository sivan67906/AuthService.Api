using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.UserAccess.GetUserAccess;

public sealed class GetUserAccessQueryHandler : IRequestHandler<GetUserAccessQuery, UserAccessDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IQueryDbContext _queryContext;

    public GetUserAccessQueryHandler(UserManager<ApplicationUser> userManager, IQueryDbContext queryContext)
    {
        _userManager = userManager;
        _queryContext = queryContext;
    }

    public async Task<UserAccessDto> Handle(GetUserAccessQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID '{request.UserId}' not found");
        }

        // Get user roles
        var userRoles = await _userManager.GetRolesAsync(user);

        // Get role IDs
        var roleIds = await _queryContext.ApplicationRoles
            .Where(r => userRoles.Contains(r.Name!))
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        // Get permissions for these roles
        var permissions = await _queryContext.RolePermissionMappings
            .Where(rpm => roleIds.Contains(rpm.RoleId))
            .Include(rpm => rpm.Permission)
            .Select(rpm => rpm.Permission.Name)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Get permission IDs
        var permissionIds = await _queryContext.Permissions
            .Where(p => permissions.Contains(p.Name))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        // Get pages accessible by these permissions
        var pageAccess = await _queryContext.PagePermissionMappings
            .Where(ppm => permissionIds.Contains(ppm.PermissionId))
            .Include(ppm => ppm.Page)
                .ThenInclude(p => p.PageFeatures)
                    .ThenInclude(pf => pf.Feature)
            .Select(ppm => ppm.Page)
            .Distinct()
            .Where(p => !p.IsDeleted && p.IsActive)
            .ToListAsync(cancellationToken);

        var pageAccessDtos = pageAccess.Select(p => new PageAccessDto(
            p.Id,
            p.Name,
            p.Url,
            p.PageFeatures
                .Where(pf => !pf.Feature.IsDeleted && pf.Feature.IsActive)
                .Select(pf => pf.Feature.Name)
                .ToList()
        )).ToList();

        return new UserAccessDto(
            user.Id,
            user.Email!,
            userRoles.ToList(),
            permissions,
            pageAccessDtos
        );
    }
}
