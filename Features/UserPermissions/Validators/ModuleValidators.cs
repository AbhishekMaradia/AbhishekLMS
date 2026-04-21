using FluentValidation;
using static LMS_SoulCode.Features.UserPermissions.DTOs.ModuleDtos;

namespace LMS_SoulCode.Features.UserPermissions.Validators
{
    public class CreateModuleDtoValidator : AbstractValidator<CreateModuleDto>
    {
        public CreateModuleDtoValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Module Code is required.")
                .MaximumLength(50).WithMessage("Module Code cannot exceed 50 characters.")
                .Matches("^[A-Z0-9_]+$").WithMessage("Module Code must be uppercase, alphanumeric, and can contain underscores.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Module Name is required.")
                .MaximumLength(100).WithMessage("Module Name cannot exceed 100 characters.");
        }
    }

    public class UpdateModuleDtoValidator : AbstractValidator<UpdateModuleDto>
    {
        public UpdateModuleDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Module Name is required.")
                .MaximumLength(100).WithMessage("Module Name cannot exceed 100 characters.");
        }
    }

    public class AssignModulePermissionsDtoValidator : AbstractValidator<AssignModulePermissionsDto>
    {
        public AssignModulePermissionsDtoValidator()
        {
            RuleFor(x => x.ModuleId)
                .GreaterThan(0).WithMessage("ModuleId is required and must be greater than 0.");

            RuleFor(x => x.PermissionIds)
                .NotNull().WithMessage("PermissionIds list cannot be null.")
                .Must(x => x != null && x.Count > 0).WithMessage("At least one permission ID must be provided.");
        }
    }
}
