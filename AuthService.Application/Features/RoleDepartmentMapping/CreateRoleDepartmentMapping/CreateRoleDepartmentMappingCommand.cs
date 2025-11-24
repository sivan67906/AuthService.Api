using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RoleDepartmentMapping.CreateRoleDepartmentMapping;

public sealed record CreateRoleDepartmentMappingCommand : IRequest<RoleDepartmentMappingDto>
{
    public Guid RoleId { get; init; }
    public Guid DepartmentId { get; init; }
    public bool IsPrimary { get; init; } = false;
}

public sealed class CreateRoleDepartmentMappingCommandHandler : IRequestHandler<CreateRoleDepartmentMappingCommand, RoleDepartmentMappingDto>
{
    private readonly ICommandDbContext _context;

    public CreateRoleDepartmentMappingCommandHandler(ICommandDbContext context)
    {
        _context = context;
    }

    public async Task<RoleDepartmentMappingDto> Handle(CreateRoleDepartmentMappingCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Set<Domain.Entities.ApplicationRole>()
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);
        if (role == null)
            throw new InvalidOperationException("Role not found");

        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == request.DepartmentId, cancellationToken);
        if (department == null)
            throw new InvalidOperationException("Department not found");

        var existingMapping = await _context.RoleDepartmentMappings
            .FirstOrDefaultAsync(rdm => rdm.RoleId == request.RoleId && 
                                       rdm.DepartmentId == request.DepartmentId, cancellationToken);
        if (existingMapping != null)
            throw new InvalidOperationException("Role department mapping already exists");

        var entity = new Domain.Entities.RoleDepartmentMapping
        {
            Id = Guid.NewGuid(),
            RoleId = request.RoleId,
            DepartmentId = request.DepartmentId,
            IsPrimary = request.IsPrimary,
            CreatedAt = DateTime.UtcNow
        };

        _context.RoleDepartmentMappings.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new RoleDepartmentMappingDto
        {
            Id = entity.Id,
            RoleId = entity.RoleId,
            RoleName = role.Name ?? string.Empty,
            DepartmentId = entity.DepartmentId,
            DepartmentName = department.Name,
            IsPrimary = entity.IsPrimary,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
