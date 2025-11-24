using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Permission.CreatePermission;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Permission.GetPermission;

public sealed class GetPermissionQueryHandler : IRequestHandler<GetPermissionQuery, PermissionDto?>
{
    private readonly IQueryDbContext _queryContext;

    public GetPermissionQueryHandler(IQueryDbContext queryContext)
    {
        _queryContext = queryContext;
    }

    public async Task<PermissionDto?> Handle(GetPermissionQuery request, CancellationToken cancellationToken)
    {
        var entity = await _queryContext.Permissions
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        return entity?.Adapt<PermissionDto>();
    }
}
