using AuthService.Application.Common.Interfaces;

namespace AuthService.Application.Features.UserAddresses.CreateUserAddress;

public class CreateUserAddressCommandHandler : IRequestHandler<CreateUserAddressCommand, UserAddressDto>
{
    private readonly ICommandDbContext _commandDb;

    public CreateUserAddressCommandHandler(ICommandDbContext commandDb)
    {
        _commandDb = commandDb;
    }

    public async Task<UserAddressDto> Handle(CreateUserAddressCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
        {
            throw new ArgumentException("Invalid user id");
        }

        var entity = new UserAddress
        {
            UserId = userId,
            Line1 = request.Line1,
            Line2 = request.Line2,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country
        };

        _commandDb.Set<UserAddress>().Add(entity);
        await _commandDb.SaveChangesAsync(cancellationToken);

        return new UserAddressDto
        {
            Id = entity.Id.ToString(),
            Line1 = entity.Line1,
            Line2 = entity.Line2,
            City = entity.City,
            State = entity.State,
            PostalCode = entity.PostalCode,
            Country = entity.Country
        };
    }
}
