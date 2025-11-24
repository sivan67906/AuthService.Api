namespace AuthService.Application.Features.RoleHierarchyMapping.DeleteRoleHierarchyMapping;
using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

public sealed record DeleteRoleHierarchyMappingCommand(Guid Id) : IRequest<bool>;

public sealed class DeleteRoleHierarchyMappingCommandHandler : IRequestHandler<DeleteRoleHierarchyMappingCommand, bool>
{
    private readonly ICommandDbContext _context;

    public DeleteRoleHierarchyMappingCommandHandler(ICommandDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteRoleHierarchyMappingCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.RoleHierarchies
            .FirstOrDefaultAsync(rh => rh.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new InvalidOperationException("Role hierarchy mapping not found");

        _context.RoleHierarchies.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
