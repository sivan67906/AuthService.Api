using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Interfaces;

namespace AuthService.Application.Features.Permission.CreatePermission;

public sealed class CreatePermissionCommandHandler : IRequestHandler<CreatePermissionCommand, PermissionDto>
{
    private readonly ICommandDbContext _commandContext;
    private readonly IQueryDbContext _queryContext;

    public CreatePermissionCommandHandler(ICommandDbContext commandContext, IQueryDbContext queryContext)
    {
        _commandContext = commandContext;
        _queryContext = queryContext;
    }

    public async Task<PermissionDto> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
    {
        var entity = new Domain.Entities.Permission
        {
            Name = request.Name,
            Description = request.Description
        };

        _commandContext.Permissions.Add(entity);
        await _commandContext.SaveChangesAsync(cancellationToken);

        // Sync to query database
        var queryEntity = entity.Adapt<Domain.Entities.Permission>();
        _queryContext.Permissions.Add(queryEntity);
        await _queryContext.SaveChangesAsync(cancellationToken);

        return entity.Adapt<PermissionDto>();
    }
}
