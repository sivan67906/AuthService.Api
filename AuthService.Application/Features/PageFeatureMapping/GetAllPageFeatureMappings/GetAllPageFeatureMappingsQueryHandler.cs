using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.PageFeatureMapping.CreatePageFeatureMapping;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.PageFeatureMapping.GetAllPageFeatureMappings;

public sealed class GetAllPageFeatureMappingsQueryHandler : IRequestHandler<GetAllPageFeatureMappingsQuery, List<PageFeatureMappingDto>>
{
    private readonly IQueryDbContext _queryContext;

    public GetAllPageFeatureMappingsQueryHandler(IQueryDbContext queryContext)
    {
        _queryContext = queryContext;
    }

    public async Task<List<PageFeatureMappingDto>> Handle(GetAllPageFeatureMappingsQuery request, CancellationToken cancellationToken)
    {
        var entities = await _queryContext.PageFeatureMappings
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Adapt<List<PageFeatureMappingDto>>();
    }
}
