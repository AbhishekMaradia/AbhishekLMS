using FluentValidation;
using LMS_SoulCode.Features.UserPermissions.DTOs;

namespace LMS_SoulCode.Features.UserPermissions.Validators
{
    public class AssignPermissionDtoValidator : AbstractValidator<AssignPermissionDto>
    {
        public AssignPermissionDtoValidator()
        {
            RuleFor(x => x.RoleId)
                .GreaterThan(0).WithMessage("RoleId is required and must be greater than 0.");

            RuleFor(x => x.ModuleId)
                .GreaterThan(0).WithMessage("ModuleId is required and must be greater than 0.");

            RuleFor(x => x.PermissionIds)
                .NotNull().WithMessage("PermissionIds list cannot be null.")
                .Must(x => x != null && x.Count > 0).WithMessage("At least one permission ID must be provided.");
        }
    }
}
