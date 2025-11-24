using AuthService.Application.Features.PagePermissionMapping.CreatePagePermissionMapping;

namespace AuthService.Application.Features.PagePermissionMapping.GetPagePermissionMapping;

public sealed record GetPagePermissionMappingQuery(Guid Id) : IRequest<PagePermissionMappingDto?>;
