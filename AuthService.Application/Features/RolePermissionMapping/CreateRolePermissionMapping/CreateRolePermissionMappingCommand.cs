namespace AuthService.Application.Features.RolePermissionMapping.CreateRolePermissionMapping;

public sealed record CreateRolePermissionMappingCommand(
    Guid RoleId,
    Guid PermissionId
) : IRequest<RolePermissionMappingDto>;
