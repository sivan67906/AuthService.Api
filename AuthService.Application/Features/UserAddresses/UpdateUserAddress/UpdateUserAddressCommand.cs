namespace AuthService.Application.Features.UserAddresses.UpdateUserAddress;

public record UpdateUserAddressCommand(
    string Id,
    string UserId,
    string Line1,
    string? Line2,
    string City,
    string State,
    string PostalCode,
    string Country) : IRequest<bool>;
