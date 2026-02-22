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
            // Validate User Tenant if tenantId provided
            if (tenantId.HasValue)
            {
                 var userTenant = await _context.Users
                     .Where(u => u.Id == userId)
                     .Select(u => u.TenantId)
                     .FirstOrDefaultAsync(cancellationToken);
                 
                 if (userTenant != tenantId.Value)
                     return ApiResponse<List<CourseProgressReportDto>>.Fail(Messages.Forbidden, StatusCodes.Forbidden);
            }

            var videos = await _context.CourseVideos
                .Include(v => v.Course)
                .Where(v => v.CourseId == courseId && v.Course.IsActive) // Only videos of active courses
                .ToListAsync(cancellationToken);

            var progress = await _context.UserVideoProgresses
                .Where(p => p.UserId == userId)
                .ToListAsync(cancellationToken);

            var totalVideos = videos.Count;
            var completedVideos = progress.Count(p => p.IsCompleted);

            var report = new CourseProgressReportDto
            (
                courseId,
                totalVideos,
                completedVideos,
                totalVideos == 0 ? 0 : (completedVideos * 100.0) / totalVideos
            );

            return ApiResponse<List<CourseProgressReportDto>>.Success(new List<CourseProgressReportDto> { report }, Messages.Success);
        }

        // Optimized paginated method for listing all reports
        public async Task<PagedApiResponse<ReportListDto>> GetReportsAsync(ReportListRequest request, int? tenantId, CancellationToken cancellationToken)
        {
            // Security: Org Admin can only see their own tenant, SuperAdmin can specify or see all
            var targetTenantId = tenantId.HasValue 
                ? tenantId                  // Org Admin - force their own tenant
                : request.TenantId;         // SuperAdmin - use request or null
            
            // 1. Base Query with Filtering
            var query = _context.UserCourses
                .Include(uc => uc.User)
                .Include(uc => uc.Course)
                .Where(uc => uc.IsActive && uc.Course.IsActive && !uc.User.IsDeleted && !uc.Course.IsDeleted)
                .AsQueryable();

            if (targetTenantId.HasValue)
            {
                query = query.Where(x => x.User.TenantId == targetTenantId.Value);
            }

            // Apply filters
            if (request.UserId.HasValue)
                query = query.Where(x => x.UserId == request.UserId.Value);

            if (request.GroupId.HasValue)
                query = query.Where(x => x.User.GroupId == request.GroupId.Value);

            if (request.CourseId.HasValue)
                query = query.Where(x => x.CourseId == request.CourseId.Value);

            if (request.GeneratedFrom.HasValue)
                query = query.Where(x => x.CreatedAt >= request.GeneratedFrom.Value);

            if (request.GeneratedTo.HasValue)
                query = query.Where(x => x.CreatedAt <= request.GeneratedTo.Value);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(x => 
                    x.User.Email.ToLower().Contains(searchTerm) ||
                    x.Course.Title.ToLower().Contains(searchTerm) ||
                    x.Course.Instructor.ToLower().Contains(searchTerm)
                );
            }

            // 2. Fetch Page Data (UserCourses)
            // Note: We cannot filter by CompletionPercentage in the database easily as it requires aggregation.
            // We will fetch the page, calculate, and then filtering might be inaccurate for pagination if heavily filtered.
            // For true pagination with calculated fields, we would need a stored procedure or complex view.
            // Current approach: Fetch page -> Calculate -> Filter (Logic maintained from original but optimized execution)
            
            var totalCount = await query.CountAsync(cancellationToken);

            var userCourses = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            if (!userCourses.Any())
            {
                return PagedApiResponse<ReportListDto>.Success(new List<ReportListDto>(), request.PageNumber, request.PageSize, totalCount, Messages.Success);
            }

            // 3. Batch Fetch Stats
            var courseIds = userCourses.Select(x => x.CourseId).Distinct().ToList();
            var userIds = userCourses.Select(x => x.UserId).Distinct().ToList();

            // A. Video Counts per Course
            var videoCounts = await _context.CourseVideos
                .Where(v => courseIds.Contains(v.CourseId) && v.Course.IsActive)
                .GroupBy(v => v.CourseId)
                .Select(g => new { CourseId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CourseId, x => x.Count, cancellationToken);

            // B. User Progress per Course (Joined via Video)
            // EF Core GroupBy translation for complex joins can be tricky, so we join explicitly
            var progressStats = await _context.UserVideoProgresses
                .Include(p => p.Video)
                .Where(p => userIds.Contains(p.UserId) && courseIds.Contains(p.Video.CourseId))
                .GroupBy(p => new { p.UserId, CourseId = p.Video.CourseId })
                .Select(g => new 
                { 
                    UserId = g.Key.UserId, 
                    CourseId = g.Key.CourseId, 
                    CompletedCount = g.Count(x => x.IsCompleted),
                    LastUpdated = g.Max(x => x.LastWatchedAt)
                })
                .ToListAsync(cancellationToken);

            // 4. Assemble Results using Mapper
            var reports = new List<ReportListDto>();

            foreach (var item in userCourses)
            {
                var report = _mapper.Map<ReportListDto>(item);

                // Fill calculated stats
                int totalVideos = videoCounts.ContainsKey(item.CourseId) ? videoCounts[item.CourseId] : 0;
                
                var userStat = progressStats.FirstOrDefault(p => p.UserId == item.UserId && p.CourseId == item.CourseId);
                int completedVideos = userStat?.CompletedCount ?? 0;
                DateTime lastUpdated = userStat?.LastUpdated ?? item.CreatedAt;

                double completionPercentage = totalVideos == 0 ? 0 : (completedVideos * 100.0) / totalVideos;

                // Apply completion percentage filter (In-memory)
                if (request.MinCompletionPercentage.HasValue && completionPercentage < request.MinCompletionPercentage.Value)
                    continue;

                if (request.MaxCompletionPercentage.HasValue && completionPercentage > request.MaxCompletionPercentage.Value)
                    continue;

                report.TotalVideos = totalVideos;
                report.CompletedVideos = completedVideos;
                report.CompletionPercentage = Math.Round(completionPercentage, 2);
                report.LastUpdated = lastUpdated;

                reports.Add(report);
            }

            return PagedApiResponse<ReportListDto>.Success(reports, request.PageNumber, request.PageSize, totalCount, Messages.Success);
        }
    }
}
