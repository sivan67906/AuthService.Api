using AuthService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Features.Auth.TwoFactor;

public class GenerateTwoFactorCodeCommandHandler : IRequestHandler<GenerateTwoFactorCodeCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _email;

    public GenerateTwoFactorCodeCommandHandler(UserManager<ApplicationUser> userManager, IEmailService email)
    {
        _userManager = userManager;
        _email = email;
    }

    public async Task<bool> Handle(GenerateTwoFactorCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId)
            ?? throw new InvalidOperationException("User not found.");

        if (!user.TwoFactorEnabled)
        {
            user.TwoFactorEnabled = true;
            await _userManager.UpdateAsync(user);
        }

        var code = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
        await _email.SendAsync(user.Email!, "Your 2FA code", $"Your security code is: {code}", cancellationToken);

        return true;
    }
}
