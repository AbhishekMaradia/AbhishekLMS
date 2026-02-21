using FluentValidation;
using LMS_SoulCode.Features.Course.DTOs;

namespace LMS_SoulCode.Features.Course.Validators
{
    public class CategoryRequestValidator : AbstractValidator<CategoryRequest>
    {
        public CategoryRequestValidator()
        {
            //RuleFor(x => x.Token).NotEmpty().WithMessage("Token is required.");
            //RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6).WithMessage("Password must be at least 6 characters.");
            //RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword).WithMessage("Passwords must match.");
        }
    }
}