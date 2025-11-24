using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Page.CreatePage;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Page.GetPage;

public sealed class GetPageQueryHandler : IRequestHandler<GetPageQuery, PageDto?>
{
    private readonly IQueryDbContext _queryContext;

    public GetPageQueryHandler(IQueryDbContext queryContext)
    {
        _queryContext = queryContext;
    }

    public async Task<PageDto?> Handle(GetPageQuery request, CancellationToken cancellationToken)
    {
        var entity = await _queryContext.Pages
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        return entity?.Adapt<PageDto>();
    }
}
