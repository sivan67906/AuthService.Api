using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Entities;
using MediatR;
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
        return await _queryDb.Set<UserAddress>()
            .Include(a => a.User)
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
