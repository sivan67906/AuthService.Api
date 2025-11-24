using AuthService.Application.Features.Permission.CreatePermission;

namespace AuthService.Application.Features.Permission.UpdatePermission;

public sealed record UpdatePermissionCommand(
    Guid Id,
    string Name,
    string? Description
) : IRequest<PermissionDto>;
