using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RoleDepartmentMapping.GetAllRoleDepartmentMappings;

public sealed record GetAllRoleDepartmentMappingsQuery : IRequest<List<RoleDepartmentMappingDto>>;

public sealed class GetAllRoleDepartmentMappingsQueryHandler : IRequestHandler<GetAllRoleDepartmentMappingsQuery, List<RoleDepartmentMappingDto>>
{
    private readonly IQueryDbContext _context;

    public GetAllRoleDepartmentMappingsQueryHandler(IQueryDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoleDepartmentMappingDto>> Handle(GetAllRoleDepartmentMappingsQuery request, CancellationToken cancellationToken)
    {
        return await _context.RoleDepartmentMappings
            .Include(rdm => rdm.Role)
            .Include(rdm => rdm.Department)
            .Select(rdm => new RoleDepartmentMappingDto
            {
                Id = rdm.Id,
                RoleId = rdm.RoleId,
                RoleName = rdm.Role.Name ?? string.Empty,
                DepartmentId = rdm.DepartmentId,
                DepartmentName = rdm.Department.Name,
                IsPrimary = rdm.IsPrimary,
                IsActive = rdm.IsActive,
                CreatedAt = rdm.CreatedAt,
                UpdatedAt = rdm.UpdatedAt
            })
            .OrderBy(rdm => rdm.DepartmentName)
            .ThenBy(rdm => rdm.RoleName)
            .ToListAsync(cancellationToken);
    }
}
