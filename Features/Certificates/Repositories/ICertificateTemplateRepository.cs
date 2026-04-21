using LMS_SoulCode.Features.Certificates.Models;
using LMS_SoulCode.Features.Common.Repositories;

namespace LMS_SoulCode.Features.Certificates.Repositories
{
    public interface ICertificateTemplateRepository : IBaseRepository<CertificateTemplate>
    {
        Task<IEnumerable<CertificateTemplate>> GetByTenantIdAsync(int tenantId, CancellationToken cancellationToken = default);
        Task<CertificateTemplate?> GetActiveTemplateAsync(int tenantId, CancellationToken cancellationToken = default);
    }
}
