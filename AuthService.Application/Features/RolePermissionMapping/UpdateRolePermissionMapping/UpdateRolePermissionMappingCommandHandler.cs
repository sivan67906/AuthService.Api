using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.RolePermissionMapping.CreateRolePermissionMapping;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RolePermissionMapping.UpdateRolePermissionMapping;

public sealed class UpdateRolePermissionMappingCommandHandler : IRequestHandler<UpdateRolePermissionMappingCommand, RolePermissionMappingDto>
{
    private readonly ICommandDbContext _commandContext;
    private readonly IQueryDbContext _queryContext;

    public UpdateRolePermissionMappingCommandHandler(ICommandDbContext commandContext, IQueryDbContext queryContext)
    {
        _commandContext = commandContext;
        _queryContext = queryContext;
    }

    public async Task<RolePermissionMappingDto> Handle(UpdateRolePermissionMappingCommand request, CancellationToken cancellationToken)
    {
        var entity = await _commandContext.RolePermissionMappings
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException($"RolePermissionMapping with ID {request.Id} not found");
        }

        entity.RoleId = request.RoleId;
        entity.PermissionId = request.PermissionId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _commandContext.SaveChangesAsync(cancellationToken);

        // Sync to query database
        var queryEntity = await _queryContext.RolePermissionMappings
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (queryEntity != null)
        {
            queryEntity.RoleId = entity.RoleId;
            queryEntity.PermissionId = entity.PermissionId;
            queryEntity.UpdatedAt = entity.UpdatedAt;
            await _queryContext.SaveChangesAsync(cancellationToken);
        }

        return entity.Adapt<RolePermissionMappingDto>();
    }
}
