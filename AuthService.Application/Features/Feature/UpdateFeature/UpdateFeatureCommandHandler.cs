using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Feature.CreateFeature;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Feature.UpdateFeature;

public sealed class UpdateFeatureCommandHandler : IRequestHandler<UpdateFeatureCommand, FeatureDto>
{
    private readonly ICommandDbContext _commandContext;
    private readonly IQueryDbContext _queryContext;

    public UpdateFeatureCommandHandler(ICommandDbContext commandContext, IQueryDbContext queryContext)
    {
        _commandContext = commandContext;
        _queryContext = queryContext;
    }

    public async Task<FeatureDto> Handle(UpdateFeatureCommand request, CancellationToken cancellationToken)
    {
        var entity = await _commandContext.Features
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException($"Feature with ID {request.Id} not found");
        }

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.RouteUrl = request.RouteUrl;
        entity.IsMainMenu = request.IsMainMenu;
        entity.ParentFeatureId = request.ParentFeatureId;
        entity.DisplayOrder = request.DisplayOrder;
        entity.Level = request.Level;
        entity.Icon = request.Icon;
        entity.UpdatedAt = DateTime.UtcNow;

        await _commandContext.SaveChangesAsync(cancellationToken);

        // Sync to query database
        var queryEntity = await _queryContext.Features
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (queryEntity != null)
        {
            queryEntity.Name = entity.Name;
            queryEntity.Description = entity.Description;
            queryEntity.RouteUrl = entity.RouteUrl;
            queryEntity.IsMainMenu = entity.IsMainMenu;
            queryEntity.ParentFeatureId = entity.ParentFeatureId;
            queryEntity.DisplayOrder = entity.DisplayOrder;
            queryEntity.Level = entity.Level;
            queryEntity.Icon = entity.Icon;
            queryEntity.UpdatedAt = entity.UpdatedAt;
            await _queryContext.SaveChangesAsync(cancellationToken);
        }

        return new FeatureDto(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.RouteUrl,
            entity.IsMainMenu,
            entity.ParentFeatureId,
            null,
            entity.DisplayOrder,
            entity.Level,
            entity.Icon,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }
}
