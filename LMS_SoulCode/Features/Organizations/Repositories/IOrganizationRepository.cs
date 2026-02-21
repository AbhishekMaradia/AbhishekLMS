using LMS_SoulCode.Features.Common.Repositories;
using LMS_SoulCode.Features.Organizations.Models;

namespace LMS_SoulCode.Features.Organizations.Repositories
{
    public interface IOrganizationRepository : IBaseRepository<Organization>
    {
        Task<(IEnumerable<Organization> Items, int TotalCount)> GetOrganizationsAsync(string? searchTerm, int pageNumber, int pageSize, bool? isActive, CancellationToken cancellationToken);
        Task<Organization?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default);
        Task SaveAsync(CancellationToken cancellationToken = default);
    }
}
