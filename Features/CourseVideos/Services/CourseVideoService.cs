using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.Course.Models;
using LMS_SoulCode.Features.Course.Repositories;
using LMS_SoulCode.Features.CourseVideos.Models;
using LMS_SoulCode.Features.CourseVideos.DTOs;
using LMS_SoulCode.Features.CourseVideos.Repositories;
using LMS_SoulCode.Features.Security.Services;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;
using Microsoft.AspNetCore.Http;
using AutoMapper;

namespace LMS_SoulCode.Features.CourseVideos.Services
{
    public class CourseVideoService
    {
        private readonly ICourseVideoRepository _videoRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly IWebHostEnvironment _env;
        private readonly IMapper _mapper;
        private readonly CryptographyService _cryptoService;
        private readonly IConfiguration _config;

        public CourseVideoService(
            ICourseVideoRepository courseVideoRepository, 
            ICourseRepository courseRepository,
            IWebHostEnvironment env, 
            IMapper mapper,
            CryptographyService cryptographyService,
            IConfiguration config)
        {
            _videoRepo = courseVideoRepository;
            _courseRepo = courseRepository;
            _env = env;
            _mapper = mapper;
            _cryptoService = cryptographyService;
            _config = config;
        }

        public async Task<ApiResponse<IEnumerable<CourseVideoDto>>> GetByCourseAsync(int courseId, int? tenantId, CancellationToken cancellationToken = default)
        {
            var result = await _videoRepo.GetByCourseIdAsync(courseId, tenantId, cancellationToken);
            var courseVideos = result.Where(v => v != null).OfType<CourseVideo>().ToList();
            var dtos = _mapper.Map<IEnumerable<CourseVideoDto>>(courseVideos).ToList();

            foreach (var video in dtos)
            {
                video.VideoUrl = $"/api/CourseVideo/stream/{video.Id}";
            }

            return ApiResponse<IEnumerable<CourseVideoDto>>.Success(dtos, Messages.Success);
        }

        // New paginated version - same method name with overload
        public async Task<PagedApiResponse<CourseVideo>> GetByCourseAsync(CourseVideosByCourseRequest request, int? tenantId, CancellationToken cancellationToken)
        {
            var (videos, totalCount) = await _videoRepo.GetByCourseIdAsync(
                request.CourseId, 
                request.SearchTerm, 
                request.PageNumber, 
                request.PageSize, 
                tenantId,
                cancellationToken);

            var courseVideos = videos.ToList();

            foreach (var video in courseVideos)
            {
                video.VideoUrl = $"/api/CourseVideo/stream/{video.Id}";
            }

            return PagedApiResponse<CourseVideo>.Success(courseVideos, request.PageNumber, request.PageSize, totalCount, Messages.Success);
        }

        public async Task<CourseVideo?> GetByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var video = await _videoRepo.GetByIdAsync(id, tenantId, cancellationToken);
            if (video != null)
            {
                video.VideoUrl = $"/api/CourseVideo/stream/{video.Id}";
            }
            return video;
        }

