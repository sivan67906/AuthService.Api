using AuthService.Application.Features.RoleDepartmentMapping;
using AuthService.Application.Features.RoleDepartmentMapping.CreateRoleDepartmentMapping;
using AuthService.Application.Features.RoleDepartmentMapping.GetAllRoleDepartmentMappings;
using AuthService.Application.Features.RoleDepartmentMapping.GetRoleDepartmentMappingById;
using AuthService.Application.Features.RoleDepartmentMapping.UpdateRoleDepartmentMapping;
using AuthService.Application.Features.RoleDepartmentMapping.DeleteRoleDepartmentMapping;
using AuthService.Domain.Constants;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/auth/[controller]")]
[Authorize]
public class RoleDepartmentMappingController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoleDepartmentMappingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = Roles.SuperAdmin)]
    public async Task<ActionResult<ApiResponse<RoleDepartmentMappingDto>>> Create([FromBody] CreateRoleDepartmentMappingCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<RoleDepartmentMappingDto>.SuccessResponse(result, "Role department mapping created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RoleDepartmentMappingDto>.FailResponse(ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RoleDepartmentMappingDto>>>> GetAll()
    {
        try
        {
            var query = new GetAllRoleDepartmentMappingsQuery();
            var result = await _mediator.Send(query);
            return Ok(ApiResponse<List<RoleDepartmentMappingDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<RoleDepartmentMappingDto>>.FailResponse(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<RoleDepartmentMappingDto>>> GetById(Guid id)
    {
        try
        {
            var query = new GetRoleDepartmentMappingByIdQuery(id);
            var result = await _mediator.Send(query);
            
            if (result == null)
                return NotFound(ApiResponse<RoleDepartmentMappingDto>.FailResponse("Role department mapping not found"));
            
            return Ok(ApiResponse<RoleDepartmentMappingDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RoleDepartmentMappingDto>.FailResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public async Task<ActionResult<ApiResponse<RoleDepartmentMappingDto>>> Update(Guid id, [FromBody] UpdateRoleDepartmentMappingCommand command)
    {
        try
        {
            if (id != command.Id)
                return BadRequest(ApiResponse<RoleDepartmentMappingDto>.FailResponse("ID mismatch"));

            var result = await _mediator.Send(command);
            return Ok(ApiResponse<RoleDepartmentMappingDto>.SuccessResponse(result, "Role department mapping updated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RoleDepartmentMappingDto>.FailResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var command = new DeleteRoleDepartmentMappingCommand(id);
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Role department mapping deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.FailResponse(ex.Message));
        }
    }
}
