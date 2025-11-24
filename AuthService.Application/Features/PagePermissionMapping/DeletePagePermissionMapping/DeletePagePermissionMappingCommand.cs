namespace AuthService.Application.Features.PagePermissionMapping.DeletePagePermissionMapping;

public sealed record DeletePagePermissionMappingCommand(Guid Id) : IRequest<bool>;
