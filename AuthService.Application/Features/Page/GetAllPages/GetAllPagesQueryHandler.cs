using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Page.CreatePage;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Page.GetAllPages;

public sealed class GetAllPagesQueryHandler : IRequestHandler<GetAllPagesQuery, List<PageDto>>
{
    private readonly IQueryDbContext _queryContext;

    public GetAllPagesQueryHandler(IQueryDbContext queryContext)
    {
        _queryContext = queryContext;
    }

    public async Task<List<PageDto>> Handle(GetAllPagesQuery request, CancellationToken cancellationToken)
    {
        var entities = await _queryContext.Pages
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Adapt<List<PageDto>>();
    }
}
