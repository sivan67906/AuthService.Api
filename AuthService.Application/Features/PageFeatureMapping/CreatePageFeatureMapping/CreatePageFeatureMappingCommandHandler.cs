using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Interfaces;

namespace AuthService.Application.Features.PageFeatureMapping.CreatePageFeatureMapping;

public sealed class CreatePageFeatureMappingCommandHandler : IRequestHandler<CreatePageFeatureMappingCommand, PageFeatureMappingDto>
{
    private readonly ICommandDbContext _commandContext;
    private readonly IQueryDbContext _queryContext;

    public CreatePageFeatureMappingCommandHandler(ICommandDbContext commandContext, IQueryDbContext queryContext)
    {
        _commandContext = commandContext;
        _queryContext = queryContext;
    }

    public async Task<PageFeatureMappingDto> Handle(CreatePageFeatureMappingCommand request, CancellationToken cancellationToken)
    {
        var entity = new Domain.Entities.PageFeatureMapping
        {
            PageId = request.PageId,
            FeatureId = request.FeatureId
        };

        _commandContext.PageFeatureMappings.Add(entity);
        await _commandContext.SaveChangesAsync(cancellationToken);

        // Sync to query database
        var queryEntity = entity.Adapt<Domain.Entities.PageFeatureMapping>();
        _queryContext.PageFeatureMappings.Add(queryEntity);
        await _queryContext.SaveChangesAsync(cancellationToken);

        return entity.Adapt<PageFeatureMappingDto>();
    }
}
