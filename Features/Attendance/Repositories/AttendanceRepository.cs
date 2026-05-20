using LMS_SoulCode.Data;
using LMS_SoulCode.Features.Attendance.Models;
using LMS_SoulCode.Features.Common.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LMS_SoulCode.Features.Attendance.Repositories
{
    public class AttendanceRepository : BaseRepository<Models.Attendance>, IAttendanceRepository
    {
        public AttendanceRepository(LmsDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Models.Attendance>> GetAttendanceByGroupAndCourseAsync(int groupId, int courseId, DateTime date, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(a => a.GroupId == groupId && a.CourseId == courseId && a.AttendanceDate.Date == date.Date)
                .ToListAsync(cancellationToken);
        }

        public async Task BulkUpsertAttendanceAsync(IEnumerable<Models.Attendance> records, CancellationToken cancellationToken = default)
        {
            foreach (var record in records)
            {
                var existing = await _dbSet
                    .FirstOrDefaultAsync(a => a.GroupId == record.GroupId && 
                                            a.CourseId == record.CourseId && 
                                            a.UserId == record.UserId && 
                                            a.AttendanceDate.Date == record.AttendanceDate.Date, 
                                            cancellationToken);

                if (existing != null)
                {
                    existing.Status = record.Status;
                    existing.Remarks = record.Remarks;
                    existing.SessionStartTime = record.SessionStartTime;
                    existing.SessionEndTime = record.SessionEndTime;
                    if (!string.IsNullOrEmpty(record.DocumentUrl))
                    {
                        existing.DocumentUrl = record.DocumentUrl;
                    }
                    if (!string.IsNullOrEmpty(record.ThumbUrl))
                    {
                        existing.ThumbUrl = record.ThumbUrl;
                    }
                    existing.UpdatedAt = DateTime.UtcNow;
                    _dbSet.Update(existing);
                }
                else
                {
                    await _dbSet.AddAsync(record, cancellationToken);
                }
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<Models.Attendance>> GetAllAttendancesAsync(int? tenantId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            
            if (tenantId.HasValue)
                query = query.Where(a => a.TenantId == tenantId.Value);

            return await query
                .Include(a => a.User)
                    .ThenInclude(u => u!.Organization)
                .Include(a => a.Group)
                .Include(a => a.Course)
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync(cancellationToken);
        }
    }
}
