namespace LMS_SoulCode.Features.Certificates.Models
{
    public class Certificate : BaseTenantEntity
    {
        public string CertificateCode { get; set; } = null!; // e.g. short GUID or custom
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public decimal? Score { get; set; } // optional
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
        public string FilePath { get; set; } = null!; // path or blob url
        public bool IsRevoked { get; set; } = false;
        public int? TemplateId { get; set; }
    }   
}
