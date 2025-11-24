namespace AuthService.Application.Features.RolePermissionMapping.UpdateRolePermissionMapping;

public sealed class UpdateRolePermissionMappingCommandValidator : AbstractValidator<UpdateRolePermissionMappingCommand>
{
    public UpdateRolePermissionMappingCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("RoleId is required");

        RuleFor(x => x.PermissionId)
            .NotEmpty().WithMessage("PermissionId is required");
    }
}
