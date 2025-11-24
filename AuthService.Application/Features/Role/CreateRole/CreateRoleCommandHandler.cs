using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Role.CreateRole;

public sealed class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleDto>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IQueryDbContext _queryContext;

    public CreateRoleCommandHandler(RoleManager<ApplicationRole> roleManager, IQueryDbContext queryContext)
    {
        _roleManager = roleManager;
        _queryContext = queryContext;
    }

    public async Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        // Check if role already exists
        var existingRole = await _roleManager.FindByNameAsync(request.Name);
        if (existingRole != null)
        {
            throw new InvalidOperationException($"Role with name '{request.Name}' already exists");
        }

        // Validate department if provided
        if (request.DepartmentId.HasValue)
        {
            var departmentExists = await _queryContext.Departments
                .AnyAsync(d => d.Id == request.DepartmentId.Value && !d.IsDeleted, cancellationToken);

            if (!departmentExists)
            {
                throw new InvalidOperationException($"Department with ID '{request.DepartmentId.Value}' not found");
            }
        }

        var role = new ApplicationRole
        {
            Name = request.Name,
            Description = request.Description,
            DepartmentId = request.DepartmentId
        };

        var result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        string? departmentName = null;
        if (request.DepartmentId.HasValue)
        {
            var department = await _queryContext.Departments
                .FirstOrDefaultAsync(d => d.Id == request.DepartmentId.Value, cancellationToken);
            departmentName = department?.Name;
        }

        return new RoleDto(role.Id, role.Name!, role.Description, role.DepartmentId, departmentName);
    }
}
