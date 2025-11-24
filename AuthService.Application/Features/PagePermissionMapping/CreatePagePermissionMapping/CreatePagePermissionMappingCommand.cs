namespace AuthService.Application.Features.PagePermissionMapping.CreatePagePermissionMapping;

public sealed record CreatePagePermissionMappingCommand(
    Guid PageId,
    Guid PermissionId
) : IRequest<PagePermissionMappingDto>;
