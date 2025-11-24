namespace AuthService.Application.Features.PagePermissionMapping.CreatePagePermissionMapping;

public sealed class CreatePagePermissionMappingCommandValidator : AbstractValidator<CreatePagePermissionMappingCommand>
{
    public CreatePagePermissionMappingCommandValidator()
    {
        RuleFor(x => x.PageId)
            .NotEmpty().WithMessage("PageId is required");

        RuleFor(x => x.PermissionId)
            .NotEmpty().WithMessage("PermissionId is required");
    }
}
