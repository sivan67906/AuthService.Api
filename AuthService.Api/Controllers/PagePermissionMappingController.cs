using AuthService.Application.Features.PagePermissionMapping.CreatePagePermissionMapping;
using AuthService.Application.Features.PagePermissionMapping.DeletePagePermissionMapping;
using AuthService.Application.Features.PagePermissionMapping.GetAllPagePermissionMappings;
using AuthService.Application.Features.PagePermissionMapping.GetPagePermissionMapping;
using AuthService.Application.Features.PagePermissionMapping.UpdatePagePermissionMapping;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PagePermissionMappingController : ControllerBase
{
    private readonly IMediator _mediator;

    public PagePermissionMappingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,FinanceAdmin")]
    public async Task<ActionResult<ApiResponse<PagePermissionMappingDto>>> Create([FromBody] CreatePagePermissionMappingCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<PagePermissionMappingDto>.SuccessResponse(result, "PagePermissionMapping created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PagePermissionMappingDto>.FailResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,FinanceAdmin")]
    public async Task<ActionResult<ApiResponse<PagePermissionMappingDto>>> Update(Guid id, [FromBody] UpdatePagePermissionMappingCommand command)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest(ApiResponse<PagePermissionMappingDto>.FailResponse("ID mismatch"));
            }

            var result = await _mediator.Send(command);
            return Ok(ApiResponse<PagePermissionMappingDto>.SuccessResponse(result, "PagePermissionMapping updated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PagePermissionMappingDto>.FailResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,FinanceAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var command = new DeletePagePermissionMappingCommand(id);
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "PagePermissionMapping deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.FailResponse(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PagePermissionMappingDto>>> Get(Guid id)
    {
        try
        {
            var query = new GetPagePermissionMappingQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(ApiResponse<PagePermissionMappingDto>.FailResponse("PagePermissionMapping not found"));
            }

            return Ok(ApiResponse<PagePermissionMappingDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PagePermissionMappingDto>.FailResponse(ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PagePermissionMappingDto>>>> GetAll()
    {
        try
        {
            var query = new GetAllPagePermissionMappingsQuery();
            var result = await _mediator.Send(query);
            return Ok(ApiResponse<List<PagePermissionMappingDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<PagePermissionMappingDto>>.FailResponse(ex.Message));
        }
    }
}
