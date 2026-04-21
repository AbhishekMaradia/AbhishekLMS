using LMS_SoulCode.Features.Certificates.DTOs;
using LMS_SoulCode.Features.Certificates.Models;
using LMS_SoulCode.Features.Common;

namespace LMS_SoulCode.Features.Certificates.Services
{
    public interface ICertificateTemplateService
    {
        Task<ApiResponse<CertificateTemplateDto>> CreateTemplateAsync(CreateCertificateTemplateRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<CertificateTemplateDto>>> GetTemplatesByTenantIdAsync(int tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<CertificateTemplateDto>> GetActiveTemplateAsync(int tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> DeleteTemplateAsync(int templateId, CancellationToken cancellationToken = default);
    }
}
