using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Interfaces;

namespace AuthService.Application.Features.RolePermissionMapping.CreateRolePermissionMapping;

public sealed class CreateRolePermissionMappingCommandHandler : IRequestHandler<CreateRolePermissionMappingCommand, RolePermissionMappingDto>
{
    private readonly ICommandDbContext _commandContext;
    private readonly IQueryDbContext _queryContext;

    public CreateRolePermissionMappingCommandHandler(ICommandDbContext commandContext, IQueryDbContext queryContext)
    {
        _commandContext = commandContext;
        _queryContext = queryContext;
    }

    public async Task<RolePermissionMappingDto> Handle(CreateRolePermissionMappingCommand request, CancellationToken cancellationToken)
    {
        var entity = new Domain.Entities.RolePermissionMapping
        {
            RoleId = request.RoleId,
            PermissionId = request.PermissionId
        };

        _commandContext.RolePermissionMappings.Add(entity);
        await _commandContext.SaveChangesAsync(cancellationToken);

        // Sync to query database
        var queryEntity = entity.Adapt<Domain.Entities.RolePermissionMapping>();
        _queryContext.RolePermissionMappings.Add(queryEntity);
        await _queryContext.SaveChangesAsync(cancellationToken);

        return entity.Adapt<RolePermissionMappingDto>();
    }
}
