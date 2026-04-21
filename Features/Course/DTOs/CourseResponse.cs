namespace LMS_SoulCode.Features.Course.DTOs
{
    public class CourseResponse
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string? CourseMainImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public List<string> VideoUrls { get; set; }
        public string? Description { get; set; }
        public string Instructor { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public double DurationHours { get; set; }
        public double Rating { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public string OrgName { get; set; } = string.Empty;
        public int? TenantId { get; set; }
    }
}
