using System.Security.Claims;
using AuthService.Application.Features.UserAddresses.CreateUserAddress;
using AuthService.Application.Features.UserAddresses.DeleteUserAddress;
using AuthService.Application.Features.UserAddresses.GetAllAddresses;
using AuthService.Application.Features.UserAddresses.GetUserAddresses;
using AuthService.Application.Features.UserAddresses.UpdateUserAddress;
using AuthService.Domain.Constants;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/auth/user/addresses")]
[Authorize]
public class UserAddressesController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserAddressesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<UserAddressListItemDto>>>> GetMyAddresses()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _mediator.Send(new GetUserAddressesQuery(userId!));
        return Ok(ApiResponse<List<UserAddressListItemDto>>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserAddressDto>>> Create([FromBody] CreateUserAddressRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var cmd = new CreateUserAddressCommand(
            userId,
            request.Line1,
            request.Line2,
            request.City,
            request.State,
            request.PostalCode,
            request.Country);

        var dto = await _mediator.Send(cmd);
        return Ok(ApiResponse<UserAddressDto>.SuccessResponse(dto, "Address created."));
    }

    public class CreateUserAddressRequest
    {
        public string Line1 { get; set; } = string.Empty;
        public string? Line2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<string>>> Update(string id, [FromBody] CreateUserAddressRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var cmd = new UpdateUserAddressCommand(
            id,
            userId,
            request.Line1,
            request.Line2,
            request.City,
            request.State,
            request.PostalCode,
            request.Country);

        var ok = await _mediator.Send(cmd);
        if (!ok)
        {
            return NotFound(ApiResponse<string>.FailResponse("Address not found."));
        }

        return Ok(ApiResponse<string>.SuccessResponse("OK", "Address updated."));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var ok = await _mediator.Send(new DeleteUserAddressCommand(id, userId));
        if (!ok)
        {
            return NotFound(ApiResponse<string>.FailResponse("Address not found."));
        }

        return Ok(ApiResponse<string>.SuccessResponse("OK", "Address deleted."));
    }

    // Admin-only endpoint example
    [HttpGet("admin/all")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ApiResponse<List<AllAddressItemDto>>>> GetAllAddresses()
    {
        var result = await _mediator.Send(new GetAllAddressesQuery());
        return Ok(ApiResponse<List<AllAddressItemDto>>.SuccessResponse(result));
    }
}
