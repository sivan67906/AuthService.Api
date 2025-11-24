using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.UserAddresses.GetUserAddresses;

public class GetUserAddressesQueryHandler : IRequestHandler<GetUserAddressesQuery, List<UserAddressListItemDto>>
{
    private readonly IQueryDbContext _queryDb;

    public GetUserAddressesQueryHandler(IQueryDbContext queryDb)
    {
        _queryDb = queryDb;
    }

    public async Task<List<UserAddressListItemDto>> Handle(GetUserAddressesQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
        {
            throw new ArgumentException("Invalid user id");
        }

        return await _queryDb.UserAddresses
            .Where(x => x.UserId == userId)
            .Select(x => new UserAddressListItemDto
            {
                Id = x.Id.ToString(),
                Line1 = x.Line1,
                Line2 = x.Line2,
                City = x.City,
                State = x.State,
                PostalCode = x.PostalCode,
                Country = x.Country
            })
            .ToListAsync(cancellationToken);
    }
}
