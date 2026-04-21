using FluentValidation;
using LMS_SoulCode.Features.Auth.DTOs;

namespace LMS_SoulCode.Features.Auth.Validators
{
    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Token).NotEmpty().WithMessage("Token is required.");
            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6).WithMessage("Password must be at least 6 characters.");
            RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword).WithMessage("Passwords must match.");
        }
    }
}