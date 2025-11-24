namespace AuthService.Application.Features.RolePermissionMapping.DeleteRolePermissionMapping;

public sealed record DeleteRolePermissionMappingCommand(Guid Id) : IRequest<bool>;
