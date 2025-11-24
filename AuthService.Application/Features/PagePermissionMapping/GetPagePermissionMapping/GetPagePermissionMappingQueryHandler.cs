using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.PagePermissionMapping.CreatePagePermissionMapping;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.PagePermissionMapping.GetPagePermissionMapping;

public sealed class GetPagePermissionMappingQueryHandler : IRequestHandler<GetPagePermissionMappingQuery, PagePermissionMappingDto?>
{
    private readonly IQueryDbContext _queryContext;

    public GetPagePermissionMappingQueryHandler(IQueryDbContext queryContext)
    {
        _queryContext = queryContext;
    }

    public async Task<PagePermissionMappingDto?> Handle(GetPagePermissionMappingQuery request, CancellationToken cancellationToken)
    {
        var entity = await _queryContext.PagePermissionMappings
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        return entity?.Adapt<PagePermissionMappingDto>();
    }
}
