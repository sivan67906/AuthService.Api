namespace AuthService.Application.Features.UserAccess.AssignRole;

public sealed record AssignRoleCommand(
    string EmailId,
    string RoleName,
    Guid DepartmentId
) : IRequest<bool>;
