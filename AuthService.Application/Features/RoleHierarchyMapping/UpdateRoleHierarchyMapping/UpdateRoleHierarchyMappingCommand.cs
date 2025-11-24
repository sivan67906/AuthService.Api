namespace AuthService.Application.Features.RoleHierarchyMapping.UpdateRoleHierarchyMapping;
using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

public sealed record UpdateRoleHierarchyMappingCommand : IRequest<RoleHierarchyMappingDto>
{
    public Guid Id { get; init; }
    public Guid ParentRoleId { get; init; }
    public Guid ChildRoleId { get; init; }
    public int Level { get; init; }
}

public sealed class UpdateRoleHierarchyMappingCommandHandler : IRequestHandler<UpdateRoleHierarchyMappingCommand, RoleHierarchyMappingDto>
{
    private readonly ICommandDbContext _context;

    public UpdateRoleHierarchyMappingCommandHandler(ICommandDbContext context)
    {
        _context = context;
    }

    public async Task<RoleHierarchyMappingDto> Handle(UpdateRoleHierarchyMappingCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.RoleHierarchies
            .Include(rh => rh.ParentRole)
                .ThenInclude(r => r.Department)
            .Include(rh => rh.ChildRole)
                .ThenInclude(r => r.Department)
            .FirstOrDefaultAsync(rh => rh.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new InvalidOperationException("Role hierarchy mapping not found");

        // Validate that roles exist
        var parentRole = await _context.Roles
            .Include(r => r.Department)
            .FirstOrDefaultAsync(r => r.Id == request.ParentRoleId, cancellationToken);
        
        if (parentRole == null)
            throw new InvalidOperationException("Parent role not found");

        var childRole = await _context.Roles
            .Include(r => r.Department)
            .FirstOrDefaultAsync(r => r.Id == request.ChildRoleId, cancellationToken);
        
        if (childRole == null)
            throw new InvalidOperationException("Child role not found");

        // Prevent circular hierarchy
        if (request.ParentRoleId == request.ChildRoleId)
            throw new InvalidOperationException("A role cannot be its own parent");

        // Check for duplicate mapping (excluding current)
        var duplicateMapping = await _context.RoleHierarchies
            .FirstOrDefaultAsync(rh => rh.Id != request.Id && 
                                      rh.ParentRoleId == request.ParentRoleId && 
                                      rh.ChildRoleId == request.ChildRoleId, cancellationToken);
        
        if (duplicateMapping != null)
            throw new InvalidOperationException("Role hierarchy mapping already exists for these roles");

        entity.ParentRoleId = request.ParentRoleId;
        entity.ChildRoleId = request.ChildRoleId;
        entity.Level = request.Level;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.RoleHierarchies.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new RoleHierarchyMappingDto
        {
            Id = entity.Id,
            ParentRoleId = entity.ParentRoleId,
            ParentRoleName = parentRole.Name ?? string.Empty,
            ParentDepartmentId = parentRole.DepartmentId,
            ParentDepartmentName = parentRole.Department?.Name,
            ChildRoleId = entity.ChildRoleId,
            ChildRoleName = childRole.Name ?? string.Empty,
            ChildDepartmentId = childRole.DepartmentId,
            ChildDepartmentName = childRole.Department?.Name,
            Level = entity.Level,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
