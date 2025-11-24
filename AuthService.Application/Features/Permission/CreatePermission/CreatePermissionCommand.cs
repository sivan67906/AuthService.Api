namespace AuthService.Application.Features.Permission.CreatePermission;

public sealed record CreatePermissionCommand(
    string Name,
    string? Description
) : IRequest<PermissionDto>;
