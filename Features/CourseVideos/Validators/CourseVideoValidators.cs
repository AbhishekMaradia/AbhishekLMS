using FluentValidation;
using LMS_SoulCode.Features.CourseVideos.DTOs;

namespace LMS_SoulCode.Features.CourseVideos.Validators
{
    public class UpdateProgressRequestValidator : AbstractValidator<UpdateProgressRequest>
    {
        public UpdateProgressRequestValidator()
        {
            RuleFor(x => x.VideoId)
                .GreaterThan(0).WithMessage("VideoId is required.");

            RuleFor(x => x.WatchedPercentage)
                .InclusiveBetween(0, 100).WithMessage("Watched Percentage must be between 0 and 100.");
        }
    }
}
