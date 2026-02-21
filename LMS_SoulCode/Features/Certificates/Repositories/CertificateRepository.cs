using LMS_SoulCode.Data;
using LMS_SoulCode.Features.Certificates.Models;
using LMS_SoulCode.Features.Certificates.DTOs;
using LMS_SoulCode.Features.Common;
using Microsoft.EntityFrameworkCore;

namespace LMS_SoulCode.Features.Certificates.Repositories
{
    public class CertificateRepository : BaseRepository<Certificate>, ICertificateRepository
    {
        public CertificateRepository(LmsDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Certificate>> GetAllAsync(int? tenantId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(x => !tenantId.HasValue || x.TenantId == tenantId.Value)
                .OrderByDescending(x => x.IssuedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Certificate?> GetByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(x => (!tenantId.HasValue || x.TenantId == tenantId.Value) && x.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Certificate?> GetByCodeAsync(string code, int? tenantId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(x => (!tenantId.HasValue || x.TenantId == tenantId.Value) && x.CertificateCode == code)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<Certificate>> GetByUserIdAsync(int userId, int? tenantId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(x => x.UserId == userId && (!tenantId.HasValue || x.TenantId == tenantId.Value))
                .OrderByDescending(x => x.IssuedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<CertificateListDto> Items, int TotalCount)> GetCertificatesAsync(
            string? searchTerm, 
            int pageNumber, 
            int pageSize, 
            int? userId, 
            int? courseId, 
            DateTime? issuedFrom, 
            DateTime? issuedTo, 
            int? tenantId,
            CancellationToken cancellationToken)
        {
            // Since this involves complex joins and isn't a direct projection from Certificate entity
            // we'll still use the query builder pattern but return the PagedList
            
            var query = from c in _context.Certificates
                        join u in _context.Users on c.UserId equals u.Id
                        join course in _context.Courses on c.CourseId equals course.Id
                        where course.IsActive // Only certificates for active courses
                        select new { c, u, course };

            if (tenantId.HasValue)
            {
                query = query.Where(x => x.c.TenantId == tenantId.Value);
            }

            // Apply filters
            if (userId.HasValue)
            {
                query = query.Where(x => x.c.UserId == userId.Value);
            }

            if (courseId.HasValue)
            {
                query = query.Where(x => x.c.CourseId == courseId.Value);
            }

            if (issuedFrom.HasValue)
            {
                query = query.Where(x => x.c.IssuedAt >= issuedFrom.Value);
            }

            if (issuedTo.HasValue)
            {
                query = query.Where(x => x.c.IssuedAt <= issuedTo.Value);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(x => 
                    x.c.CertificateCode.ToLower().Contains(searchTerm) ||
                    x.u.UserName.ToLower().Contains(searchTerm) ||
                    x.u.Email.ToLower().Contains(searchTerm) ||
                    x.course.Title.ToLower().Contains(searchTerm)
                );
            }

            return await query
                .OrderByDescending(x => x.c.IssuedAt)
                .Select(x => new CertificateListDto
                {
                    Id = x.c.Id,
                    CertificateCode = x.c.CertificateCode,
                    UserId = x.c.UserId,
                    UserName = x.u.UserName,
                    UserEmail = x.u.Email,
                    CourseId = x.c.CourseId,
                    CourseTitle = x.course.Title,
                    Score = x.c.Score,
                    IssuedAt = x.c.IssuedAt,
                    FileUrl = x.c.FilePath
                })
                .ToPagedListAsync(pageNumber, pageSize, cancellationToken);
        }
    }
}
