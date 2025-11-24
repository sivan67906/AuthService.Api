using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Auth.RevokeToken;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, bool>
{
    private readonly ICommandDbContext _commandDb;

    public RevokeTokenCommandHandler(ICommandDbContext commandDb)
    {
        _commandDb = commandDb;
    }

    public async Task<bool> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var token = await _commandDb.Set<UserRefreshToken>()
            .Where(x => x.Token == request.RefreshToken && !x.IsRevoked)
            .FirstOrDefaultAsync(cancellationToken);

        if (token is null)
        {
            return false;
        }

        token.IsRevoked = true;
        await _commandDb.SaveChangesAsync(cancellationToken);
        return true;
    }
}
