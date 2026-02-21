using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.SubscribedCourse.Models;
using LMS_SoulCode.Features.SubscribedCourse.DTOs;
using LMS_SoulCode.Features.SubscribedCourse.Repositories;
using AutoMapper;

namespace LMS_SoulCode.Features.SubscribedCourse.Services
{
    public interface IUserCourseService
    {
        Task<ApiResponse<List<string>>> SubscribeAsync(int userId, int courseId, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<string>>> UnsubscribeAsync(int userId, int courseId, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<bool>>> IsSubscribedAsync(int userId, int courseId, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<UserCourseResponse>>> GetUserCoursesAsync(int userId, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<UserCourseResponse>>> GetAllSubscribedAsync(int? tenantId, CancellationToken cancellationToken = default);
        Task<PagedApiResponse<UserCourseListDto>> GetUserCoursesAsync(UserCourseListRequest request, int? tenantId, CancellationToken cancellationToken);
    }

    public class UserCourseService : IUserCourseService
    {
        private readonly IUserCourseRepository _repo;
        private readonly IMapper _mapper;
        
        public UserCourseService(IUserCourseRepository repo, IMapper mapper) 
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<string>>> SubscribeAsync(int userId, int courseId, int? tenantId, CancellationToken cancellationToken = default)
        {
            var uc = new UserCourse
            {
                UserId = userId,
                CourseId = courseId,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _repo.SubscribeAsync(uc, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);

            return ApiResponse<List<string>>.Success(new List<string> { courseId.ToString() }, "Subscribed successfully");
        }

        public async Task<ApiResponse<List<string>>> UnsubscribeAsync(int userId, int courseId, int? tenantId, CancellationToken cancellationToken = default)
        {
            var uc = await _repo.GetAsync(userId, courseId, tenantId, cancellationToken);
            if (uc != null)
            {
                await _repo.UnsubscribeAsync(uc, cancellationToken);
                await _repo.SaveChangesAsync(cancellationToken);
            }

            return ApiResponse<List<string>>.Success(new List<string> { courseId.ToString() }, "Unsubscribed successfully");
        }

        public async Task<ApiResponse<List<bool>>> IsSubscribedAsync(int userId, int courseId, int? tenantId, CancellationToken cancellationToken = default)
        {
            var result = await _repo.IsSubscribedAsync(userId, courseId, tenantId, cancellationToken);
            return ApiResponse<List<bool>>.Success(new List<bool> { result });
        }

        public async Task<ApiResponse<IEnumerable<UserCourseResponse>>> GetUserCoursesAsync(int userId, int? tenantId, CancellationToken cancellationToken = default)
        {
            var courses = await _repo.GetByUserAsync(userId, tenantId, cancellationToken);
            var response = _mapper.Map<IEnumerable<UserCourseResponse>>(courses);
            return ApiResponse<IEnumerable<UserCourseResponse>>.Success(response);
        }

        public async Task<ApiResponse<IEnumerable<UserCourseResponse>>> GetAllSubscribedAsync(int? tenantId, CancellationToken cancellationToken = default)
        {
            var courses = await _repo.GetAllSubscribedAsync(tenantId, cancellationToken);
            var response = _mapper.Map<IEnumerable<UserCourseResponse>>(courses);
            return ApiResponse<IEnumerable<UserCourseResponse>>.Success(response);
        }

        public async Task<PagedApiResponse<UserCourseListDto>> GetUserCoursesAsync(UserCourseListRequest request, int? tenantId, CancellationToken cancellationToken)
        {
            // Security: Org Admin can only see their own tenant, SuperAdmin can specify or see all
            var targetTenantId = tenantId.HasValue 
                ? tenantId                  // Org Admin - force their own tenant
                : request.TenantId;         // SuperAdmin - use request or null
            
            var (userCourses, totalCount) = await _repo.GetUserCoursesAsync(
                request.SearchTerm, 
                request.PageNumber, 
                request.PageSize, 
                request.UserId, 
                request.CourseId, 
                request.SubscribedFrom, 
                request.SubscribedTo, 
                targetTenantId,
                cancellationToken);

            return PagedApiResponse<UserCourseListDto>.Success(userCourses, request.PageNumber, request.PageSize, totalCount, Messages.Success);
        }
    }
}
