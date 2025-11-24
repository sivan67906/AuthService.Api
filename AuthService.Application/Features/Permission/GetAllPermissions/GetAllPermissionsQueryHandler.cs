using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Permission.CreatePermission;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Permission.GetAllPermissions;

public sealed class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, List<PermissionDto>>
{
    private readonly IQueryDbContext _queryContext;

    public GetAllPermissionsQueryHandler(IQueryDbContext queryContext)
    {
        _queryContext = queryContext;
    }

    public async Task<List<PermissionDto>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
    {
        var entities = await _queryContext.Permissions
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Adapt<List<PermissionDto>>();
    }
}
