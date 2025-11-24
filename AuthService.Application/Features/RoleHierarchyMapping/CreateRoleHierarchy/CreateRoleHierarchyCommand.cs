using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RoleHierarchyMapping.CreateRoleHierarchy;

public record CreateRoleHierarchyCommand : IRequest<RoleHierarchyDto>
{
    public Guid ParentRoleId { get; set; }
    public Guid ChildRoleId { get; set; }
    public int Level { get; set; }
}

public class CreateRoleHierarchyCommandHandler : IRequestHandler<CreateRoleHierarchyCommand, RoleHierarchyDto>
{
    private readonly ICommandDbContext _context;

    public CreateRoleHierarchyCommandHandler(ICommandDbContext context)
    {
        _context = context;
    }

    public async Task<RoleHierarchyDto> Handle(CreateRoleHierarchyCommand request, CancellationToken cancellationToken)
    {
        // Validate that both roles exist
        var parentRole = await _context.Roles.FindAsync(new object[] { request.ParentRoleId }, cancellationToken);
        var childRole = await _context.Roles.FindAsync(new object[] { request.ChildRoleId }, cancellationToken);

        if (parentRole == null)
            throw new Exception($"Parent role with ID {request.ParentRoleId} not found");

        if (childRole == null)
            throw new Exception($"Child role with ID {request.ChildRoleId} not found");

        if (request.ParentRoleId == request.ChildRoleId)
            throw new Exception("Parent role and child role cannot be the same");

        // Check for existing mapping
        var existing = await _context.RoleHierarchies
            .FirstOrDefaultAsync(rh => rh.ParentRoleId == request.ParentRoleId && rh.ChildRoleId == request.ChildRoleId, cancellationToken);

        if (existing != null)
            throw new Exception("This role hierarchy mapping already exists");

        var roleHierarchy = new RoleHierarchy
        {
            ParentRoleId = request.ParentRoleId,
            ChildRoleId = request.ChildRoleId,
            Level = request.Level
        };

        _context.RoleHierarchies.Add(roleHierarchy);
        await _context.SaveChangesAsync(cancellationToken);

        return new RoleHierarchyDto
        {
            Id = roleHierarchy.Id,
            ParentRoleId = roleHierarchy.ParentRoleId,
            ParentRoleName = parentRole.Name ?? string.Empty,
            ChildRoleId = roleHierarchy.ChildRoleId,
            ChildRoleName = childRole.Name ?? string.Empty,
            Level = roleHierarchy.Level,
            IsActive = roleHierarchy.IsActive,
            CreatedAt = roleHierarchy.CreatedAt,
            UpdatedAt = roleHierarchy.UpdatedAt
        };
    }
}
