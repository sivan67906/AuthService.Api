using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Permission.CreatePermission;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Permission.UpdatePermission;

public sealed class UpdatePermissionCommandHandler : IRequestHandler<UpdatePermissionCommand, PermissionDto>
{
    private readonly ICommandDbContext _commandContext;
    private readonly IQueryDbContext _queryContext;

    public UpdatePermissionCommandHandler(ICommandDbContext commandContext, IQueryDbContext queryContext)
    {
        _commandContext = commandContext;
        _queryContext = queryContext;
    }

    public async Task<PermissionDto> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _commandContext.Permissions
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException($"Permission with ID {request.Id} not found");
        }

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.UpdatedAt = DateTime.UtcNow;

        await _commandContext.SaveChangesAsync(cancellationToken);

        // Sync to query database
        var queryEntity = await _queryContext.Permissions
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (queryEntity != null)
        {
            queryEntity.Name = entity.Name;
            queryEntity.Description = entity.Description;
            queryEntity.UpdatedAt = entity.UpdatedAt;
            await _queryContext.SaveChangesAsync(cancellationToken);
        }

        return entity.Adapt<PermissionDto>();
    }
}
