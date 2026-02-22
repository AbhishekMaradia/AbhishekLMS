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
        Task<ApiResponse<List<string>>> UploadVideoAsync(int courseId, IFormFile file, string title, string description, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<string>>> UploadDocumentAsync(int courseId, IFormFile file, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<LMS_SoulCode.Features.CourseVideos.DTOs.CourseVideoDto>>> GetCourseVideosAsync(int courseId, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<LMS_SoulCode.Features.CourseVideos.DTOs.CourseDocumentResponse>>> GetCourseDocumentsAsync(int courseId, int? tenantId, CancellationToken cancellationToken = default);
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

        public CourseService(ICourseRepository courseRepository, ICategoryRepository categoryRepo, IWebHostEnvironment env, CryptographyService cryptographyService, IMapper mapper, IGroupService groupService, IConfiguration config)
        {
            _courseRepo = courseRepository;
            _categoryRepo = categoryRepo;
            _env = env;
            _cryptoService = cryptographyService;
            _mapper = mapper;
            _groupService = groupService;
            _config = config;
        }

        public async Task<PagedApiResponse<CourseResponse>> GetAllCourseAsync(CourseListRequest request, int? tenantId, CancellationToken cancellationToken)
        {
            var targetTenantId = tenantId.HasValue ? tenantId : request.TenantId;
            var isActiveFilter = request.IsActive;

            var (courses, totalCount) = await _courseRepo.GetCoursesAsync(
                request.SearchTerm,
                request.PageNumber,
                request.PageSize,
                isActiveFilter,
                targetTenantId,
                cancellationToken);

            var responseList = _mapper.Map<IEnumerable<CourseResponse>>(courses).ToList();

            foreach (var res in responseList)
            {
                var originalEntity = courses.FirstOrDefault(c => c.Id == res.CourseId);
                FormatCourseUrls(res, originalEntity);
            }

            return PagedApiResponse<CourseResponse>.Success(responseList, request.PageNumber, request.PageSize, totalCount, Messages.Success);
        }

        public async Task<PagedApiResponse<CourseResponse>> GetCoursesByUserGroupAsync(int? userId, int? groupId, CourseListRequest request, int? tenantId, CancellationToken cancellationToken)
        {
            var (items, totalCount) = await _courseRepo.GetCoursesByUserGroupAsync(
                userId,
                groupId,
                request.SearchTerm,
                request.PageNumber,
                request.PageSize,
                tenantId,
                cancellationToken);

            var responseList = _mapper.Map<List<CourseResponse>>(items);

            foreach (var res in responseList)
            {
                var originalEntity = items.FirstOrDefault(c => c.Id == res.CourseId);
                FormatCourseUrls(res, originalEntity);
            }

            return PagedApiResponse<CourseResponse>.Success(responseList, request.PageNumber, request.PageSize, totalCount, Messages.Success);
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
            if (originalEntity == null) return;
            var gateway = _config["AppSettings:CourseVideoUrl"] ?? "/api/Crypto/get?path=";

            if (!string.IsNullOrEmpty(originalEntity.CourseMainImageUrl))
                res.CourseMainImageUrl = $"{gateway}{Uri.EscapeDataString(originalEntity.CourseMainImageUrl)}";
            
            if (!string.IsNullOrEmpty(originalEntity.ThumbnailUrl))
                res.ThumbnailUrl = $"{gateway}{Uri.EscapeDataString(originalEntity.ThumbnailUrl)}";

            res.VideoUrls = originalEntity.Videos?.Select(v => $"{gateway}{Uri.EscapeDataString(v.VideoUrl)}").ToList() ?? new List<string>();
        }

        public async Task<ApiResponse<List<CourseResponse>>> AddAsync(CourseRequest request, int? tenantId, CancellationToken cancellationToken = default)
        {
            var category = await _categoryRepo.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
                return ApiResponse<List<CourseResponse>>.Fail("Invalid Category ID.", StatusCodes.BadRequest);

            var course = _mapper.Map<CourseEntity>(request);
            course.TenantId = tenantId;
            
            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                var (mainPath, thumbPath) = await ProcessAndSaveCourseImagesAsync(request.ImageFile, cancellationToken);
                course.CourseMainImageUrl = mainPath;
                course.ThumbnailUrl = thumbPath;
            }

            await _courseRepo.AddAsync(course, cancellationToken);
            
            // --- IDEA 2: AUTO-LINK TO GROUPS (Internal Call) ---
            await _groupService.LinkCourseToAllGroupsAsync(course.Id, tenantId, cancellationToken);
            
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

        public async Task<ApiResponse<List<string>>> UploadVideoAsync(int courseId, IFormFile file, string title, string description, int? tenantId, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                return ApiResponse<List<string>>.Fail(Messages.NoFileSelected, StatusCodes.BadRequest);

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
                VideoUrl = dbPath
            };

            await _courseRepo.AddVideoAsync(video, cancellationToken);
            var gateway = _config["AppSettings:CourseVideoUrl"] ?? "/api/Crypto/get?path=";

            var publicUrl = $"{gateway}{Uri.EscapeDataString(video.VideoUrl)}";
            return ApiResponse<List<string>>.Success(new List<string> { publicUrl }, Messages.Uploaded);
        }

        public async Task<ApiResponse<List<string>>> UploadDocumentAsync(int courseId, IFormFile file, int? tenantId, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                return ApiResponse<List<string>>.Fail(Messages.NoFileSelected, StatusCodes.BadRequest);

            var ext = Path.GetExtension(file.FileName).ToLower();
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt" };
            if (!allowedExtensions.Contains(ext))
                return ApiResponse<List<string>>.Fail("Invalid document format.", StatusCodes.BadRequest);

            var folderPath = _config["AppSettings:DocsPath"] ?? "wwwroot/uploads/documents";
            
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}{ext}.enc";
            var filePath = Path.Combine(folderPath, fileName);

            using var stream = file.OpenReadStream();
            await _cryptoService.EncryptLargeFileAsync(stream, filePath, cancellationToken);

            var dbPath = Path.Combine(folderPath, fileName).Replace("\\", "/");

            var doc = new CourseDocument
            {
                CourseId = courseId,
                DocName = Path.GetFileNameWithoutExtension(file.FileName),
                DocUrl = dbPath
            };

            await _courseRepo.AddDocsAsync(doc, cancellationToken);
            var gateway = _config["AppSettings:CourseVideoUrl"] ?? "/api/Crypto/get?path=";

            var publicUrl = $"{gateway}{Uri.EscapeDataString(doc.DocUrl)}";
            return ApiResponse<List<string>>.Success(new List<string> { publicUrl }, Messages.Uploaded);
        }

        public async Task<ApiResponse<IEnumerable<LMS_SoulCode.Features.CourseVideos.DTOs.CourseVideoDto>>> GetCourseVideosAsync(int courseId, int? tenantId, CancellationToken cancellationToken = default)
        {
            var course = await _courseRepo.GetByIdForAdminAsync(courseId, tenantId, cancellationToken);
            if (course == null) return ApiResponse<IEnumerable<LMS_SoulCode.Features.CourseVideos.DTOs.CourseVideoDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            var videos = await _courseRepo.GetVideosByCourseIdAsync(courseId, cancellationToken);
            var responseList = _mapper.Map<IEnumerable<LMS_SoulCode.Features.CourseVideos.DTOs.CourseVideoDto>>(videos).ToList();
            
            var gateway = _config["AppSettings:CourseVideoUrl"] ?? "/api/Crypto/get?path=";
            foreach(var v in responseList)
            {
                v.VideoUrl = $"{gateway}{Uri.EscapeDataString(v.VideoUrl)}";
            }

            return ApiResponse<IEnumerable<LMS_SoulCode.Features.CourseVideos.DTOs.CourseVideoDto>>.Success(responseList, Messages.Success);
        }

        public async Task<ApiResponse<IEnumerable<LMS_SoulCode.Features.CourseVideos.DTOs.CourseDocumentResponse>>> GetCourseDocumentsAsync(int courseId, int? tenantId, CancellationToken cancellationToken = default)
        {
            var course = await _courseRepo.GetByIdForAdminAsync(courseId, tenantId, cancellationToken);
            if (course == null) return ApiResponse<IEnumerable<LMS_SoulCode.Features.CourseVideos.DTOs.CourseDocumentResponse>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            var docs = await _courseRepo.GetDocsByCourseIdAsync(courseId, cancellationToken);
            var responseList = _mapper.Map<IEnumerable<LMS_SoulCode.Features.CourseVideos.DTOs.CourseDocumentResponse>>(docs).ToList();
            
            var gateway = _config["AppSettings:CourseVideoUrl"] ?? "/api/Crypto/get?path=";
            foreach(var d in responseList)
            {
                d.DocUrl = $"{gateway}{Uri.EscapeDataString(d.DocUrl)}";
            }

            return ApiResponse<IEnumerable<LMS_SoulCode.Features.CourseVideos.DTOs.CourseDocumentResponse>>.Success(responseList, Messages.Success);
        }

        public async Task<ApiResponse<List<CourseResponse>>> UpdateAsync(int id, UpdateCourseRequest request, int? tenantId, CancellationToken cancellationToken = default)
        {
            var course = await _courseRepo.GetByIdForAdminAsync(id, tenantId, cancellationToken);
            if (course == null) return ApiResponse<List<CourseResponse>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            if (request.CategoryId > 0) 
            {
                 var category = await _categoryRepo.GetByIdAsync(request.CategoryId, cancellationToken);
                 if (category == null) return ApiResponse<List<CourseResponse>>.Fail("Invalid Category ID.", StatusCodes.BadRequest);
            }

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
