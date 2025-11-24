using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Feature.CreateFeature;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Feature.GetAllFeatures;

public sealed class GetAllFeaturesQueryHandler : IRequestHandler<GetAllFeaturesQuery, List<FeatureDto>>
{
    private readonly IQueryDbContext _queryContext;

    public GetAllFeaturesQueryHandler(IQueryDbContext queryContext)
    {
        _queryContext = queryContext;
    }

    public async Task<List<FeatureDto>> Handle(GetAllFeaturesQuery request, CancellationToken cancellationToken)
    {
        var entities = await _queryContext.Features
            .Include(x => x.ParentFeature)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);

        return entities.Select(x => new FeatureDto(
            x.Id,
            x.Name,
            x.Description,
            x.RouteUrl,
            x.IsMainMenu,
            x.ParentFeatureId,
            x.ParentFeature?.Name,
            x.DisplayOrder,
            x.Level,
            x.Icon,
            x.IsActive,
            x.CreatedAt,
            x.UpdatedAt
        )).ToList();
    }
}
