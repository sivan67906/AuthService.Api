namespace AuthService.Application.Features.Department.CreateDepartment;

public sealed record DepartmentDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
