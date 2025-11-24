namespace AuthService.Application.Features.Page.CreatePage;

public sealed record CreatePageCommand(
    string Name,
    string Url,
    string? Description,
    int DisplayOrder
) : IRequest<PageDto>;
