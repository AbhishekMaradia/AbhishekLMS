using LMS_SoulCode.Data;
using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.Reports.DTOs;
using Microsoft.EntityFrameworkCore;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;
using Microsoft.AspNetCore.Http;

namespace LMS_SoulCode.Features.Reports.Services
{
    public class CourseReportService
    {
        private readonly LmsDbContext _context;
        private readonly AutoMapper.IMapper _mapper;

        public CourseReportService(LmsDbContext context, AutoMapper.IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<CourseProgressReportDto>>> GetUserCourseReport(int userId, int courseId, int? tenantId, CancellationToken cancellationToken = default)
        {
            // ARCHITECTURAL FIX: 
            // 1. If a student is requesting their own report, the session's tenantId (from JWT) is our primary source of truth.
            // 2. We verify the Course context rather than re-verifying the User-Tenant relationship which is already handled by JWT.
            
            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == courseId, cancellationToken);

            if (course == null)
                return ApiResponse<List<CourseProgressReportDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            // Isolation Check: Course must belong to the same tenant or be a Global course (TenantId 0 or NULL)
            if (tenantId.HasValue && tenantId != 0 && course.TenantId != tenantId && course.TenantId != null && course.TenantId != 0)
            {
                return ApiResponse<List<CourseProgressReportDto>>.Fail(Messages.Forbidden, StatusCodes.Forbidden);
            }

            var videos = await _context.CourseVideos
                .AsNoTracking()
                .Where(v => v.CourseId == courseId)
                .ToListAsync(cancellationToken);

            var videoIds = videos.Select(v => v.Id).ToList();
            
            var progress = await _context.UserVideoProgresses
                .AsNoTracking()
                .Where(p => p.UserId == userId && videoIds.Contains(p.VideoId))
                .ToListAsync(cancellationToken);

            var totalVideos = videos.Count;
            var completedVideos = progress.Count(p => p.IsCompleted);

            var videoDetails = videos.Select(v => {
                var vp = progress.FirstOrDefault(p => p.VideoId == v.Id);
                return new VideoProgressDto(
                    v.Id,
                    v.Title,
                    vp?.WatchedPercentage ?? 0,
                    vp?.IsCompleted ?? false
                );
            }).ToList();

            var report = new CourseProgressReportDto
            (
                courseId,
                totalVideos,
                completedVideos,
                totalVideos == 0 ? 0 : Math.Round((completedVideos * 100.0) / totalVideos, 2),
                videoDetails
            );

            return ApiResponse<List<CourseProgressReportDto>>.Success(new List<CourseProgressReportDto> { report }, Messages.Success);
        }

        public async Task<PagedApiResponse<ReportListDto>> GetReportsAsync(ReportListRequest request, int? tenantId, CancellationToken cancellationToken)
        {
            var targetTenantId = tenantId.HasValue ? tenantId : request.TenantId;
            
            var query = _context.UserCourses
                .Include(uc => uc.User).ThenInclude(u => u.Organization)
                .Include(uc => uc.User).ThenInclude(u => u.Group)
                .Include(uc => uc.Course)
                .Where(uc => uc.IsActive && uc.Course.IsActive && !uc.User.IsDeleted && !uc.Course.IsDeleted)
                .AsNoTracking()
                .AsQueryable();

            if (targetTenantId.HasValue && targetTenantId != 0)
            {
                query = query.Where(x => x.User.TenantId == targetTenantId.Value || x.Course.TenantId == targetTenantId.Value);
            }

            if (request.UserId.HasValue) query = query.Where(x => x.UserId == request.UserId.Value);
            if (request.GroupId.HasValue) query = query.Where(x => x.User.GroupId == request.GroupId.Value);
            if (request.CourseId.HasValue) query = query.Where(x => x.CourseId == request.CourseId.Value);

            var totalCount = await query.CountAsync(cancellationToken);
            var userCourses = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var courseIds = userCourses.Select(x => x.CourseId).Distinct().ToList();
            var userIds = userCourses.Select(x => x.UserId).Distinct().ToList();

            var videoCounts = await _context.CourseVideos
                .Where(v => courseIds.Contains(v.CourseId))
                .GroupBy(v => v.CourseId)
                .Select(g => new { CourseId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CourseId, x => x.Count, cancellationToken);

            var progressStats = await _context.UserVideoProgresses
                .Where(p => userIds.Contains(p.UserId))
                .GroupBy(p => new { p.UserId, p.VideoId }) // Simplified for better grouping
                .Select(g => new { g.Key.UserId, g.Key.VideoId, IsCompleted = g.Any(x => x.IsCompleted) })
                .ToListAsync(cancellationToken);

            var reports = new List<ReportListDto>();
            foreach (var item in userCourses)
            {
                var report = _mapper.Map<ReportListDto>(item);
                int totalVideos = videoCounts.ContainsKey(item.CourseId) ? videoCounts[item.CourseId] : 0;
                
                // Calculate progress for this user-course specifically
                var courseVideoIds = await _context.CourseVideos.Where(v => v.CourseId == item.CourseId).Select(v => v.Id).ToListAsync();
                int completedVideos = progressStats.Count(p => p.UserId == item.UserId && courseVideoIds.Contains(p.VideoId) && p.IsCompleted);

                double completionPercentage = totalVideos == 0 ? 0 : (completedVideos * 100.0) / totalVideos;
                report.TotalVideos = totalVideos;
                report.CompletedVideos = completedVideos;
                report.CompletionPercentage = Math.Round(completionPercentage, 2);
                reports.Add(report);
            }

            return PagedApiResponse<ReportListDto>.Success(reports, request.PageNumber, request.PageSize, totalCount, Messages.Success);
        }
    }
}
