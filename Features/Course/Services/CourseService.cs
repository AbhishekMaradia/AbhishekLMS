using AutoMapper;
using CourseEntity = LMS_SoulCode.Features.Course.Models.Course;
using LMS_SoulCode.Features.Course.DTOs;
using LMS_SoulCode.Features.Course.Repositories;
using LMS_SoulCode.Features.CourseVideos.Models;
using LMS_SoulCode.Features.Common;
using Microsoft.AspNetCore.Http;
using LMS_SoulCode.Features.Security.Services;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;
using LMS_SoulCode.Features.Groups.Services;
using LMS_SoulCode.Features.Groups.Repositories;
using LMS_SoulCode.Features.SubscribedCourse.Repositories;

namespace LMS_SoulCode.Features.Course.Services
{
    public interface ICourseService
    {
        Task<PagedApiResponse<CourseResponse>> GetAllCourseAsync(CourseListRequest request, int? tenantId, CancellationToken cancellationToken);
        Task<PagedApiResponse<CourseResponse>> GetCoursesByUserGroupAsync(int? userId, int? groupId, CourseListRequest request, int? tenantId, CancellationToken cancellationToken);
        Task<ApiResponse<List<CourseResponse>>> GetByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<CourseResponse>>> AddAsync(CourseRequest request, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<CourseResponse>>> UpdateAsync(int id, UpdateCourseRequest request, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<string>>> DeleteAsync(int id, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<CourseResponse>>> GetCourseByCategoryIdAsync(int categoryId, int? tenantId, CancellationToken cancellationToken = default);
        Task DecryptFileToStreamAsync(string fullPath, Stream outputStream, CancellationToken cancellationToken = default);
    }

    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IWebHostEnvironment _env;
        private readonly CryptographyService _cryptoService;
        private readonly IMapper _mapper;
        private readonly IGroupService _groupService;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGroupRepository _groupRepo;
        private readonly IUserCourseRepository _userCourseRepo;
        private readonly ILogger<CourseService> _logger;

        public CourseService(
            ICourseRepository courseRepository, 
            ICategoryRepository categoryRepo, 
            IWebHostEnvironment env, 
            CryptographyService cryptographyService, 
            IMapper mapper, 
            IGroupService groupService, 
            IConfiguration config, 
            IHttpContextAccessor httpContextAccessor,
            IGroupRepository groupRepo,
            IUserCourseRepository userCourseRepo,
            ILogger<CourseService> logger)
        {
            _courseRepo = courseRepository;
            _categoryRepo = categoryRepo;
            _env = env;
            _cryptoService = cryptographyService;
            _mapper = mapper;
            _groupService = groupService;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _groupRepo = groupRepo;
            _userCourseRepo = userCourseRepo;
            _logger = logger;
        }

        private string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null) return string.Empty;
            return $"{request.Scheme}://{request.Host}";
        }

        public async Task<PagedApiResponse<CourseResponse>> GetAllCourseAsync(CourseListRequest request, int? tenantId, CancellationToken cancellationToken)
        {
            var targetTenantId = tenantId.HasValue ? tenantId : request.TenantId;
            var isActiveFilter = request.IsActive;

            var (responseList, totalCount) = await _courseRepo.GetCoursesAsync(
                request.SearchTerm,
                request.PageNumber,
                request.PageSize,
                isActiveFilter,
                targetTenantId,
                cancellationToken);

            foreach (var res in responseList)
            {
                FormatCourseUrls(res, null);
            }

            return PagedApiResponse<CourseResponse>.Success(responseList.ToList(), request.PageNumber, request.PageSize, totalCount, Messages.Success);
        }

        public async Task<PagedApiResponse<CourseResponse>> GetCoursesByUserGroupAsync(int? userId, int? groupId, CourseListRequest request, int? tenantId, CancellationToken cancellationToken)
        {
            var (responseList, totalCount) = await _courseRepo.GetCoursesByUserGroupAsync(
                userId,
                groupId,
                request.SearchTerm,
                request.PageNumber,
                request.PageSize,
                tenantId,
                cancellationToken);

            foreach (var res in responseList)
            {
                FormatCourseUrls(res, null);
            }

            return PagedApiResponse<CourseResponse>.Success(responseList.ToList(), request.PageNumber, request.PageSize, totalCount, Messages.Success);
        }

