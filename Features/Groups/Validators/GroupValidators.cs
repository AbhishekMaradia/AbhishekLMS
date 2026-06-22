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
            // Only validate GroupName length when it is provided (not null)
            RuleFor(x => x.GroupName)
                .NotEmpty().WithMessage("Group Name cannot be empty.")
                .MaximumLength(100).WithMessage("Group Name must not exceed 100 characters.")
                .When(x => x.GroupName != null);
        }
    }

    public class BulkUpdateCoursesRequestValidator : AbstractValidator<BulkUpdateCoursesRequest>
    {
        public BulkUpdateCoursesRequestValidator()
        {
            RuleFor(x => x.GroupId)
                .GreaterThan(0).WithMessage("A valid Group ID is required.");

            RuleFor(x => x.Courses)
                .NotNull().WithMessage("Courses list cannot be null.")
                .NotEmpty().WithMessage("At least one course must be provided.");

            RuleForEach(x => x.Courses)
                .ChildRules(course =>
                {
                    course.RuleFor(c => c.CourseId)
                        .GreaterThan(0).WithMessage("Each course must have a valid Course ID.");
                });
        }
    }
}
