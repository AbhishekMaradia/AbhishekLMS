using FluentValidation;
using static LMS_SoulCode.Features.UserPermissions.DTOs.RoleDtos;

namespace LMS_SoulCode.Features.UserPermissions.Validators
{
    public class CreateRoleDtoValidator : AbstractValidator<CreateRoleDto>
    {
        public CreateRoleDtoValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Role Code is required.")
                .MaximumLength(50).WithMessage("Role Code cannot exceed 50 characters.")
                .Matches("^[A-Z0-9_]+$").WithMessage("Role Code must be uppercase, alphanumeric, and can contain underscores.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Role Name is required.")
                .MaximumLength(100).WithMessage("Role Name cannot exceed 100 characters.");
        }
    }

    public class UpdateRoleDtoValidator : AbstractValidator<UpdateRoleDto>
    {
        public UpdateRoleDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Role Name is required.")
                .MaximumLength(100).WithMessage("Role Name cannot exceed 100 characters.");
        }
    }
}
