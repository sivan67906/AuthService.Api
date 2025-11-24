using AuthService.Application.Features.RolePermissionMapping.CreateRolePermissionMapping;

namespace AuthService.Application.Features.RolePermissionMapping.UpdateRolePermissionMapping;

public sealed record UpdateRolePermissionMappingCommand(
    Guid Id,
    Guid RoleId,
    Guid PermissionId
) : IRequest<RolePermissionMappingDto>;
