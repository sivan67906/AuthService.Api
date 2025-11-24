namespace AuthService.Application.Features.RolePermissionMapping.CreateRolePermissionMapping;

public sealed record RolePermissionMappingDto(
    Guid Id,
    Guid RoleId,
    Guid PermissionId,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
