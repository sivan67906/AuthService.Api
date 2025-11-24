using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RoleHierarchyMapping.GetAllRoleHierarchies;

public record GetAllRoleHierarchiesQuery : IRequest<List<RoleHierarchyDto>>;

public class GetAllRoleHierarchiesQueryHandler : IRequestHandler<GetAllRoleHierarchiesQuery, List<RoleHierarchyDto>>
{
    private readonly IQueryDbContext _context;

    public GetAllRoleHierarchiesQueryHandler(IQueryDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoleHierarchyDto>> Handle(GetAllRoleHierarchiesQuery request, CancellationToken cancellationToken)
    {
        return await _context.RoleHierarchies
            .Include(rh => rh.ParentRole)
            .Include(rh => rh.ChildRole)
            .Select(rh => new RoleHierarchyDto
            {
                Id = rh.Id,
                ParentRoleId = rh.ParentRoleId,
                ParentRoleName = rh.ParentRole.Name ?? string.Empty,
                ChildRoleId = rh.ChildRoleId,
                ChildRoleName = rh.ChildRole.Name ?? string.Empty,
                Level = rh.Level,
                IsActive = rh.IsActive,
                CreatedAt = rh.CreatedAt,
                UpdatedAt = rh.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
