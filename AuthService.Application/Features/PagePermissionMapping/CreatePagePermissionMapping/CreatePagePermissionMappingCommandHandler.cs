using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Interfaces;

namespace AuthService.Application.Features.PagePermissionMapping.CreatePagePermissionMapping;

public sealed class CreatePagePermissionMappingCommandHandler : IRequestHandler<CreatePagePermissionMappingCommand, PagePermissionMappingDto>
{
    private readonly ICommandDbContext _commandContext;
    private readonly IQueryDbContext _queryContext;

    public CreatePagePermissionMappingCommandHandler(ICommandDbContext commandContext, IQueryDbContext queryContext)
    {
        _commandContext = commandContext;
        _queryContext = queryContext;
    }

    public async Task<PagePermissionMappingDto> Handle(CreatePagePermissionMappingCommand request, CancellationToken cancellationToken)
    {
        var entity = new Domain.Entities.PagePermissionMapping
        {
            PageId = request.PageId,
            PermissionId = request.PermissionId
        };

        _commandContext.PagePermissionMappings.Add(entity);
        await _commandContext.SaveChangesAsync(cancellationToken);

        // Sync to query database
        var queryEntity = entity.Adapt<Domain.Entities.PagePermissionMapping>();
        _queryContext.PagePermissionMappings.Add(queryEntity);
        await _queryContext.SaveChangesAsync(cancellationToken);

        return entity.Adapt<PagePermissionMappingDto>();
    }
}
