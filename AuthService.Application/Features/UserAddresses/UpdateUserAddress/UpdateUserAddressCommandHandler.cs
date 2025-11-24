using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.UserAddresses.UpdateUserAddress;

public class UpdateUserAddressCommandHandler : IRequestHandler<UpdateUserAddressCommand, bool>
{
    private readonly ICommandDbContext _commandDb;

    public UpdateUserAddressCommandHandler(ICommandDbContext commandDb)
    {
        _commandDb = commandDb;
    }

    public async Task<bool> Handle(UpdateUserAddressCommand request, CancellationToken cancellationToken)
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

        entity.Line1 = request.Line1;
        entity.Line2 = request.Line2;
        entity.City = request.City;
        entity.State = request.State;
        entity.PostalCode = request.PostalCode;
        entity.Country = request.Country;

        await _commandDb.SaveChangesAsync(cancellationToken);
        return true;
    }
}
