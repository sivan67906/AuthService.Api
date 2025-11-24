using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RoleDepartmentMapping.UpdateRoleDepartmentMapping;

public sealed record UpdateRoleDepartmentMappingCommand : IRequest<RoleDepartmentMappingDto>
{
    public Guid Id { get; init; }
    public Guid RoleId { get; init; }
    public Guid DepartmentId { get; init; }
    public bool IsPrimary { get; init; }
}

public sealed class UpdateRoleDepartmentMappingCommandHandler : IRequestHandler<UpdateRoleDepartmentMappingCommand, RoleDepartmentMappingDto>
{
    private readonly ICommandDbContext _context;

    public UpdateRoleDepartmentMappingCommandHandler(ICommandDbContext context)
    {
        _context = context;
    }

    public async Task<RoleDepartmentMappingDto> Handle(UpdateRoleDepartmentMappingCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.RoleDepartmentMappings
            .Include(rdm => rdm.Role)
            .Include(rdm => rdm.Department)
            .FirstOrDefaultAsync(rdm => rdm.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new InvalidOperationException("Role department mapping not found");

        var role = await _context.Set<Domain.Entities.ApplicationRole>()
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);
        if (role == null)
            throw new InvalidOperationException("Role not found");

        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == request.DepartmentId, cancellationToken);
        if (department == null)
            throw new InvalidOperationException("Department not found");

        var duplicateMapping = await _context.RoleDepartmentMappings
            .FirstOrDefaultAsync(rdm => rdm.Id != request.Id && 
                                       rdm.RoleId == request.RoleId && 
                                       rdm.DepartmentId == request.DepartmentId, cancellationToken);
        if (duplicateMapping != null)
            throw new InvalidOperationException("Role department mapping already exists");

        entity.RoleId = request.RoleId;
        entity.DepartmentId = request.DepartmentId;
        entity.IsPrimary = request.IsPrimary;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.RoleDepartmentMappings.Update(entity);
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