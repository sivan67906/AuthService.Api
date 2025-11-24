using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.RolePermissionMapping.CreateRolePermissionMapping;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RolePermissionMapping.GetRolePermissionMapping;

public sealed class GetRolePermissionMappingQueryHandler : IRequestHandler<GetRolePermissionMappingQuery, RolePermissionMappingDto?>
{
    private readonly IQueryDbContext _queryContext;

    public GetRolePermissionMappingQueryHandler(IQueryDbContext queryContext)
    {
        _queryContext = queryContext;
    }

    public async Task<RolePermissionMappingDto?> Handle(GetRolePermissionMappingQuery request, CancellationToken cancellationToken)
    {
        var entity = await _queryContext.RolePermissionMappings
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        return entity?.Adapt<RolePermissionMappingDto>();
    }
}
