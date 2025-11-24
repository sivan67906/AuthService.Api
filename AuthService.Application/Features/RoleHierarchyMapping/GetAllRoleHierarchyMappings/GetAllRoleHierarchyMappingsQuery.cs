namespace AuthService.Application.Features.RoleHierarchyMapping.GetAllRoleHierarchyMappings;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public sealed record GetAllRoleHierarchyMappingsQuery : IRequest<List<RoleHierarchyMappingDto>>;

public sealed class GetAllRoleHierarchyMappingsQueryHandler : IRequestHandler<GetAllRoleHierarchyMappingsQuery, List<RoleHierarchyMappingDto>>
{
    private readonly IQueryDbContext _context;

    public GetAllRoleHierarchyMappingsQueryHandler(IQueryDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoleHierarchyMappingDto>> Handle(GetAllRoleHierarchyMappingsQuery request, CancellationToken cancellationToken)
    {
        return await _context.RoleHierarchies
            .Include(rh => rh.ParentRole)
                .ThenInclude(r => r.Department)
            .Include(rh => rh.ChildRole)
                .ThenInclude(r => r.Department)
            .Select(rh => new RoleHierarchyMappingDto
            {
                Id = rh.Id,
                ParentRoleId = rh.ParentRoleId,
                ParentRoleName = rh.ParentRole.Name ?? string.Empty,
                ParentDepartmentId = rh.ParentRole.DepartmentId,
                ParentDepartmentName = rh.ParentRole.Department != null ? rh.ParentRole.Department.Name : null,
                ChildRoleId = rh.ChildRoleId,
                ChildRoleName = rh.ChildRole.Name ?? string.Empty,
                ChildDepartmentId = rh.ChildRole.DepartmentId,
                ChildDepartmentName = rh.ChildRole.Department != null ? rh.ChildRole.Department.Name : null,
                Level = rh.Level,
                IsActive = rh.IsActive,
                CreatedAt = rh.CreatedAt,
                UpdatedAt = rh.UpdatedAt
            })
            .OrderBy(rh => rh.Level)
            .ThenBy(rh => rh.ParentDepartmentName)
            .ThenBy(rh => rh.ParentRoleName)
            .ToListAsync(cancellationToken);
    }
}
