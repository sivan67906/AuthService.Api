namespace AuthService.Application.Features.Page.CreatePage;

public sealed record PageDto(
    Guid Id,
    string Name,
    string Url,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
