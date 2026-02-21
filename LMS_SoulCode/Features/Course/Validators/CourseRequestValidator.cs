using FluentValidation;
using LMS_SoulCode.Features.Course.DTOs;

namespace LMS_SoulCode.Features.Course.Validators
{
    public class CourseRequestValidator : AbstractValidator<CourseRequest>
    {
        public CourseRequestValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Instructor).NotEmpty();
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
            RuleFor(x => x.DurationHours).GreaterThan(0);
        }
    }
}
