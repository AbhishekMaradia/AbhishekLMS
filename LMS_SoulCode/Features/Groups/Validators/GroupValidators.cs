using FluentValidation;
using LMS_SoulCode.Features.Groups.DTOs;

namespace LMS_SoulCode.Features.Groups.Validators
{
    public class CreateGroupRequestValidator : AbstractValidator<CreateGroupRequest>
    {
        public CreateGroupRequestValidator()
        {
            RuleFor(x => x.GroupName)
                .NotEmpty().WithMessage("Group Name is required.")
                .MaximumLength(100).WithMessage("Group Name must not exceed 100 characters.");
        }
    }

    public class UpdateGroupRequestValidator : AbstractValidator<UpdateGroupRequest>
    {
        public UpdateGroupRequestValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Group Id is required.");

            RuleFor(x => x.GroupName)
                .NotEmpty().WithMessage("Group Name is required.")
                .MaximumLength(100).WithMessage("Group Name must not exceed 100 characters.")
                .When(x => x.GroupName != null);
        }
    }
}
