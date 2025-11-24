namespace AuthService.Application.Features.Page.CreatePage;

public sealed class CreatePageCommandValidator : AbstractValidator<CreatePageCommand>
{
    public CreatePageCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Url is required");

        RuleFor(x => x.Url)
            .MaximumLength(100).WithMessage("Url must not exceed 100 characters");
    }
}
