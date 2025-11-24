namespace AuthService.Application.Features.PagePermissionMapping.CreatePagePermissionMapping;

public sealed record PagePermissionMappingDto(
    Guid Id,
    Guid PageId,
    Guid PermissionId,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
