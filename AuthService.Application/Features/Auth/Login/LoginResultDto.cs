namespace AuthService.Application.Features.Auth.Login;

public class LoginResultDto
{
    public string AccessToken { get; set; } = string.Empty;
    public int ExpiresInSeconds { get; set; }
    public string? RefreshToken { get; set; }
}
