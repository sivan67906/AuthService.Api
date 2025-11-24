namespace AuthService.Application.Features.UserAddresses.CreateUserAddress;

public record CreateUserAddressCommand(
    string UserId,
    string Line1,
    string? Line2,
    string City,
    string State,
    string PostalCode,
    string Country) : IRequest<UserAddressDto>;
