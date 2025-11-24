namespace AuthService.Application.Features.RolePermissionMapping.CreateRolePermissionMapping;

public sealed class CreateRolePermissionMappingCommandValidator : AbstractValidator<CreateRolePermissionMappingCommand>
{
    public CreateRolePermissionMappingCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("RoleId is required");

        RuleFor(x => x.PermissionId)
            .NotEmpty().WithMessage("PermissionId is required");
    }
}
