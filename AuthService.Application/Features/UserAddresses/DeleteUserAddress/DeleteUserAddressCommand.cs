namespace AuthService.Application.Features.UserAddresses.DeleteUserAddress;

public record DeleteUserAddressCommand(string Id, string UserId) : IRequest<bool>;
