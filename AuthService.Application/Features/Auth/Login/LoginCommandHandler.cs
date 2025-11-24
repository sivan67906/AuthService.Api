using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Application.Features.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResultDto>
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;
    private readonly ICommandDbContext _commandDb;
    private readonly IMediator _mediator;

    public LoginCommandHandler(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration config,
        ICommandDbContext commandDb,
        IMediator mediator)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _config = config;
        _commandDb = commandDb;
        _mediator = mediator;
    }

    public async Task<LoginResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new InvalidOperationException("Invalid credentials.");

        if (!user.EmailConfirmed)
        {
            throw new InvalidOperationException("Email not confirmed.");
        }

        var passwordResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);
        if (!passwordResult.Succeeded)
        {
            throw new InvalidOperationException("Invalid credentials.");
        }

        var roles = await _userManager.GetRolesAsync(user);


        var fullName = string.Join(" ", new[]
        {
            user.FirstName,
            user.LastName
        }.Where(s => !string.IsNullOrWhiteSpace(s)));

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
        var expires = DateTime.UtcNow.AddMinutes(15);

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


        var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"]
        });

        var accessToken = tokenHandler.WriteToken(token);

        var refresh = new UserRefreshToken
        {
            UserId = user.Id,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };
        _commandDb.Set<UserRefreshToken>().Add(refresh);
        await _commandDb.SaveChangesAsync(cancellationToken);

        return new LoginResultDto
        {
            AccessToken = accessToken,
            ExpiresInSeconds = (int)(expires - DateTime.UtcNow).TotalSeconds,
            RefreshToken = refresh.Token
        };
    }
}
