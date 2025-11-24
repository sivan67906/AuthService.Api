namespace AuthService.Application.Features.UserAccess.GetUserAccess;

public sealed record GetUserAccessQuery(Guid UserId) : IRequest<UserAccessDto>;

public sealed record UserAccessDto(
    Guid UserId,
    string Email,
    List<string> Roles,
    List<string> Permissions,
    List<PageAccessDto> Pages
);

public sealed record PageAccessDto(
    Guid PageId,
    string PageName,
    string PageUrl,
    List<string> Features
);
