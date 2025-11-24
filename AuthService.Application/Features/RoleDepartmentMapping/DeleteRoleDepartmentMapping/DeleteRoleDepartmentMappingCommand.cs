using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RoleDepartmentMapping.DeleteRoleDepartmentMapping;

public sealed record DeleteRoleDepartmentMappingCommand(Guid Id) : IRequest<bool>;

public sealed class DeleteRoleDepartmentMappingCommandHandler : IRequestHandler<DeleteRoleDepartmentMappingCommand, bool>
{
    private readonly ICommandDbContext _context;

    public DeleteRoleDepartmentMappingCommandHandler(ICommandDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteRoleDepartmentMappingCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.RoleDepartmentMappings
            .FirstOrDefaultAsync(rdm => rdm.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new InvalidOperationException("Role department mapping not found");

        _context.RoleDepartmentMappings.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}