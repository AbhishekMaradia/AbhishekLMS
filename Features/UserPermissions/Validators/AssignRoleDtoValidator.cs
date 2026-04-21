using FluentValidation;
using LMS_SoulCode.Features.UserPermissions.DTOs;

namespace LMS_SoulCode.Features.UserPermissions.Validators
{
    public class AssignRoleDtoValidator : AbstractValidator<AssignRoleDto>
    {
        public AssignRoleDtoValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("UserId is required and must be greater than 0.");

            RuleFor(x => x.RoleId)
                .GreaterThan(0).WithMessage("RoleId is required and must be greater than 0.");
        }
    }
}
