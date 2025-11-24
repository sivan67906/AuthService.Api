using AuthService.Application.Features.PageFeatureMapping.CreatePageFeatureMapping;

namespace AuthService.Application.Features.PageFeatureMapping.UpdatePageFeatureMapping;

public sealed record UpdatePageFeatureMappingCommand(
    Guid Id,
    Guid PageId,
    Guid FeatureId
) : IRequest<PageFeatureMappingDto>;