        public async Task<ApiResponse<List<CourseResponse>>> GetByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var c = await _courseRepo.GetByIdAsync(id, tenantId, cancellationToken);
            if (c == null) return ApiResponse<List<CourseResponse>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            var courseResponse = _mapper.Map<CourseResponse>(c);
            FormatCourseUrls(courseResponse, c);

            return ApiResponse<List<CourseResponse>>.Success(new List<CourseResponse> { courseResponse }, Messages.Success);
        }

        private void FormatCourseUrls(CourseResponse res, CourseEntity? originalEntity)
        {
            var baseUrl = GetBaseUrl();
            var gateway = _config["AppSettings:CourseVideoUrl"] ?? "/api/Crypto/get?path=";

            // Sources for URL formatting
            var mainPath = originalEntity?.CourseMainImageUrl ?? res.CourseMainImageUrl;
            var thumbPath = originalEntity?.ThumbnailUrl ?? res.ThumbnailUrl;
            var videoPaths = originalEntity != null 
                ? originalEntity.Videos?.Select(v => v.VideoUrl).ToList() 
                : res.VideoUrls;

            if (!string.IsNullOrEmpty(mainPath))
                res.CourseMainImageUrl = $"{baseUrl}{gateway}{Uri.EscapeDataString(mainPath)}";
            
            if (!string.IsNullOrEmpty(thumbPath))
                res.ThumbnailUrl = $"{baseUrl}{gateway}{Uri.EscapeDataString(thumbPath)}";

            res.VideoUrls = videoPaths?.Select(v => $"{baseUrl}{gateway}{Uri.EscapeDataString(v)}").ToList() ?? new List<string>();
        }

        public async Task<ApiResponse<List<CourseResponse>>> AddAsync(CourseRequest request, int? tenantId, CancellationToken cancellationToken = default)
        {
            var category = await _categoryRepo.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
                return ApiResponse<List<CourseResponse>>.Fail("Invalid Category ID.", StatusCodes.BadRequest);

            var course = _mapper.Map<CourseEntity>(request);
            course.TenantId = tenantId ?? 0;
            
            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                var (mainPath, thumbPath) = await ProcessAndSaveCourseImagesAsync(request.ImageFile, cancellationToken);
                course.CourseMainImageUrl = mainPath;
                course.ThumbnailUrl = thumbPath;
            }

            await _courseRepo.AddAsync(course, cancellationToken);
            
            // --- IDEA 2: AUTO-LINK TO GROUPS (Internal Call) ---
            // Courses will be linked lazily when GetGroupById is called
            
            var courseResponse = _mapper.Map<CourseResponse>(course);
            FormatCourseUrls(courseResponse, course);

