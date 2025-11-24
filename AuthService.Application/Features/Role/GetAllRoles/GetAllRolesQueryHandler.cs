using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Role.CreateRole;
using AuthService.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Role.GetAllRoles;

public sealed class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, List<RoleDto>>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IQueryDbContext _queryContext;

    public GetAllRolesQueryHandler(RoleManager<ApplicationRole> roleManager, IQueryDbContext queryContext)
    {
        _roleManager = roleManager;
        _queryContext = queryContext;
    }

    public async Task<List<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _roleManager.Roles
            .Include(r => r.Department)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return roles.Select(r => new RoleDto(
            r.Id,
            r.Name!,
            r.Description,
            r.DepartmentId,
            r.Department?.Name
        )).ToList();
    }
}
