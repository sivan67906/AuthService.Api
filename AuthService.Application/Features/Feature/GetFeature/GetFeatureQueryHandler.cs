using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Feature.CreateFeature;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Feature.GetFeature;

public sealed class GetFeatureQueryHandler : IRequestHandler<GetFeatureQuery, FeatureDto?>
{
    private readonly IQueryDbContext _queryContext;

    public GetFeatureQueryHandler(IQueryDbContext queryContext)
    {
        _queryContext = queryContext;
    }

    public async Task<FeatureDto?> Handle(GetFeatureQuery request, CancellationToken cancellationToken)
    {
        var entity = await _queryContext.Features
            .Include(x => x.ParentFeature)
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (entity == null)
        {
            return null;
        }

        return new FeatureDto(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.RouteUrl,
            entity.IsMainMenu,
            entity.ParentFeatureId,
            entity.ParentFeature?.Name,
            entity.DisplayOrder,
            entity.Level,
            entity.Icon,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }
}
