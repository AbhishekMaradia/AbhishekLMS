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
            
        }
    }
}
