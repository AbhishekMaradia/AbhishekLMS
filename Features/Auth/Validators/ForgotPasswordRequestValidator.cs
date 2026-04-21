using FluentValidation;
using LMS_SoulCode.Features.Auth.DTOs;

namespace LMS_SoulCode.Features.Auth.Validators
{
    public class ForgotPasswordRequestValidator:AbstractValidator<ForgotPasswordRequest>
    {
        public ForgotPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .Matches(@"^[\w\.\-]+@([\w\-]+\.)+[a-zA-Z]{2,7}$")
                .WithMessage("Please enter a valid email address");
        }
        }
    }
