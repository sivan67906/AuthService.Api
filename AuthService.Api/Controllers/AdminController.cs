using System;
using AuthService.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("admin")]
[Authorize(Policy = "RequireAdmin")]
public class AdminController : ControllerBase
{
    [HttpGet("stats")]
    public ActionResult<ApiResponse<object>> GetStats()
    {
        var payload = new
        {
            UsersOnline = 0,
            GeneratedAtUtc = DateTime.UtcNow
        };
        return Ok(ApiResponse<object>.SuccessResponse(payload, "Admin stats"));
    }
}
