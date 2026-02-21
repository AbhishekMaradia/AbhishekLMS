using FluentValidation;
using LMS_SoulCode.Features.Auth.Models;
using LMS_SoulCode.Features.Auth.DTOs;

namespace LMS_SoulCode.Features.Auth.Validators
{

        public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
        {
            public RegisterRequestValidator()
            {
                RuleFor(x => x.UserName).NotEmpty().WithMessage("Username is required");

                RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required");

                RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required");

                RuleFor(x => x.Mobile).NotEmpty().WithMessage("Mobile number is required");

                RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required").EmailAddress().WithMessage("Invalid email format");

                RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required").MinimumLength(6).WithMessage("Password must be at least 6 characters long");
            }
        }

    
}
