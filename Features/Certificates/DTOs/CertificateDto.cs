namespace LMS_SoulCode.Features.Certificates.DTOs
{
    public record CreateCertificateRequest(int UserId, int CourseId, decimal? Score, int? TenantId);

    public class CertificateDto
    {
        public int Id { get; set; }
        public string CertificateCode { get; set; } = null!;
        public int UserId { get; set; }
        public int TenantId { get; set; }
        public int CourseId { get; set; }
        public decimal? Score { get; set; }
        public DateTime IssuedAt { get; set; }
        public string FileUrl { get; set; } = null!;
    }

}
