using System;

namespace LMS_SoulCode.Features.Common.Models
{
    public abstract class BaseEntity : IBaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }

    public abstract class BaseTenantEntity : BaseEntity, ITenantEntity
    {
        public int? TenantId { get; set; }
    }
}
