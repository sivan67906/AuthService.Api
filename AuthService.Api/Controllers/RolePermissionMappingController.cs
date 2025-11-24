using AuthService.Application.Features.RolePermissionMapping.CreateRolePermissionMapping;
using AuthService.Application.Features.RolePermissionMapping.DeleteRolePermissionMapping;
using AuthService.Application.Features.RolePermissionMapping.GetAllRolePermissionMappings;
using AuthService.Application.Features.RolePermissionMapping.GetRolePermissionMapping;
using AuthService.Application.Features.RolePermissionMapping.UpdateRolePermissionMapping;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolePermissionMappingController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolePermissionMappingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,FinanceAdmin")]
    public async Task<ActionResult<ApiResponse<RolePermissionMappingDto>>> Create([FromBody] CreateRolePermissionMappingCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<RolePermissionMappingDto>.SuccessResponse(result, "RolePermissionMapping created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RolePermissionMappingDto>.FailResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,FinanceAdmin")]
    public async Task<ActionResult<ApiResponse<RolePermissionMappingDto>>> Update(Guid id, [FromBody] UpdateRolePermissionMappingCommand command)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest(ApiResponse<RolePermissionMappingDto>.FailResponse("ID mismatch"));
            }

            var result = await _mediator.Send(command);
            return Ok(ApiResponse<RolePermissionMappingDto>.SuccessResponse(result, "RolePermissionMapping updated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RolePermissionMappingDto>.FailResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,FinanceAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var command = new DeleteRolePermissionMappingCommand(id);
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "RolePermissionMapping deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.FailResponse(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<RolePermissionMappingDto>>> Get(Guid id)
    {
        try
        {
            var query = new GetRolePermissionMappingQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(ApiResponse<RolePermissionMappingDto>.FailResponse("RolePermissionMapping not found"));
            }

            return Ok(ApiResponse<RolePermissionMappingDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RolePermissionMappingDto>.FailResponse(ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RolePermissionMappingDto>>>> GetAll()
    {
        try
        {
            var query = new GetAllRolePermissionMappingsQuery();
            var result = await _mediator.Send(query);
            return Ok(ApiResponse<List<RolePermissionMappingDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<RolePermissionMappingDto>>.FailResponse(ex.Message));
        }
    }
}
