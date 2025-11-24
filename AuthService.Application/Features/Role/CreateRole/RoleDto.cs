namespace AuthService.Application.Features.Role.CreateRole;

public sealed record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? DepartmentId,
    string? DepartmentName
);
