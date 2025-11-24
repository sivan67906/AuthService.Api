using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Interfaces;

namespace AuthService.Application.Features.Feature.CreateFeature;

public sealed class CreateFeatureCommandHandler : IRequestHandler<CreateFeatureCommand, FeatureDto>
{
    private readonly ICommandDbContext _commandContext;
    private readonly IQueryDbContext _queryContext;

    public CreateFeatureCommandHandler(ICommandDbContext commandContext, IQueryDbContext queryContext)
    {
        _commandContext = commandContext;
        _queryContext = queryContext;
    }

    public async Task<FeatureDto> Handle(CreateFeatureCommand request, CancellationToken cancellationToken)
    {
        var entity = new Domain.Entities.Feature
        {
            Name = request.Name,
            Description = request.Description,
            RouteUrl = request.RouteUrl,
            IsMainMenu = request.IsMainMenu,
            ParentFeatureId = request.ParentFeatureId,
            DisplayOrder = request.DisplayOrder,
            Level = request.Level,
            Icon = request.Icon
        };

        _commandContext.Features.Add(entity);
        await _commandContext.SaveChangesAsync(cancellationToken);

        // Sync to query database
        var queryEntity = entity.Adapt<Domain.Entities.Feature>();
        _queryContext.Features.Add(queryEntity);
        await _queryContext.SaveChangesAsync(cancellationToken);

        return new FeatureDto(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.RouteUrl,
            entity.IsMainMenu,
            entity.ParentFeatureId,
            null, // ParentFeatureName will be null for new entities
            entity.DisplayOrder,
            entity.Level,
            entity.Icon,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }
}
