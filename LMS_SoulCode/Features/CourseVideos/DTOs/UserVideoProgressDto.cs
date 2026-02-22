namespace LMS_SoulCode.Features.CourseVideos.DTOs
{
    public class UserVideoProgressDto
    {
        public int UserId { get; set; }
        public int VideoId { get; set; }
        public string VideoTitle { get; set; } = string.Empty;
        public double WatchedPercentage { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime LastWatchedAt { get; set; }
    }
}
