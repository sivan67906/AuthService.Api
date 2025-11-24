namespace AuthService.Application.Features.UserRoleMapping.GetAllUserRoleMappings;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public sealed record GetAllUserRoleMappingsQuery : IRequest<List<UserRoleMappingDto>>;

public sealed class GetAllUserRoleMappingsQueryHandler : IRequestHandler<GetAllUserRoleMappingsQuery, List<UserRoleMappingDto>>
{
    private readonly IQueryDbContext _context;

    public GetAllUserRoleMappingsQueryHandler(IQueryDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserRoleMappingDto>> Handle(GetAllUserRoleMappingsQuery request, CancellationToken cancellationToken)
    {
        return await _context.UserRoleMappings
            .Include(urm => urm.User)
            .Include(urm => urm.Role)
                .ThenInclude(r => r.Department)
            .Include(urm => urm.Department)
            .Select(urm => new UserRoleMappingDto
            {
                Id = urm.Id,
                UserId = urm.UserId,
                UserEmail = urm.User.Email ?? string.Empty,
                UserName = urm.User.UserName ?? string.Empty,
                RoleId = urm.RoleId,
                RoleName = urm.Role.Name ?? string.Empty,
                DepartmentId = urm.DepartmentId,
                DepartmentName = urm.Department != null ? urm.Department.Name : urm.Role.Department != null ? urm.Role.Department.Name : null,
                AssignedByEmail = urm.AssignedByEmail,
                AssignedAt = urm.AssignedAt,
                IsActive = urm.IsActive,
                CreatedAt = urm.CreatedAt,
                UpdatedAt = urm.UpdatedAt
            })
            .OrderBy(urm => urm.DepartmentName)
            .ThenBy(urm => urm.RoleName)
            .ThenBy(urm => urm.UserName)
            .ToListAsync(cancellationToken);
    }
}
