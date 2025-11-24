using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.PagePermissionMapping.CreatePagePermissionMapping;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.PagePermissionMapping.GetAllPagePermissionMappings;

public sealed class GetAllPagePermissionMappingsQueryHandler : IRequestHandler<GetAllPagePermissionMappingsQuery, List<PagePermissionMappingDto>>
{
    private readonly IQueryDbContext _queryContext;

    public GetAllPagePermissionMappingsQueryHandler(IQueryDbContext queryContext)
    {
        _queryContext = queryContext;
    }

    public async Task<List<PagePermissionMappingDto>> Handle(GetAllPagePermissionMappingsQuery request, CancellationToken cancellationToken)
    {
        var entities = await _queryContext.PagePermissionMappings
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Adapt<List<PagePermissionMappingDto>>();
    }
}
