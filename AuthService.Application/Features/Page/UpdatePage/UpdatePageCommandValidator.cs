namespace AuthService.Application.Features.Page.UpdatePage;

public sealed class UpdatePageCommandValidator : AbstractValidator<UpdatePageCommand>
{
    public UpdatePageCommandValidator()
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
