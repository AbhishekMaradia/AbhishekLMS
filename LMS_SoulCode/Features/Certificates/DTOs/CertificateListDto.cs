namespace LMS_SoulCode.Features.Certificates.DTOs
{
    public class CertificateListDto
    {
        public int Id { get; set; }
        public string CertificateCode { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int? TenantId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public decimal? Score { get; set; }
        public DateTime IssuedAt { get; set; }
        public string FileUrl { get; set; } = string.Empty;
    }
}