            return ApiResponse<List<CourseResponse>>.Success(new List<CourseResponse> { courseResponse }, Messages.Created);
        }

        public async Task<ApiResponse<IEnumerable<CourseResponse>>> GetCourseByCategoryIdAsync(int categoryId, int? tenantId, CancellationToken cancellationToken = default)
        {
            var courses = await _courseRepo.GetByCategoryIdAsync(categoryId, tenantId, cancellationToken);

            if (courses == null || !courses.Any())
                return ApiResponse<IEnumerable<CourseResponse>>.Fail(Messages.NoCoursesInCategory, StatusCodes.NotFound);

            var responseList = _mapper.Map<IEnumerable<CourseResponse>>(courses).ToList();

            foreach (var res in responseList)
            {
                var originalEntity = courses.FirstOrDefault(c => c.Id == res.CourseId);
                FormatCourseUrls(res, originalEntity);
            }

            return ApiResponse<IEnumerable<CourseResponse>>.Success(responseList, Messages.Success);
        }

        public async Task<ApiResponse<List<CourseResponse>>> UpdateAsync(int id, UpdateCourseRequest request, int? tenantId, CancellationToken cancellationToken = default)
        {
            var course = await _courseRepo.GetByIdForAdminAsync(id, tenantId, cancellationToken);
            if (course == null) return ApiResponse<List<CourseResponse>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            // Category Validation & Retention
            if (request.CategoryId <= 0)
            {
                // If CategoryId is not provided (0), retain the current category
                request.CategoryId = course.CategoryId;
            }
            else 
            {
                // Validate that the new CategoryId exists
                var category = await _categoryRepo.GetByIdAsync(request.CategoryId, cancellationToken);
                if (category == null) 
                    return ApiResponse<List<CourseResponse>>.Fail("Invalid Category ID.", StatusCodes.BadRequest);
            }

            // Security Check: Ensure Org Admin doesn't access other tenants' courses
            if (tenantId.HasValue && course.TenantId != tenantId.Value)
            {
                return ApiResponse<List<CourseResponse>>.Fail(Messages.Forbidden, StatusCodes.Forbidden);
            }

            // Organization Lock: We do not allow changing the organization after a course is created.
            // This simplifies the logic and prevents data inconsistency across groups.
            request.TenantId = course.TenantId; 


            _mapper.Map(request, course);

            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                var (mainPath, thumbPath) = await ProcessAndSaveCourseImagesAsync(request.ImageFile, cancellationToken);
                course.CourseMainImageUrl = mainPath;
                course.ThumbnailUrl = thumbPath;
            }

            await _courseRepo.UpdateAsync(course, cancellationToken);

            var courseResponse = _mapper.Map<CourseResponse>(course);
            FormatCourseUrls(courseResponse, course);

            return ApiResponse<List<CourseResponse>>.Success(new List<CourseResponse> { courseResponse }, Messages.Updated);
        }

        private async Task<(string CourseMainImageUrl, string ThumbnailUrl)> ProcessAndSaveCourseImagesAsync(IFormFile file, CancellationToken cancellationToken)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();
            var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            if (!allowedImageExtensions.Contains(ext)) throw new Exception("Invalid image format.");

            var mainFolderPath = _config["AppSettings:CoursesMainPath"] ?? "wwwroot/uploads/courses/main";
            var thumbFolderPath = _config["AppSettings:CoursesThumbPath"] ?? "wwwroot/uploads/courses/thumb";

            if (!Directory.Exists(mainFolderPath)) Directory.CreateDirectory(mainFolderPath);
            if (!Directory.Exists(thumbFolderPath)) Directory.CreateDirectory(thumbFolderPath);

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, cancellationToken);
            
            var thumbSize = int.Parse(_config["AppSettings:CourseThumbSize"] ?? "300");
            var (mainBytes, thumbBytes) = await _cryptoService.ProcessImageWithThumbAsync(ms, thumbSize, cancellationToken);

            string fileName = $"{Guid.NewGuid()}{ext}.enc";

            await _cryptoService.EncryptLargeFileAsync(new MemoryStream(mainBytes), Path.Combine(mainFolderPath, fileName), cancellationToken);
            await _cryptoService.EncryptLargeFileAsync(new MemoryStream(thumbBytes), Path.Combine(thumbFolderPath, fileName), cancellationToken);

            return (Path.Combine(mainFolderPath, fileName).Replace("\\", "/"), Path.Combine(thumbFolderPath, fileName).Replace("\\", "/"));
        }

        public async Task<ApiResponse<List<string>>> DeleteAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var course = await _courseRepo.GetByIdForAdminAsync(id, tenantId, cancellationToken);
            if (course == null) return ApiResponse<List<string>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            // 1. Enrollment Check: Cannot delete course if students are already enrolled
            if (await _courseRepo.AnyEnrolledAsync(id, cancellationToken))
            {
                return ApiResponse<List<string>>.Fail(Messages.DeleteBlockedCourseUsers, StatusCodes.BadRequest);
            }

            // 2. Cascade Unassign: Auto-remove from all groups before deletion
            await _groupService.RemoveCourseFromAllGroupsAsync(id, cancellationToken);

            await _courseRepo.DeleteAsync(id, cancellationToken);
            return ApiResponse<List<string>>.Success(new List<string>(), Messages.Deleted);
        }

        public async Task DecryptFileToStreamAsync(string fullPath, Stream outputStream, CancellationToken cancellationToken = default)
        {
            await _cryptoService.DecryptLargeFileToStreamAsync(fullPath, outputStream, cancellationToken);
        }
    }
}
