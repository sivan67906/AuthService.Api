using Microsoft.AspNetCore.Identity;
using AuthService.Domain.Constants;

namespace AuthService.Application.Features.Auth.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResultDto>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<RegisterResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(";", result.Errors);
            throw new InvalidOperationException($"Unable to register user: {errors}");
        }

        // Assign PendingUser role by default
        await _userManager.AddToRoleAsync(user, Roles.PendingUser);

        return new RegisterResultDto
        {
            UserId = user.Id.ToString(),
            Email = user.Email!
        };
    }
}
