using LMS_SoulCode.Features.Auth.Models;

namespace LMS_SoulCode.Features.CourseVideos.Models
{
    using LMS_SoulCode.Features.Common.Models;

    public class UserVideoProgress : BaseTenantEntity
    {
        public int UserId { get; set; }
        public int VideoId { get; set; }
        public double WatchedPercentage { get; set; } 
        public bool IsCompleted { get; set; }
        public DateTime LastWatchedAt { get; set; } = DateTime.UtcNow;
        public User User { get; set; } = null!;
        public CourseVideo Video { get; set; } = null!;
    }
}
