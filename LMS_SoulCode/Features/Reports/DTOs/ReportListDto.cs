namespace LMS_SoulCode.Features.Reports.DTOs
{
    public class ReportListDto
    {
        public int UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string CourseInstructor { get; set; } = string.Empty;
        public int TotalVideos { get; set; }
        public int CompletedVideos { get; set; }
        public double CompletionPercentage { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime EnrolledAt { get; set; }
        public int? TenantId { get; set; }
    }
}