namespace AuthService.Application.Features.Auth.TwoFactor;

public record DisableTwoFactorCommand(string UserId) : IRequest<bool>;
