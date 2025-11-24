namespace AuthService.Application.Features.PagePermissionMapping.UpdatePagePermissionMapping;

public sealed class UpdatePagePermissionMappingCommandValidator : AbstractValidator<UpdatePagePermissionMappingCommand>
{
    public UpdatePagePermissionMappingCommandValidator()
    {
        RuleFor(x => x.PageId)
            .NotEmpty().WithMessage("PageId is required");

        RuleFor(x => x.PermissionId)
            .NotEmpty().WithMessage("PermissionId is required");
    }
}
