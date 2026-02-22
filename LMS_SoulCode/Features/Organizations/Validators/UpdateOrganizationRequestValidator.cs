using FluentValidation;
using LMS_SoulCode.Features.Organizations.DTOs;

namespace LMS_SoulCode.Features.Organizations.Validators
{
    public class UpdateOrganizationRequestValidator : AbstractValidator<UpdateOrganizationRequest>
    {
        public UpdateOrganizationRequestValidator()
        {
            // Organization Rules
            RuleFor(x => x.OrgName).NotEmpty().WithMessage("Organization name is required");
            RuleFor(x => x.OrgCode).NotEmpty().WithMessage("Organization code is required")
                .Matches("^[a-zA-Z0-9_]*$").WithMessage("Organization code can only contain letters, numbers and underscores");
            
            // Admin User Rules
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("Admin first name is required");
            RuleFor(x => x.LastName).NotEmpty().WithMessage("Admin last name is required");
            RuleFor(x => x.Email).NotEmpty().WithMessage("Admin email is required").EmailAddress().WithMessage("Invalid email format");
            RuleFor(x => x.Mobile).NotEmpty().WithMessage("Admin mobile number is required");

            // Password Rules - Optional in update, but if provided must meet criteria
            RuleFor(x => x.Password)
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long")
                .When(x => !string.IsNullOrEmpty(x.Password));

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("Password and Confirm Password must match")
                .When(x => !string.IsNullOrEmpty(x.Password));
        }
    }
}
