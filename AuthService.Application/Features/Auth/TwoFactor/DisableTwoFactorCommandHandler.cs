using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Features.Auth.TwoFactor;

public class DisableTwoFactorCommandHandler : IRequestHandler<DisableTwoFactorCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DisableTwoFactorCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> Handle(DisableTwoFactorCommand request, CancellationToken cancellationToken)
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

        if (user.TwoFactorEnabled)
        {
            user.TwoFactorEnabled = false;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Failed to disable two-factor authentication.");
            }
        }

        return true;
    }
}
