using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RoleHierarchyMapping.GetRoleHierarchyById;

public record GetRoleHierarchyByIdQuery(Guid Id) : IRequest<RoleHierarchyDto>;

public class GetRoleHierarchyByIdQueryHandler : IRequestHandler<GetRoleHierarchyByIdQuery, RoleHierarchyDto>
{
    private readonly IQueryDbContext _context;

    public GetRoleHierarchyByIdQueryHandler(IQueryDbContext context)
    {
        _context = context;
    }

    public async Task<RoleHierarchyDto> Handle(GetRoleHierarchyByIdQuery request, CancellationToken cancellationToken)
    {
        var roleHierarchy = await _context.RoleHierarchies
            .Include(rh => rh.ParentRole)
            .Include(rh => rh.ChildRole)
            .Where(rh => rh.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (roleHierarchy == null)
            throw new Exception($"Role hierarchy with ID {request.Id} not found");

        return roleHierarchy;
    }
}
