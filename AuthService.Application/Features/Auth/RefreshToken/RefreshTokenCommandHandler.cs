using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Application.Features.Auth.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResultDto>
{
    private readonly ICommandDbContext _commandDb;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;

    public RefreshTokenCommandHandler(
        ICommandDbContext commandDb,
        UserManager<ApplicationUser> userManager,
        IConfiguration config)
    {
        _commandDb = commandDb;
        _userManager = userManager;
        _config = config;
    }

    public async Task<RefreshTokenResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existing = await _commandDb.Set<UserRefreshToken>()
            .Where(x => x.Token == request.RefreshToken && !x.IsRevoked)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is null || existing.ExpiresAt <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Invalid or expired refresh token.");
        }

        var user = await _userManager.FindByIdAsync(existing.UserId.ToString())
            ?? throw new InvalidOperationException("User not found.");

        // revoke old token
        existing.IsRevoked = true;
        existing.ReplacedByToken = Guid.NewGuid().ToString("N");

        var newRefresh = new UserRefreshToken
        {
            UserId = user.Id,
            Token = existing.ReplacedByToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };
        _commandDb.Set<UserRefreshToken>().Add(newRefresh);


        var roles = await _userManager.GetRolesAsync(user);

        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
        var expires = DateTime.UtcNow.AddMinutes(15);

        var fullName = string.Join(" ", new[]
        {
            user.FirstName,
            user.LastName
        }.Where(s => !string.IsNullOrWhiteSpace(s)));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
            new(ClaimTypes.Surname, user.LastName ?? string.Empty),
            new(ClaimTypes.Name, string.IsNullOrWhiteSpace(fullName)
                ? (user.Email ?? string.Empty)
                : fullName)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }


        var token = handler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"]
        });

        var accessToken = handler.WriteToken(token);

        await _commandDb.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResultDto
        {
            AccessToken = accessToken,
            ExpiresInSeconds = (int)(expires - DateTime.UtcNow).TotalSeconds,
            NewRefreshToken = newRefresh.Token
        };
    }
}
