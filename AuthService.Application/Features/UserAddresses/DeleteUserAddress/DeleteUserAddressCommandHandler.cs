using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.UserAddresses.DeleteUserAddress;

public class DeleteUserAddressCommandHandler : IRequestHandler<DeleteUserAddressCommand, bool>
{
    private readonly ICommandDbContext _commandDb;

    public DeleteUserAddressCommandHandler(ICommandDbContext commandDb)
    {
        _commandDb = commandDb;
    }

    public async Task<bool> Handle(DeleteUserAddressCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var id) || !Guid.TryParse(request.UserId, out var userId))
        {
            throw new ArgumentException("Invalid id");
        }

        var entity = await _commandDb.Set<UserAddress>()
            .Where(x => x.Id == id && x.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            return false;
        }

        _commandDb.Set<UserAddress>().Remove(entity);
        await _commandDb.SaveChangesAsync(cancellationToken);
        return true;
    }
}