        public async Task<CourseVideo?> GetRawByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            return await _videoRepo.GetByIdAsync(id, tenantId, cancellationToken);
        }

        public async Task<ApiResponse<List<string>>> UploadAsync(int courseId, IFormFile file, string title, string description, int? tenantId, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                return ApiResponse<List<string>>.Fail(Messages.NoFileSelected, StatusCodes.BadRequest);

            var course = await _courseRepo.GetByIdForAdminAsync(courseId, tenantId, cancellationToken);
            if (course == null) return ApiResponse<List<string>>.Fail("Course not found", StatusCodes.NotFound);

            var ext = Path.GetExtension(file.FileName).ToLower();
            var allowedExtensions = new[] { ".mp4", ".mov", ".avi", ".webm", ".mkv" };
            if (!allowedExtensions.Contains(ext))
                return ApiResponse<List<string>>.Fail("Invalid video format.", StatusCodes.BadRequest);

            var folderPath = _config["AppSettings:VideosPath"] ?? "wwwroot/uploads/videos";
            
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}{ext}.enc";
            var filePath = Path.Combine(folderPath, fileName);

            using var stream = file.OpenReadStream();
            await _cryptoService.EncryptLargeFileAsync(stream, filePath, cancellationToken);

            var dbPath = Path.Combine(folderPath, fileName).Replace("\\", "/");

            var video = new CourseVideo
            {
                CourseId = courseId,
                Title = title,
                Description = description,
                VideoUrl = dbPath,
                TenantId = course.TenantId // Inherit from course
            };

            await _videoRepo.AddAsync(video, cancellationToken);
            return ApiResponse<List<string>>.Success(new List<string> { video.VideoUrl }, Messages.Uploaded);
        }

        public async Task<ApiResponse<string>> DeleteAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var video = await _videoRepo.GetByIdAsync(id, tenantId, cancellationToken);
            if (video == null) return ApiResponse<string>.Fail(Messages.NotFound, StatusCodes.NotFound);

            var fullPath = Path.Combine(_env.ContentRootPath, video.VideoUrl.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            await _videoRepo.DeleteAsync(video.Id, cancellationToken);
            return ApiResponse<string>.Success(Messages.Success);   
        }

        public async Task <ApiResponse<IEnumerable<CourseVideo>>> GetAllCourseVideoAsync(int? tenantId, CancellationToken cancellationToken = default)
        {
            var result = await _videoRepo.GetAllCourseVideoAsync(tenantId, cancellationToken);
            var courseVideos = result.Where(v => v != null).ToList();
            if (courseVideos == null || !courseVideos.Any())
                return ApiResponse<IEnumerable<CourseVideo>>.Fail(
                    "No videos found for any course",
                    StatusCodes.NotFound
                );
            
            foreach (var video in courseVideos)
            {
                video.VideoUrl = $"/api/CourseVideo/stream/{video.Id}";
            }

            return ApiResponse<IEnumerable<CourseVideo>>.Success(courseVideos, Messages.Success);
        }

        // New paginated method
        public async Task<PagedApiResponse<CourseVideoDto>> GetCourseVideosAsync(CourseVideoListRequest request, int? tenantId, CancellationToken cancellationToken)
        {
            // Security: Org Admin can only see their own tenant, SuperAdmin can specify or see all
            var targetTenantId = tenantId.HasValue 
                ? tenantId                  // Org Admin - force their own tenant
                : request.TenantId;         // SuperAdmin - use request or null
            
            var (videos, totalCount) = await _videoRepo.GetCourseVideosAsync(
                request.SearchTerm, 
                request.PageNumber, 
                request.PageSize, 
                request.CourseId, 
                targetTenantId,
                cancellationToken);

            var videoDtos = _mapper.Map<IEnumerable<CourseVideoDto>>(videos);
            
            // Update URLs after mapping
            foreach (var video in videoDtos)
            {
                video.VideoUrl = $"/api/CourseVideo/stream/{video.Id}";
            }

            return PagedApiResponse<CourseVideoDto>.Success(videoDtos, request.PageNumber, request.PageSize, totalCount, Messages.Success);
        }

        // Progress tracking methods
        public async Task<ApiResponse<List<string>>> UpdateProgressAsync(int userId, int videoId, double watchedPercentage, bool isCompleted, int? groupId, int? tenantId, CancellationToken cancellationToken = default)
        {
            // Validate video exists and belongs to tenant
            var video = await _videoRepo.GetByIdAsync(videoId, tenantId, cancellationToken);
            if (video == null)
                return ApiResponse<List<string>>.Fail(Messages.NotFound, StatusCodes.BadRequest);

            var progress = new UserVideoProgress
            {
                UserId = userId,
                VideoId = videoId,
                GroupId = groupId,
                WatchedPercentage = Math.Max(0, Math.Min(100, watchedPercentage)), // Ensure 0-100 range
                IsCompleted = isCompleted || watchedPercentage >= 95, // Auto-complete at 95%
                LastWatchedAt = DateTime.UtcNow,
                TenantId = tenantId
            };

            await _videoRepo.UpdateProgressAsync(progress, cancellationToken);
            return ApiResponse<List<string>>.Success(new List<string>(), "Progress updated successfully");
        }

        public async Task<ApiResponse<List<UserVideoProgress>>> GetProgressAsync(int userId, int videoId, int? tenantId, CancellationToken cancellationToken = default)
        {
            var progress = await _videoRepo.GetProgressAsync(userId, videoId, tenantId, cancellationToken);
            if (progress == null)
            {
                // Return default progress if not found
                progress = new UserVideoProgress
                {
                    UserId = userId,
                    VideoId = videoId,
                    WatchedPercentage = 0,
                    IsCompleted = false,
                    LastWatchedAt = DateTime.UtcNow,
                    TenantId = tenantId
                };
            }
            return ApiResponse<List<UserVideoProgress>>.Success(new List<UserVideoProgress> { progress }, Messages.Success);
        }

        public async Task<ApiResponse<string>> UpdateAsync(int videoId, string title, string description, int? tenantId, CancellationToken cancellationToken = default)
        {
            var video = await _videoRepo.GetByIdAsync(videoId, tenantId, cancellationToken);
            if (video == null)
                return ApiResponse<string>.Fail(Messages.NotFound, StatusCodes.NotFound);

            if (tenantId.HasValue && tenantId != 0 && video.Course?.TenantId != tenantId && video.Course?.TenantId != null)
                return ApiResponse<string>.Fail("Unauthorized", StatusCodes.Unauthorized);

            video.Title = title;
            video.Description = description;
            await _videoRepo.UpdateAsync(video, cancellationToken);
            return ApiResponse<string>.Success("Updated", Messages.Success);
        }
    }
}