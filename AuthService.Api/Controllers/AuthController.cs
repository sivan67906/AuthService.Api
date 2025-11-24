using System.Security.Claims;
using AuthService.Application.Features.Auth.ChangePassword;
using AuthService.Application.Features.Auth.EmailConfirmation;
using AuthService.Application.Features.Auth.ExternalLogin;
using AuthService.Application.Features.Auth.ForgotPassword;
using AuthService.Application.Features.Auth.Login;
using AuthService.Application.Features.Auth.RefreshToken;
using AuthService.Application.Features.Auth.Register;
using AuthService.Application.Features.Auth.ResetPassword;
using AuthService.Application.Features.Auth.RevokeToken;
using AuthService.Application.Features.Auth.TwoFactor;
using AuthService.Application.Features.Profile.GetProfile;
using Microsoft.AspNetCore.Http;
using DisableTwoFactorCommand = AuthService.Application.Features.Auth.TwoFactor.DisableTwoFactorCommand;
using EnableTwoFactorCommand = AuthService.Application.Features.Auth.TwoFactor.EnableTwoFactorCommand;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpGet("debug-claims")]
    public IActionResult DebugClaims()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(claims);
    }


    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<RegisterResultDto>>> Register([FromBody] RegisterCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<RegisterResultDto>.SuccessResponse(result, "User registered."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RegisterResultDto>.FailResponse("Registration failed.",
                new() { ex.Message }));
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResultDto>>> Login([FromBody] LoginCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);

            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
            }

            // do not return refresh token in body in real-world if you rely purely on HttpOnly cookie
            return Ok(ApiResponse<LoginResultDto>.SuccessResponse(result, "Login successful."));
        }
        catch (Exception ex)
        {
            return Unauthorized(ApiResponse<LoginResultDto>.FailResponse("Login failed.", new() { ex.Message }));
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public ActionResult<ApiResponse<string>> Logout()
    {
        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        });
        return Ok(ApiResponse<string>.SuccessResponse("OK", "Logged out."));
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> Profile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<ProfileDto>.FailResponse("User not found."));
        }

        var dto = await _mediator.Send(new GetProfileQuery(userId));
        return Ok(ApiResponse<ProfileDto>.SuccessResponse(dto));
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<string>>> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        // enrich command with client IP address
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var enriched = command with { IpAddress = ip };

        await _mediator.Send(enriched);
        return Ok(ApiResponse<string>.SuccessResponse("OK", "If the email exists, a reset link was sent."));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<string>>> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        try
        {
            await _mediator.Send(command);
            return Ok(ApiResponse<string>.SuccessResponse("OK", "Password reset successful."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.FailResponse("Reset password failed.", new() { ex.Message }));
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<string>.FailResponse("User not found."));
        }

        try
        {
            var cmd = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);
            await _mediator.Send(cmd);
            return Ok(ApiResponse<string>.SuccessResponse("OK", "Password changed."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.FailResponse("Change password failed.", new() { ex.Message }));
        }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<RefreshTokenResultDto>>> RefreshToken()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var token) || string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(ApiResponse<RefreshTokenResultDto>.FailResponse("No refresh token."));
        }

        try
        {
            var result = await _mediator.Send(new RefreshTokenCommand(token));

            Response.Cookies.Append("refreshToken", result.NewRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(ApiResponse<RefreshTokenResultDto>.SuccessResponse(result, "Token refreshed."));
        }
        catch (Exception ex)
        {
            return Unauthorized(ApiResponse<RefreshTokenResultDto>.FailResponse("Refresh failed.", new() { ex.Message }));
        }
    }

    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> RevokeToken()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var token) || string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(ApiResponse<string>.FailResponse("No refresh token."));
        }

        var revoked = await _mediator.Send(new RevokeTokenCommand(token));
        if (revoked)
        {
            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
            return Ok(ApiResponse<string>.SuccessResponse("OK", "Refresh token revoked."));
        }

        return BadRequest(ApiResponse<string>.FailResponse("Token already revoked or not found."));
    }

    [HttpPost("send-confirmation-email")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<string>>> SendConfirmationEmail([FromBody] SendEmailConfirmationCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result)
        {
            // email already confirmed
            return Ok(ApiResponse<string>.SuccessResponse("OK", "Your email is already confirmed."));
        }

        return Ok(ApiResponse<string>.SuccessResponse("OK", "If the email exists, a confirmation email was sent."));
    }

    [HttpGet("confirm-email")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<string>>> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
    {
        try
        {
            await _mediator.Send(new ConfirmEmailCommand(email, token));
            return Ok(ApiResponse<string>.SuccessResponse("OK", "Email confirmed."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.FailResponse("Email confirmation failed.", new() { ex.Message }));
        }
    }

    [HttpPost("2fa/generate")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> GenerateTwoFactorCode()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<string>.FailResponse("User not found."));
        }

        await _mediator.Send(new GenerateTwoFactorCodeCommand(userId));
        return Ok(ApiResponse<string>.SuccessResponse("OK", "2FA code sent."));
    }

    [HttpPost("2fa/verify")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> VerifyTwoFactorCode([FromBody] VerifyTwoFactorRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<string>.FailResponse("User not found."));
        }

        try
        {
            await _mediator.Send(new VerifyTwoFactorCodeCommand(userId, request.Code));
            return Ok(ApiResponse<string>.SuccessResponse("OK", "2FA code verified."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.FailResponse("2FA verification failed.", new() { ex.Message }));
        }
    }



    [HttpPost("2fa/enable")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> EnableTwoFactor()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<string>.FailResponse("User not found."));
        }

        await _mediator.Send(new EnableTwoFactorCommand(userId));
        return Ok(ApiResponse<string>.SuccessResponse("OK", "Two-factor enabled."));
    }

    [HttpPost("2fa/disable")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> DisableTwoFactor()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<string>.FailResponse("User not found."));
        }

        await _mediator.Send(new DisableTwoFactorCommand(userId));
        return Ok(ApiResponse<string>.SuccessResponse("OK", "Two-factor disabled."));
    }

    public class VerifyTwoFactorRequest
    {
        public string Code { get; set; } = string.Empty;
    }

    [HttpPost("external-login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResultDto>>> ExternalLogin([FromBody] ExternalLoginCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);

            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
            }

            return Ok(ApiResponse<LoginResultDto>.SuccessResponse(result, "External login successful."));
        }
        catch (Exception ex)
        {
            return Unauthorized(ApiResponse<LoginResultDto>.FailResponse("External login failed.", new() { ex.Message }));
        }
    }
}
