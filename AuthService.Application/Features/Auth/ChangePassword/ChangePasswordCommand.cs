namespace AuthService.Application.Features.Auth.ChangePassword;

public record ChangePasswordCommand(string UserId, string CurrentPassword, string NewPassword) : IRequest<bool>;
