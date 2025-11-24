using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RolePermissionMapping.DeleteRolePermissionMapping;

public sealed class DeleteRolePermissionMappingCommandHandler : IRequestHandler<DeleteRolePermissionMappingCommand, bool>
{
    private readonly ICommandDbContext _commandContext;
    private readonly IQueryDbContext _queryContext;

    public DeleteRolePermissionMappingCommandHandler(ICommandDbContext commandContext, IQueryDbContext queryContext)
    {
        _commandContext = commandContext;
        _queryContext = queryContext;
    }

    public async Task<bool> Handle(DeleteRolePermissionMappingCommand request, CancellationToken cancellationToken)
    {
        var entity = await _commandContext.RolePermissionMappings
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException($"RolePermissionMapping with ID {request.Id} not found");
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _commandContext.SaveChangesAsync(cancellationToken);

        // Sync to query database
        var queryEntity = await _queryContext.RolePermissionMappings
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
