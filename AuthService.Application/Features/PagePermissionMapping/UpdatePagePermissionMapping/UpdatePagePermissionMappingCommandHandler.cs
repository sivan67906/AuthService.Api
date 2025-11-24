using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.PagePermissionMapping.CreatePagePermissionMapping;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.PagePermissionMapping.UpdatePagePermissionMapping;

public sealed class UpdatePagePermissionMappingCommandHandler : IRequestHandler<UpdatePagePermissionMappingCommand, PagePermissionMappingDto>
{
    private readonly ICommandDbContext _commandContext;
    private readonly IQueryDbContext _queryContext;

    public UpdatePagePermissionMappingCommandHandler(ICommandDbContext commandContext, IQueryDbContext queryContext)
    {
        _commandContext = commandContext;
        _queryContext = queryContext;
    }

    public async Task<PagePermissionMappingDto> Handle(UpdatePagePermissionMappingCommand request, CancellationToken cancellationToken)
    {
        var entity = await _commandContext.PagePermissionMappings
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException($"PagePermissionMapping with ID {request.Id} not found");
        }

        entity.PageId = request.PageId;
        entity.PermissionId = request.PermissionId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _commandContext.SaveChangesAsync(cancellationToken);

        // Sync to query database
        var queryEntity = await _queryContext.PagePermissionMappings
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (queryEntity != null)
        {
            queryEntity.PageId = entity.PageId;
            queryEntity.PermissionId = entity.PermissionId;
            queryEntity.UpdatedAt = entity.UpdatedAt;
            await _queryContext.SaveChangesAsync(cancellationToken);
        }

        return entity.Adapt<PagePermissionMappingDto>();
    }
}
