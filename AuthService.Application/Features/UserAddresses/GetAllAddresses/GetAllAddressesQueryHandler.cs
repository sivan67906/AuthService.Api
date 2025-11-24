using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.UserAddresses.GetAllAddresses;

public class GetAllAddressesQueryHandler : IRequestHandler<GetAllAddressesQuery, List<AllAddressItemDto>>
{
    private readonly IQueryDbContext _queryDb;

    public GetAllAddressesQueryHandler(IQueryDbContext queryDb)
    {
        _queryDb = queryDb;
    }

    public async Task<List<AllAddressItemDto>> Handle(GetAllAddressesQuery request, CancellationToken cancellationToken)
    {
        return await _queryDb.UserAddresses
    .AsNoTracking()
    .Select(a => new AllAddressItemDto
    {

        Id = a.Id.ToString(),
        UserId = a.UserId.ToString(),
        Email = a.User != null ? a.User.Email ?? string.Empty : string.Empty,
        Line1 = a.Line1,
        City = a.City
    })
            .ToListAsync(cancellationToken);
    }
}
