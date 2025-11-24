using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Department.CreateDepartment;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Department.GetDepartment;

public sealed class GetDepartmentQueryHandler : IRequestHandler<GetDepartmentQuery, DepartmentDto?>
{
    private readonly IQueryDbContext _queryContext;

    public GetDepartmentQueryHandler(IQueryDbContext queryContext)
    {
        _queryContext = queryContext;
    }

    public async Task<DepartmentDto?> Handle(GetDepartmentQuery request, CancellationToken cancellationToken)
    {
        var entity = await _queryContext.Departments
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        return entity?.Adapt<DepartmentDto>();
    }
}
