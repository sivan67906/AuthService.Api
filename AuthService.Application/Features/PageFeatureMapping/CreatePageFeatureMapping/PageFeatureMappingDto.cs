namespace AuthService.Application.Features.PageFeatureMapping.CreatePageFeatureMapping;

public sealed record PageFeatureMappingDto(
    Guid Id,
    Guid PageId,
    Guid FeatureId,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
