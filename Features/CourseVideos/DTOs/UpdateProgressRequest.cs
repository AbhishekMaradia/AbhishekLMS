namespace LMS_SoulCode.Features.CourseVideos.DTOs
{
    public class UpdateProgressRequest
    {
        public int VideoId { get; set; }
        public int? GroupId { get; set; }
        public double WatchedPercentage { get; set; } // 0-100
        public bool IsCompleted { get; set; }
    }
}