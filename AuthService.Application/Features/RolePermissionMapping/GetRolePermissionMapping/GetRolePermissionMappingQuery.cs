using AuthService.Application.Features.RolePermissionMapping.CreateRolePermissionMapping;

namespace AuthService.Application.Features.RolePermissionMapping.GetRolePermissionMapping;

public sealed record GetRolePermissionMappingQuery(Guid Id) : IRequest<RolePermissionMappingDto?>;
