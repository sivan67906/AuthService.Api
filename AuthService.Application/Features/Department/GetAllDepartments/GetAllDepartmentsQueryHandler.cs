using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Department.CreateDepartment;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Department.GetAllDepartments;

public sealed class GetAllDepartmentsQueryHandler : IRequestHandler<GetAllDepartmentsQuery, List<DepartmentDto>>
{
    private readonly IQueryDbContext _queryContext;

    public GetAllDepartmentsQueryHandler(IQueryDbContext queryContext)
    {
        _queryContext = queryContext;
    }

    public async Task<List<DepartmentDto>> Handle(GetAllDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var entities = await _queryContext.Departments
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Adapt<List<DepartmentDto>>();
    }
}
