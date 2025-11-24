using AuthService.Application.Features.Role.CreateRole;

namespace AuthService.Application.Features.Role.UpdateRole;

public sealed record UpdateRoleCommand(
    Guid RoleId,
    string Name,
    string? Description,
    Guid? DepartmentId
) : IRequest<RoleDto>;
