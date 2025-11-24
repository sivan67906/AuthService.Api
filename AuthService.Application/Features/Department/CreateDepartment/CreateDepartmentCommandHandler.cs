using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Interfaces;

namespace AuthService.Application.Features.Department.CreateDepartment;

public sealed class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, DepartmentDto>
{
    private readonly ICommandDbContext _commandContext;
    private readonly IQueryDbContext _queryContext;

    public CreateDepartmentCommandHandler(ICommandDbContext commandContext, IQueryDbContext queryContext)
    {
        _commandContext = commandContext;
        _queryContext = queryContext;
    }

    public async Task<DepartmentDto> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var entity = new Domain.Entities.Department
        {
            Name = request.Name,
            Description = request.Description
        };

        _commandContext.Departments.Add(entity);
        await _commandContext.SaveChangesAsync(cancellationToken);

        // Sync to query database
        var queryEntity = entity.Adapt<Domain.Entities.Department>();
        _queryContext.Departments.Add(queryEntity);
        await _queryContext.SaveChangesAsync(cancellationToken);

        return entity.Adapt<DepartmentDto>();
    }
}
