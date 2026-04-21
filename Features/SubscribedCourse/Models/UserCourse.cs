using CourseEntity = LMS_SoulCode.Features.Course.Models.Course;
using LMS_SoulCode.Features.Auth.Models;
using System.ComponentModel.DataAnnotations;

namespace LMS_SoulCode.Features.SubscribedCourse.Models
{
    using LMS_SoulCode.Features.Common.Models;

    public class UserCourse : ITenantEntity, ISoftDelete, IAuditEntity
    {
        // Remove Id property since database doesn't have it
        [Key]
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public int? TenantId { get; set; }

        // navigation props
        public User User { get; set; } = null!;
        public CourseEntity Course { get; set; } = null!;

        // metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
