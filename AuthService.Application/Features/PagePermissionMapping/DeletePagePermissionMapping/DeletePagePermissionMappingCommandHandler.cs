using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.PagePermissionMapping.DeletePagePermissionMapping;

public sealed class DeletePagePermissionMappingCommandHandler : IRequestHandler<DeletePagePermissionMappingCommand, bool>
{
    private readonly ICommandDbContext _commandContext;
    private readonly IQueryDbContext _queryContext;

    public DeletePagePermissionMappingCommandHandler(ICommandDbContext commandContext, IQueryDbContext queryContext)
    {
        _commandContext = commandContext;
        _queryContext = queryContext;
    }

    public async Task<bool> Handle(DeletePagePermissionMappingCommand request, CancellationToken cancellationToken)
    {
        var entity = await _commandContext.PagePermissionMappings
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException($"PagePermissionMapping with ID {request.Id} not found");
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _commandContext.SaveChangesAsync(cancellationToken);

        // Sync to query database
        var queryEntity = await _queryContext.PagePermissionMappings
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (queryEntity != null)
        {
            queryEntity.IsDeleted = true;
            queryEntity.UpdatedAt = entity.UpdatedAt;
            await _queryContext.SaveChangesAsync(cancellationToken);
        }

        return true;
    }
}
