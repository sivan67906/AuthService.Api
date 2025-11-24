using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.PageFeatureMapping.CreatePageFeatureMapping;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.PageFeatureMapping.GetPageFeatureMapping;

public sealed class GetPageFeatureMappingQueryHandler : IRequestHandler<GetPageFeatureMappingQuery, PageFeatureMappingDto?>
{
    private readonly IQueryDbContext _queryContext;

    public GetPageFeatureMappingQueryHandler(IQueryDbContext queryContext)
    {
        _queryContext = queryContext;
    }

    public async Task<PageFeatureMappingDto?> Handle(GetPageFeatureMappingQuery request, CancellationToken cancellationToken)
    {
        var entity = await _queryContext.PageFeatureMappings
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        return entity?.Adapt<PageFeatureMappingDto>();
    }
}
