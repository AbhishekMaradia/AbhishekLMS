using FluentValidation;
using LMS_SoulCode.Features.Auth.DTOs;

namespace LMS_SoulCode.Features.Auth.Validators
{

    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email, Mobile or Username is required");

            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
        }
              
    }
}
