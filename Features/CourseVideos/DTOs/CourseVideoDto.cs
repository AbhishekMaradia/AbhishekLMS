namespace LMS_SoulCode.Features.CourseVideos.DTOs
{
    public class CourseVideoDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int? TenantId { get; set; }
    }
}