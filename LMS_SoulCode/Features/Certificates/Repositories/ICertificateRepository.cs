using LMS_SoulCode.Features.Certificates.Models;
using LMS_SoulCode.Features.Certificates.DTOs;
using LMS_SoulCode.Features.Common.Repositories;

namespace LMS_SoulCode.Features.Certificates.Repositories
{
    public interface ICertificateRepository : IBaseRepository<Certificate>
    {
        Task<Certificate?> GetByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default);
        Task<Certificate?> GetByCodeAsync(string code, int? tenantId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Certificate>> GetByUserIdAsync(int userId, int? tenantId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Certificate>> GetAllAsync(int? tenantId, CancellationToken cancellationToken = default);
        Task<(IEnumerable<CertificateListDto> Items, int TotalCount)> GetCertificatesAsync(string? searchTerm, int pageNumber, int pageSize, int? userId, int? courseId, DateTime? issuedFrom, DateTime? issuedTo, int? tenantId, CancellationToken cancellationToken);
    }
}
