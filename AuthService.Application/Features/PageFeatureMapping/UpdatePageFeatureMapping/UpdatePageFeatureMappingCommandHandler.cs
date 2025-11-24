using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.PageFeatureMapping.CreatePageFeatureMapping;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.PageFeatureMapping.UpdatePageFeatureMapping;

public sealed class UpdatePageFeatureMappingCommandHandler : IRequestHandler<UpdatePageFeatureMappingCommand, PageFeatureMappingDto>
{
    private readonly ICommandDbContext _commandContext;
    private readonly IQueryDbContext _queryContext;

    public UpdatePageFeatureMappingCommandHandler(ICommandDbContext commandContext, IQueryDbContext queryContext)
    {
        _commandContext = commandContext;
        _queryContext = queryContext;
    }

    public async Task<PageFeatureMappingDto> Handle(UpdatePageFeatureMappingCommand request, CancellationToken cancellationToken)
    {
        var entity = await _commandContext.PageFeatureMappings
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException($"PageFeatureMapping with ID {request.Id} not found");
        }

        entity.PageId = request.PageId;
        entity.FeatureId = request.FeatureId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _commandContext.SaveChangesAsync(cancellationToken);

        // Sync to query database
        var queryEntity = await _queryContext.PageFeatureMappings
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (queryEntity != null)
        {
            queryEntity.PageId = entity.PageId;
            queryEntity.FeatureId = entity.FeatureId;
            queryEntity.UpdatedAt = entity.UpdatedAt;
            await _queryContext.SaveChangesAsync(cancellationToken);
        }

        return entity.Adapt<PageFeatureMappingDto>();
    }
}
