namespace AuthService.Application.Features.UserRoleMapping.CreateUserRoleMapping;
using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

public sealed record CreateUserRoleMappingCommand : IRequest<UserRoleMappingDto>
{
    public Guid UserId { get; init; }
    public Guid RoleId { get; init; }
    public Guid? DepartmentId { get; init; }
    public string AssignedByEmail { get; init; } = string.Empty;
}

public sealed class CreateUserRoleMappingCommandHandler : IRequestHandler<CreateUserRoleMappingCommand, UserRoleMappingDto>
{
    private readonly ICommandDbContext _context;

    public CreateUserRoleMappingCommandHandler(ICommandDbContext context)
    {
        _context = context;
    }

    public async Task<UserRoleMappingDto> Handle(CreateUserRoleMappingCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException("User not found");

        var role = await _context.Roles
            .Include(r => r.Department)
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);
        if (role == null)
            throw new InvalidOperationException("Role not found");

        Domain.Entities.Department? department = null;
        if (request.DepartmentId.HasValue)
        {
            department = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == request.DepartmentId.Value, cancellationToken);
            if (department == null)
                throw new InvalidOperationException("Department not found");
        }

        var existingMapping = await _context.UserRoleMappings
            .FirstOrDefaultAsync(urm => urm.UserId == request.UserId && 
                                       urm.RoleId == request.RoleId && 
                                       urm.DepartmentId == request.DepartmentId, cancellationToken);
        if (existingMapping != null)
            throw new InvalidOperationException("User role mapping already exists");

        var entity = new Domain.Entities.UserRoleMapping
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            RoleId = request.RoleId,
            DepartmentId = request.DepartmentId,
            AssignedByEmail = request.AssignedByEmail,
            AssignedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.UserRoleMappings.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new UserRoleMappingDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            UserEmail = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            RoleId = entity.RoleId,
            RoleName = role.Name ?? string.Empty,
            DepartmentId = entity.DepartmentId,
            DepartmentName = department?.Name ?? role.Department?.Name,
            AssignedByEmail = entity.AssignedByEmail,
            AssignedAt = entity.AssignedAt,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
