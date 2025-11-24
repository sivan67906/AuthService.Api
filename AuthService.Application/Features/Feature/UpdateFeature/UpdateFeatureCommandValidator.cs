namespace AuthService.Application.Features.Feature.UpdateFeature;

public sealed class UpdateFeatureCommandValidator : AbstractValidator<UpdateFeatureCommand>
{
    public UpdateFeatureCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");
    }
}
