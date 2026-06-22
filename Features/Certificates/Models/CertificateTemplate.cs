using System.ComponentModel.DataAnnotations;

namespace LMS_SoulCode.Features.Certificates.Models
{
    public class CertificateTemplate : BaseTenantEntity
    {
        public string Name { get; set; } = null!; // Template Name

        // Path to the Encrypted Razor (.cshtml.enc) file
        public string TemplateFilePath { get; set; } = null!; 

        // Any additional config (reserved for future use)
        public string MetadataConfig { get; set; } = "{}"; 

        public bool IsActive { get; set; } = true;
    }
}
