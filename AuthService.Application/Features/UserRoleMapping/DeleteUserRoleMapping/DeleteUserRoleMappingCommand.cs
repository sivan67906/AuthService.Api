namespace AuthService.Application.Features.UserRoleMapping.DeleteUserRoleMapping;
using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

public sealed record DeleteUserRoleMappingCommand(Guid Id) : IRequest<bool>;

public sealed class DeleteUserRoleMappingCommandHandler : IRequestHandler<DeleteUserRoleMappingCommand, bool>
{
    private readonly ICommandDbContext _context;

    public DeleteUserRoleMappingCommandHandler(ICommandDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteUserRoleMappingCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.UserRoleMappings
            .FirstOrDefaultAsync(urm => urm.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new InvalidOperationException("User role mapping not found");

        _context.UserRoleMappings.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
