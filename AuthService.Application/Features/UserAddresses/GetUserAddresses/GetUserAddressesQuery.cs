namespace AuthService.Application.Features.UserAddresses.GetUserAddresses;

public record GetUserAddressesQuery(string UserId) : IRequest<List<UserAddressListItemDto>>;
