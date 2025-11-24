using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Features.Auth.TwoFactor;

public class EnableTwoFactorCommandHandler : IRequestHandler<EnableTwoFactorCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public EnableTwoFactorCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> Handle(EnableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.UserId, out var userGuid))
        {
            throw new InvalidOperationException("Invalid user identifier.");
        }

        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        if (!user.TwoFactorEnabled)
        {
            user.TwoFactorEnabled = true;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Failed to enable two-factor authentication.");
            }
        }

        return true;
    }
}
