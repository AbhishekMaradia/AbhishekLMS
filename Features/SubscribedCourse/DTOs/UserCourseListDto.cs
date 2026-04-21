namespace LMS_SoulCode.Features.SubscribedCourse.DTOs
{
    public class UserCourseListDto
    {
        // Removed Id since database table doesn't have it
        public int UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string CourseInstructor { get; set; } = string.Empty;
        public decimal CoursePrice { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;

        public DateTime SubscribedAt { get; set; }
        public int? TenantId { get; set; }
    }
}