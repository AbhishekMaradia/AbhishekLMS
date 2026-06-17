using FluentValidation;
using LMS_SoulCode.Features.Organizations.DTOs;

namespace LMS_SoulCode.Features.Organizations.Validators
{
    public class OrgRegisterRequestValidator : AbstractValidator<OrgRegisterRequest>
    {
        public OrgRegisterRequestValidator()
        {
            // Organization Rules
            RuleFor(x => x.OrgName).NotEmpty().WithMessage("Organization name is required");
            
            // Admin User Rules
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("Admin first name is required");
            RuleFor(x => x.LastName).NotEmpty().WithMessage("Admin last name is required");
            RuleFor(x => x.Email).NotEmpty().WithMessage("Admin email is required").EmailAddress().WithMessage("Invalid email format");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Admin password is required").MinimumLength(6).WithMessage("Password must be at least 6 characters long");
            RuleFor(x => x.Mobile).NotEmpty().WithMessage("Admin mobile number is required");
        }
    }
}
