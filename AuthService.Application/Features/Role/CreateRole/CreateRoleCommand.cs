namespace AuthService.Application.Features.Role.CreateRole;

public sealed record CreateRoleCommand(
    string Name,
    string? Description,
    Guid? DepartmentId
) : IRequest<RoleDto>;
