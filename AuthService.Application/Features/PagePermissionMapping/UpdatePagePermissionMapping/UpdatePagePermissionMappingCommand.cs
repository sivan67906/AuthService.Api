using AuthService.Application.Features.PagePermissionMapping.CreatePagePermissionMapping;

namespace AuthService.Application.Features.PagePermissionMapping.UpdatePagePermissionMapping;

public sealed record UpdatePagePermissionMappingCommand(
    Guid Id,
    Guid PageId,
    Guid PermissionId
) : IRequest<PagePermissionMappingDto>;
