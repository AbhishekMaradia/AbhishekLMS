using LMS_SoulCode.Features.Common.Models;
using LMS_SoulCode.Features.CourseVideos.Models;

namespace LMS_SoulCode.Features.Course.Models
{
    public class Course : BaseTenantEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Navigation Properties
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        public string Instructor { get; set; } = string.Empty;
        public string? CourseMainImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string Difficulty { get; set; } = string.Empty;
        public double DurationHours { get; set; }
        public int EnrolledCount { get; set; }
        public double Rating { get; set; }
        public decimal Price { get; set; }
        public int Lectures { get; set; }
        public string? Materials { get; set; }
        public string? Tags { get; set; }

        public ICollection<CourseVideo> Videos { get; set; } = new List<CourseVideo>();
    }
}
