namespace AuthService.Application.Features.Auth.TwoFactor;

public record EnableTwoFactorCommand(string UserId) : IRequest<bool>;
