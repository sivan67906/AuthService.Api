using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RoleHierarchyMapping.DeleteRoleHierarchy;

public record DeleteRoleHierarchyCommand(Guid Id) : IRequest<bool>;

public class DeleteRoleHierarchyCommandHandler : IRequestHandler<DeleteRoleHierarchyCommand, bool>
{
    private readonly ICommandDbContext _context;

    public DeleteRoleHierarchyCommandHandler(ICommandDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteRoleHierarchyCommand request, CancellationToken cancellationToken)
    {
        var roleHierarchy = await _context.RoleHierarchies
            .FirstOrDefaultAsync(rh => rh.Id == request.Id, cancellationToken);

        if (roleHierarchy == null)
            throw new Exception($"Role hierarchy with ID {request.Id} not found");

        _context.RoleHierarchies.Remove(roleHierarchy);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
