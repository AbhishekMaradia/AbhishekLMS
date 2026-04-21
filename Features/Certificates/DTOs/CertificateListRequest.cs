using LMS_SoulCode.Features.Common;

namespace LMS_SoulCode.Features.Certificates.DTOs
{
    public class CertificateListRequest : BasePagedRequest
    {
        public string? SearchTerm { get; set; }
        public int? UserId { get; set; }
        public int? CourseId { get; set; }
        public DateTime? IssuedFrom { get; set; }
        public DateTime? IssuedTo { get; set; }
        public int? TenantId { get; set; }  // Optional - for SuperAdmin to specify org
    }
}