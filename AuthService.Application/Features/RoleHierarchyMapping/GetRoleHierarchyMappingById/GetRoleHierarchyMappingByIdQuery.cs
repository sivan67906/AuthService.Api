namespace AuthService.Application.Features.RoleHierarchyMapping.GetRoleHierarchyMappingById;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public sealed record GetRoleHierarchyMappingByIdQuery(Guid Id) : IRequest<RoleHierarchyMappingDto?>;

public sealed class GetRoleHierarchyMappingByIdQueryHandler : IRequestHandler<GetRoleHierarchyMappingByIdQuery, RoleHierarchyMappingDto?>
{
    private readonly IQueryDbContext _context;

    public GetRoleHierarchyMappingByIdQueryHandler(IQueryDbContext context)
    {
        _context = context;
    }

    public async Task<RoleHierarchyMappingDto?> Handle(GetRoleHierarchyMappingByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.RoleHierarchies
            .Include(rh => rh.ParentRole)
                .ThenInclude(r => r.Department)
            .Include(rh => rh.ChildRole)
                .ThenInclude(r => r.Department)
            .Where(rh => rh.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}
