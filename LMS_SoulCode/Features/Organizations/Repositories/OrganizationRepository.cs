using LMS_SoulCode.Features.Common.Repositories;
using LMS_SoulCode.Features.Organizations.Models;
using LMS_SoulCode.Features.Common;
using Microsoft.EntityFrameworkCore;
using LMS_SoulCode.Data;

namespace LMS_SoulCode.Features.Organizations.Repositories
{
    public class OrganizationRepository : BaseRepository<Organization>, IOrganizationRepository
    {
        public OrganizationRepository(LmsDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<Organization> Items, int TotalCount)> GetOrganizationsAsync(string? searchTerm, int pageNumber, int pageSize, bool? isActive, CancellationToken cancellationToken)
        => await GetPagedAsync(
                pageNumber,
                pageSize,
                filter: o => (!isActive.HasValue || o.IsActive == isActive.Value) &&
                            (string.IsNullOrWhiteSpace(searchTerm) || 
                             o.Name.ToLower().Contains(searchTerm.ToLower()) || 
                             o.Code.ToLower().Contains(searchTerm.ToLower()) ||
                             (o.Website != null && o.Website.ToLower().Contains(searchTerm.ToLower()))),
                queryModifier: q => q.AsNoTracking().OrderByDescending(o => o.Id),
                cancellationToken: cancellationToken
            );        

        public async Task<Organization?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await _context.Organizations
                .FirstOrDefaultAsync(o => o.Code == code && !o.IsDeleted, cancellationToken);
        

        public async Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default)
        => await _context.Organizations.AnyAsync(o => o.Code == code && !o.IsDeleted, cancellationToken);
        

        public async Task SaveAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
        
    }
}
