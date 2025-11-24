using AuthService.Application.Features.Page.CreatePage;

namespace AuthService.Application.Features.Page.UpdatePage;

public sealed record UpdatePageCommand(
    Guid Id,
    string Name,
    string Url,
    string? Description,
    int DisplayOrder
) : IRequest<PageDto>;
