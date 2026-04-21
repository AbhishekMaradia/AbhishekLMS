using Microsoft.AspNetCore.Http;

namespace LMS_SoulCode.Features.Certificates.DTOs
{
    public class CreateCertificateTemplateRequest
    {
        public int TenantId { get; set; }
        public string Name { get; set; } = null!;
        public IFormFile TemplateFile { get; set; } = null!; // The image file
        public string MetadataConfig { get; set; } = "{}"; // JSON config
    }
}
