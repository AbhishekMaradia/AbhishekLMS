using System;

namespace LMS_SoulCode.Features.Common.Models
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
    }

    public interface IAuditEntity
    {
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
    }

    public interface IBaseEntity : IAuditEntity, ISoftDelete
    {
        int Id { get; set; }
    }

    public interface ITenantEntity
    {
        int? TenantId { get; set; }
    }
}
