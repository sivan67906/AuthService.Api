namespace AuthService.Application.Features.Feature.CreateFeature;

public sealed record CreateFeatureCommand(
    string Name,
    string? Description,
    string? RouteUrl,
    bool IsMainMenu,
    Guid? ParentFeatureId,
    int DisplayOrder,
    int Level,
    string? Icon
) : IRequest<FeatureDto>;
