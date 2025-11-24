namespace AuthService.Application.Features.RoleHierarchyMapping.CreateRoleHierarchyMapping;
using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

public sealed record CreateRoleHierarchyMappingCommand : IRequest<RoleHierarchyMappingDto>
{
    public Guid ParentRoleId { get; init; }
    public Guid ChildRoleId { get; init; }
    public int Level { get; init; }
}

public sealed class CreateRoleHierarchyMappingCommandHandler : IRequestHandler<CreateRoleHierarchyMappingCommand, RoleHierarchyMappingDto>
{
    private readonly ICommandDbContext _context;

    public CreateRoleHierarchyMappingCommandHandler(ICommandDbContext context)
    {
        _context = context;
    }

    public async Task<RoleHierarchyMappingDto> Handle(CreateRoleHierarchyMappingCommand request, CancellationToken cancellationToken)
    {
        // Validate that parent and child roles exist
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

        // Check if mapping already exists
        var existingMapping = await _context.RoleHierarchies
            .FirstOrDefaultAsync(rh => rh.ParentRoleId == request.ParentRoleId && 
                                      rh.ChildRoleId == request.ChildRoleId, cancellationToken);
        
        if (existingMapping != null)
            throw new InvalidOperationException("Role hierarchy mapping already exists");

        // Prevent circular hierarchy
        if (request.ParentRoleId == request.ChildRoleId)
            throw new InvalidOperationException("A role cannot be its own parent");

        var entity = new Domain.Entities.RoleHierarchy
        {
            Id = Guid.NewGuid(),
            ParentRoleId = request.ParentRoleId,
            ChildRoleId = request.ChildRoleId,
            Level = request.Level,
            CreatedAt = DateTime.UtcNow
        };

        _context.RoleHierarchies.Add(entity);
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
