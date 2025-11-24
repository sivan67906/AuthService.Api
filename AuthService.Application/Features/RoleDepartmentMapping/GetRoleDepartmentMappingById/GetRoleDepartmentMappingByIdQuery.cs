using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RoleDepartmentMapping.GetRoleDepartmentMappingById;

public sealed record GetRoleDepartmentMappingByIdQuery(Guid Id) : IRequest<RoleDepartmentMappingDto?>;

public sealed class GetRoleDepartmentMappingByIdQueryHandler : IRequestHandler<GetRoleDepartmentMappingByIdQuery, RoleDepartmentMappingDto?>
{
    private readonly IQueryDbContext _context;

    public GetRoleDepartmentMappingByIdQueryHandler(IQueryDbContext context)
    {
        _context = context;
    }

    public async Task<RoleDepartmentMappingDto?> Handle(GetRoleDepartmentMappingByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.RoleDepartmentMappings
            .Include(rdm => rdm.Role)
            .Include(rdm => rdm.Department)
            .Where(rdm => rdm.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}