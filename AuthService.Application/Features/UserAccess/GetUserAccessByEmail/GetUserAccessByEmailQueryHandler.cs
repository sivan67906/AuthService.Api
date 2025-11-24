using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.UserAccess.GetUserAccess;
using AuthService.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.UserAccess.GetUserAccessByEmail;

public sealed class GetUserAccessByEmailQueryHandler : IRequestHandler<GetUserAccessByEmailQuery, UserAccessDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IQueryDbContext _queryContext;

    public GetUserAccessByEmailQueryHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IQueryDbContext queryContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _queryContext = queryContext;
    }

    public async Task<UserAccessDto> Handle(GetUserAccessByEmailQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            throw new InvalidOperationException($"User with email '{request.Email}' not found");
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        var permissions = new HashSet<string>();
        var pageAccessList = new Dictionary<Guid, PageAccessDto>();

        foreach (var roleName in userRoles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) continue;

            var rolePermissions = await _queryContext.RolePermissionMappings
                .Where(rpm => rpm.RoleId == role.Id && rpm.IsActive && !rpm.IsDeleted)
                .Include(rpm => rpm.Permission)
                .ToListAsync(cancellationToken);

            foreach (var rpm in rolePermissions)
            {
                if (rpm.Permission != null)
                {
                    permissions.Add(rpm.Permission.Name);
                }
            }
        }

        var permissionIds = await _queryContext.Permissions
            .Where(p => permissions.Contains(p.Name))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var pagePermissions = await _queryContext.PagePermissionMappings
            .Where(ppm => permissionIds.Contains(ppm.PermissionId) && ppm.IsActive && !ppm.IsDeleted)
            .Include(ppm => ppm.Page)
            .ToListAsync(cancellationToken);

        foreach (var pp in pagePermissions)
        {
            if (pp.Page == null) continue;

            if (!pageAccessList.ContainsKey(pp.PageId))
            {
                var features = await _queryContext.PageFeatureMappings
                    .Where(pfm => pfm.PageId == pp.PageId && pfm.IsActive && !pfm.IsDeleted)
                    .Include(pfm => pfm.Feature)
                    .Select(pfm => pfm.Feature.Name)
                    .ToListAsync(cancellationToken);

                var pageAccessDto = new PageAccessDto(
                    pp.Page.Id,
                    pp.Page.Name,
                    pp.Page.Url,
                    features
                );

                pageAccessList[pp.PageId] = pageAccessDto;
            }
        }

        return new UserAccessDto(
            user.Id,
            user.Email ?? string.Empty,
            userRoles.ToList(),
            permissions.ToList(),
            pageAccessList.Values.ToList()
        );
    }
}