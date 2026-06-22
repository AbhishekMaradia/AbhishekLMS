using LMS_SoulCode.Data;
using LMS_SoulCode.Features.Certificates.Models;
using LMS_SoulCode.Features.Common.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LMS_SoulCode.Features.Certificates.Repositories
{
    public class CertificateTemplateRepository : BaseRepository<CertificateTemplate>, ICertificateTemplateRepository
    {
        public CertificateTemplateRepository(LmsDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CertificateTemplate>> GetByTenantIdAsync(int tenantId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.TenantId == tenantId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<CertificateTemplate?> GetActiveTemplateAsync(int tenantId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.TenantId == tenantId && t.IsActive)
                .OrderByDescending(t => t.UpdatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
