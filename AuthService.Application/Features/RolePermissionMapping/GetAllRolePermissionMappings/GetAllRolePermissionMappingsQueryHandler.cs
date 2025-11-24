using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.RolePermissionMapping.CreateRolePermissionMapping;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RolePermissionMapping.GetAllRolePermissionMappings;

public sealed class GetAllRolePermissionMappingsQueryHandler : IRequestHandler<GetAllRolePermissionMappingsQuery, List<RolePermissionMappingDto>>
{
    private readonly IQueryDbContext _queryContext;

    public GetAllRolePermissionMappingsQueryHandler(IQueryDbContext queryContext)
    {
        _queryContext = queryContext;
    }

    public async Task<List<RolePermissionMappingDto>> Handle(GetAllRolePermissionMappingsQuery request, CancellationToken cancellationToken)
    {
        var entities = await _queryContext.RolePermissionMappings
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Adapt<List<RolePermissionMappingDto>>();
    }
}
