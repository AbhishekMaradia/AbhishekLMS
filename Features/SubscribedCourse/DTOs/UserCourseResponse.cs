using LMS_SoulCode.Features.Course.DTOs;

namespace LMS_SoulCode.Features.SubscribedCourse.DTOs
{
    public record UserCourseResponse(int UserId, int CourseId, DateTime SubscribedAt, bool IsActive, int? TenantId, CourseResponse? Course = null);
}
