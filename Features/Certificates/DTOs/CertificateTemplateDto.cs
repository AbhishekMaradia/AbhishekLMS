namespace LMS_SoulCode.Features.Certificates.DTOs
{
    public record CertificateTemplateDto(int Id, int TenantId, string Name, string TemplateFilePath, string? MetadataConfig, bool IsActive, DateTime CreatedAt);
}
