using System.Collections.Generic;
using MediatR;

namespace AuthService.Application.Features.UserAddresses.GetAllAddresses;

public record GetAllAddressesQuery() : IRequest<List<AllAddressItemDto>>;
