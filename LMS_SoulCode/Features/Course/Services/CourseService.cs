using AutoMapper;
using CourseEntity = LMS_SoulCode.Features.Course.Models.Course;
using LMS_SoulCode.Features.Course.DTOs;
using LMS_SoulCode.Features.Course.Repositories;
using LMS_SoulCode.Features.CourseVideos.Models;
using LMS_SoulCode.Features.Common;
using Microsoft.AspNetCore.Http;
using LMS_SoulCode.Features.Security.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;
using LMS_SoulCode.Features.Groups.Services;

namespace LMS_SoulCode.Features.Course.Services
{
    public interface ICourseService
    {
        Task<PagedApiResponse<CourseResponse>> GetAllCourseAsync(CourseListRequest request, int? tenantId, CancellationToken cancellationToken);
        Task<PagedApiResponse<CourseResponse>> GetCoursesByUserGroupAsync(int? groupId, CourseListRequest request, int? tenantId, CancellationToken cancellationToken);
        Task<ApiResponse<List<CourseResponse>>> GetByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<CourseResponse>>> AddAsync(CourseRequest request, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<CourseResponse>>> UpdateAsync(int id, UpdateCourseRequest request, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<string>>> DeleteAsync(int id, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<CourseResponse>>> GetCourseByCategoryIdAsync(int categoryId, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<string>>> UploadVideoAsync(int courseId, IFormFile file, string title, string description, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<string>>> UploadDocumentAsync(int courseId, IFormFile file, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<LMS_SoulCode.Features.CourseVideos.DTOs.CourseVideoResponse>>> GetCourseVideosAsync(int courseId, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<LMS_SoulCode.Features.CourseVideos.DTOs.CourseDocumentResponse>>> GetCourseDocumentsAsync(int courseId, int? tenantId, CancellationToken cancellationToken = default);
        Task<byte[]> GetDecryptedFileAsync(string fullPath, CancellationToken cancellationToken = default);
        Task<(byte[] Bytes, string ContentType)?> GetMainImageContentAsync(int id, int? tenantId, CancellationToken cancellationToken = default);
        Task<(byte[] Bytes, string ContentType)?> GetThumbnailContentAsync(int id, int? tenantId, CancellationToken cancellationToken = default);
        Task<(byte[] Bytes, string ContentType, string FileName)?> GetDocumentContentAsync(int documentId, int? tenantId, CancellationToken cancellationToken = default);
    }

    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IWebHostEnvironment _env;
        private readonly CryptographyService _cryptoService;
        private readonly IMapper _mapper;
        private readonly IGroupService _groupService;

        public CourseService(ICourseRepository courseRepository, ICategoryRepository categoryRepo, IWebHostEnvironment env, CryptographyService cryptographyService, IMapper mapper, IGroupService groupService)
        {
            _courseRepo = courseRepository;
            _categoryRepo = categoryRepo;
            _env = env;
            _cryptoService = cryptographyService;
            _mapper = mapper;
            _groupService = groupService;
        }

        public async Task<PagedApiResponse<CourseResponse>> GetAllCourseAsync(CourseListRequest request, int? tenantId, CancellationToken cancellationToken)
        {
            // Security: Org Admin can only see their own tenant, SuperAdmin can specify or see all
            var targetTenantId = tenantId.HasValue 
                ? tenantId                  // Org Admin - force their own tenant
                : request.TenantId;         // SuperAdmin - use request or null
            
            // Allow null to mean 'All' as per DTO contract
            var isActiveFilter = request.IsActive;
            try
            {
                var (courses, totalCount) =
                    await _courseRepo.GetCoursesAsync(
                        request.SearchTerm,
                        request.PageNumber,
                        request.PageSize,
                        isActiveFilter,
                        targetTenantId,
                        cancellationToken);
      
            var response = courses.Select(c => 
            {
                var courseResponse = _mapper.Map<CourseResponse>(c);
                courseResponse.VideoUrls = c.Videos?.Select(v => $"/api/CourseVideo/stream/{v.Id}").ToList() ?? new List<string>();
                
                // Convert paths to absolute ID-based URLs
                if (!string.IsNullOrEmpty(c.CourseMainImageUrl))
                    courseResponse.CourseMainImageUrl = $"/api/Course/main-image/{c.Id}";
                if (!string.IsNullOrEmpty(c.ThumbnailUrl))
                    courseResponse.ThumbnailUrl = $"/api/Course/thumbnail/{c.Id}";
                
                return courseResponse;
            });

            return PagedApiResponse<CourseResponse>.Success(response, request.PageNumber, request.PageSize, totalCount, Messages.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }

        }

        public async Task<PagedApiResponse<CourseResponse>> GetCoursesByUserGroupAsync(int? groupId, CourseListRequest request, int? tenantId, CancellationToken cancellationToken)
        {
            try
            {
                var (courses, totalCount) =
                    await _courseRepo.GetCoursesByUserGroupAsync(
                        groupId,
                        request.SearchTerm,
                        request.PageNumber,
                        request.PageSize,
                        tenantId,
                        cancellationToken);
      
                // Same response mapping as GetAllCourseAsync - exact same data structure
                var response = courses.Select(c => 
                {
                    var courseResponse = _mapper.Map<CourseResponse>(c);
                    courseResponse.VideoUrls = c.Videos?.Select(v => $"/api/CourseVideo/stream/{v.Id}").ToList() ?? new List<string>();
                    
                    // Convert paths to absolute ID-based URLs
                    if (!string.IsNullOrEmpty(c.CourseMainImageUrl))
                        courseResponse.CourseMainImageUrl = $"/api/Course/main-image/{c.Id}";
                    if (!string.IsNullOrEmpty(c.ThumbnailUrl))
                        courseResponse.ThumbnailUrl = $"/api/Course/thumbnail/{c.Id}";
                    
                    return courseResponse;
                });

                return PagedApiResponse<CourseResponse>.Success(response, request.PageNumber, request.PageSize, totalCount, Messages.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<ApiResponse<List<CourseResponse>>> GetByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var c = await _courseRepo.GetByIdAsync(id, tenantId, cancellationToken);
            if (c == null) return ApiResponse<List<CourseResponse>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            var courseResponse = _mapper.Map<CourseResponse>(c);
            courseResponse.VideoUrls = c.Videos?.Select(v => $"/api/CourseVideo/stream/{v.Id}").ToList() ?? new List<string>();

            // Convert paths to absolute ID-based URLs
            if (!string.IsNullOrEmpty(c.CourseMainImageUrl))
                courseResponse.CourseMainImageUrl = $"/api/Course/main-image/{c.Id}";
            if (!string.IsNullOrEmpty(c.ThumbnailUrl))
                courseResponse.ThumbnailUrl = $"/api/Course/thumbnail/{c.Id}";

            return ApiResponse<List<CourseResponse>>.Success(new List<CourseResponse> { courseResponse }, Messages.Success);
        }

        public async Task<ApiResponse<List<CourseResponse>>> AddAsync(CourseRequest request, int? tenantId, CancellationToken cancellationToken = default)
        {
            // Validate Category Existence
            var category = await _categoryRepo.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
                return ApiResponse<List<CourseResponse>>.Fail("Invalid Category ID. Please select a valid category.", StatusCodes.BadRequest);

            var course = _mapper.Map<CourseEntity>(request);
            course.TenantId = tenantId; // Explicitly set tenantId
            
            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                var (mainPath, thumbPath) = await ProcessAndSaveCourseImagesAsync(request.ImageFile, cancellationToken);
                course.CourseMainImageUrl = mainPath;
                course.ThumbnailUrl = thumbPath;
            }

            await _courseRepo.AddAsync(course, cancellationToken);

            // Auto-add to tenant groups

            
            var courseResponse = _mapper.Map<CourseResponse>(course);
            if (!string.IsNullOrEmpty(course.CourseMainImageUrl)) courseResponse.CourseMainImageUrl = $"/api/Course/main-image/{course.Id}";
            if (!string.IsNullOrEmpty(course.ThumbnailUrl)) courseResponse.ThumbnailUrl = $"/api/Course/thumbnail/{course.Id}";

            return ApiResponse<List<CourseResponse>>.Success(new List<CourseResponse> { courseResponse }, Messages.Created);
        }

        public async Task<ApiResponse<IEnumerable<CourseResponse>>> GetCourseByCategoryIdAsync(int categoryId, int? tenantId, CancellationToken cancellationToken = default)
        {
            var courses = await _courseRepo.GetByCategoryIdAsync(categoryId, tenantId, cancellationToken);

            if (courses == null || !courses.Any())
                return ApiResponse<IEnumerable<CourseResponse>>.Fail(Messages.NoCoursesInCategory, StatusCodes.NotFound);

            var response = courses.Select(c => 
            {
                var courseResponse = _mapper.Map<CourseResponse>(c);
                courseResponse.VideoUrls = c.Videos?.Select(v => $"/api/CourseVideo/stream/{v.Id}").ToList() ?? new List<string>();
                
                // Convert paths to absolute ID-based URLs
                if (!string.IsNullOrEmpty(c.CourseMainImageUrl))
                    courseResponse.CourseMainImageUrl = $"/api/Course/main-image/{c.Id}";
                if (!string.IsNullOrEmpty(c.ThumbnailUrl))
                    courseResponse.ThumbnailUrl = $"/api/Course/thumbnail/{c.Id}";
                
                return courseResponse;
            });

            return ApiResponse<IEnumerable<CourseResponse>>.Success(response, Messages.Success);
        }

        public async Task<ApiResponse<List<string>>> UploadVideoAsync(int courseId, IFormFile file, string title, string description, int? tenantId, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                return ApiResponse<List<string>>.Fail(Messages.NoFileSelected, StatusCodes.BadRequest);

            // Validate File Type
            var ext = Path.GetExtension(file.FileName).ToLower();
            var allowedExtensions = new[] { ".mp4", ".mov", ".avi", ".webm", ".mkv" };
            if (!allowedExtensions.Contains(ext))
                return ApiResponse<List<string>>.Fail("Invalid video format. Allowed: .mp4, .mov, .avi, .webm, .mkv", StatusCodes.BadRequest);

            var course = await _courseRepo.GetByIdForAdminAsync(courseId, tenantId, cancellationToken);
            if (course == null)
                return ApiResponse<List<string>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            var rootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var folderPath = Path.Combine(rootPath, "uploads", "courses", courseId.ToString());
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid() + ext; // Use original extension (e.g., .mp4), NO .enc
            var filePath = Path.Combine(folderPath, fileName);

            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms, cancellationToken);
                fileBytes = ms.ToArray();
            }

            string encryptedBase64 = _cryptoService.EncryptBytes(fileBytes);
            await File.WriteAllTextAsync(filePath, encryptedBase64, cancellationToken);

            var relativePath = $"/uploads/courses/{courseId}/{fileName}";

            var video = new CourseVideo
            {
                CourseId = courseId,
                Title = title,
                Description = description,
                VideoUrl = relativePath // Strictly store the RELATIVE path in the DB
            };

            await _courseRepo.AddVideoAsync(video, cancellationToken);

            // Construct absolute URL for the response (ID based)
            var streamUrl = $"/api/CourseVideo/stream/{video.Id}";

            return ApiResponse<List<string>>.Success(new List<string> { streamUrl }, Messages.Uploaded);
        }

        public async Task<ApiResponse<List<string>>> UploadDocumentAsync(int courseId, IFormFile file, int? tenantId, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                return ApiResponse<List<string>>.Fail(Messages.NoFileSelected, StatusCodes.BadRequest);

            // Validate File Type
            var ext = Path.GetExtension(file.FileName).ToLower();
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt" };
            if (!allowedExtensions.Contains(ext))
                return ApiResponse<List<string>>.Fail("Invalid document format. Allowed: .pdf, .doc, .docx, .xls, .ppt, .txt", StatusCodes.BadRequest);

            var course = await _courseRepo.GetByIdForAdminAsync(courseId, tenantId, cancellationToken);
            if (course == null)
                return ApiResponse<List<string>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            var rootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var folderPath = Path.Combine(rootPath, "uploads", "courses", "docs", courseId.ToString());
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid() + ext + ".enc";
            var filePath = Path.Combine(folderPath, fileName);

            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms, cancellationToken);
                fileBytes = ms.ToArray();
            }

            string encryptedBase64 = _cryptoService.EncryptBytes(fileBytes);
            await File.WriteAllTextAsync(filePath, encryptedBase64, cancellationToken);

            var doc = new CourseDocument
            {
                CourseId = courseId,
                DocName = Path.GetFileNameWithoutExtension(file.FileName),
                DocUrl = $"/uploads/courses/docs/{courseId}/{fileName}"
            };

            await _courseRepo.AddDocsAsync(doc, cancellationToken);

            return ApiResponse<List<string>>.Success(new List<string> { doc.DocUrl }, Messages.Uploaded);
        }
    
        public async Task<ApiResponse<IEnumerable<LMS_SoulCode.Features.CourseVideos.DTOs.CourseVideoResponse>>> GetCourseVideosAsync(int courseId, int? tenantId, CancellationToken cancellationToken = default)
        {
             // Security check: Does course belong to tenant?
            var course = await _courseRepo.GetByIdForAdminAsync(courseId, tenantId, cancellationToken);
            if (course == null)
                 return ApiResponse<IEnumerable<LMS_SoulCode.Features.CourseVideos.DTOs.CourseVideoResponse>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            var videos = await _courseRepo.GetVideosByCourseIdAsync(courseId, cancellationToken);
            var response = _mapper.Map<IEnumerable<LMS_SoulCode.Features.CourseVideos.DTOs.CourseVideoResponse>>(videos);
            return ApiResponse<IEnumerable<LMS_SoulCode.Features.CourseVideos.DTOs.CourseVideoResponse>>.Success(response, Messages.Success);
        }

        public async Task<ApiResponse<IEnumerable<LMS_SoulCode.Features.CourseVideos.DTOs.CourseDocumentResponse>>> GetCourseDocumentsAsync(int courseId, int? tenantId, CancellationToken cancellationToken = default)
        {
             // Security check: Does course belong to tenant?
            var course = await _courseRepo.GetByIdForAdminAsync(courseId, tenantId, cancellationToken);
            if (course == null)
                 return ApiResponse<IEnumerable<LMS_SoulCode.Features.CourseVideos.DTOs.CourseDocumentResponse>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            var docs = await _courseRepo.GetDocsByCourseIdAsync(courseId, cancellationToken);
            var response = _mapper.Map<IEnumerable<LMS_SoulCode.Features.CourseVideos.DTOs.CourseDocumentResponse>>(docs);
            return ApiResponse<IEnumerable<LMS_SoulCode.Features.CourseVideos.DTOs.CourseDocumentResponse>>.Success(response, Messages.Success);
        }

        public async Task<ApiResponse<List<CourseResponse>>> UpdateAsync(int id, UpdateCourseRequest request, int? tenantId, CancellationToken cancellationToken = default)
        {
            var course = await _courseRepo.GetByIdForAdminAsync(id, tenantId, cancellationToken); // Use admin method to allow updating inactive courses
            if (course == null)
                return ApiResponse<List<CourseResponse>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            // Validate Category Existence if it's being updated (assuming request always has CategoryId)
            if (request.CategoryId > 0) 
            {
                 var category = await _categoryRepo.GetByIdAsync(request.CategoryId, cancellationToken);
                 if (category == null)
                    return ApiResponse<List<CourseResponse>>.Fail("Invalid Category ID. Please select a valid category.", StatusCodes.BadRequest);
            }

            // Use AutoMapper to update course properties
            _mapper.Map(request, course);

            // Handle image update if a new file is provided
            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                var (mainPath, thumbPath) = await ProcessAndSaveCourseImagesAsync(request.ImageFile, cancellationToken);
                course.CourseMainImageUrl = mainPath;
                course.ThumbnailUrl = thumbPath;
            }

            await _courseRepo.UpdateAsync(course, cancellationToken);

            var courseResponse = _mapper.Map<CourseResponse>(course);
            if (!string.IsNullOrEmpty(course.CourseMainImageUrl)) courseResponse.CourseMainImageUrl = $"/api/Course/main-image/{course.Id}";
            if (!string.IsNullOrEmpty(course.ThumbnailUrl)) courseResponse.ThumbnailUrl = $"/api/Course/thumbnail/{course.Id}";

            return ApiResponse<List<CourseResponse>>.Success(new List<CourseResponse> { courseResponse }, Messages.Updated);
        }

        private async Task<(string CourseMainImageUrl, string ThumbnailUrl)> ProcessAndSaveCourseImagesAsync(IFormFile file, CancellationToken cancellationToken)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();
            
            // Validate Image Type
            var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            if (!allowedImageExtensions.Contains(ext))
                throw new Exception("Invalid image format. Allowed: .jpg, .jpeg, .png, .webp, .gif");

            var folderName = "CourseThumbnail";
            var rootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var folderPath = Path.Combine(rootPath, "uploads", folderName);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // 1. Read original file
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, cancellationToken);
            var originalBytes = ms.ToArray();

            // 2. Generate Thumbnail (300px)
            ms.Position = 0;
            using var image = await Image.LoadAsync(ms, cancellationToken);
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(300, 0),
                Mode = ResizeMode.Max
            }));

            using var thumbMs = new MemoryStream();
            await image.SaveAsJpegAsync(thumbMs, cancellationToken);
            var thumbBytes = thumbMs.ToArray();

            // 3. Encrypt both
            string encryptedMain = _cryptoService.EncryptBytes(originalBytes);
            string encryptedThumb = _cryptoService.EncryptBytes(thumbBytes);

            // 4. Save files
            string fileGuid = Guid.NewGuid().ToString();
            string mainFileName = $"{fileGuid}_main{ext}.enc";
            string thumbFileName = $"{fileGuid}_thumb{ext}.enc";

            await File.WriteAllTextAsync(Path.Combine(folderPath, mainFileName), encryptedMain, cancellationToken);
            await File.WriteAllTextAsync(Path.Combine(folderPath, thumbFileName), encryptedThumb, cancellationToken);

            return ($"/uploads/{folderName}/{mainFileName}", $"/uploads/{folderName}/{thumbFileName}");
        }

        public async Task<ApiResponse<List<string>>> DeleteAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var course = await _courseRepo.GetByIdForAdminAsync(id, tenantId, cancellationToken); // Use admin method to allow deleting inactive courses
            if (course == null)
                return ApiResponse<List<string>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            // Remove course from all groups first
            await _groupService.RemoveCourseFromAllGroupsAsync(id, cancellationToken);

            await _courseRepo.DeleteAsync(id, cancellationToken);
            return ApiResponse<List<string>>.Success(new List<string>(), Messages.Deleted);
        }

        public async Task<byte[]> GetDecryptedFileAsync(string fullPath, CancellationToken cancellationToken = default)
        {
            string encryptedBase64 = await File.ReadAllTextAsync(fullPath, cancellationToken);
            return _cryptoService.DecryptBytes(encryptedBase64);
        }

        public async Task<(byte[] Bytes, string ContentType)?> GetMainImageContentAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var course = await _courseRepo.GetByIdForAdminAsync(id, tenantId, cancellationToken);
            if (course == null || string.IsNullOrEmpty(course.CourseMainImageUrl)) return null;

            var rootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var fullPath = Path.Combine(rootPath, course.CourseMainImageUrl.TrimStart('/'));

            if (!File.Exists(fullPath)) return null;

            var bytes = await GetDecryptedFileAsync(fullPath, cancellationToken);
            var contentType = Path.GetExtension(course.CourseMainImageUrl).ToLower().Contains("png") ? "image/png" : "image/jpeg";
            return (bytes, contentType);
        }

        public async Task<(byte[] Bytes, string ContentType)?> GetThumbnailContentAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var course = await _courseRepo.GetByIdForAdminAsync(id, tenantId, cancellationToken);
            if (course == null || string.IsNullOrEmpty(course.ThumbnailUrl)) return null;

            var rootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var fullPath = Path.Combine(rootPath, course.ThumbnailUrl.TrimStart('/'));

            if (!File.Exists(fullPath)) return null;

            var bytes = await GetDecryptedFileAsync(fullPath, cancellationToken);
            var contentType = Path.GetExtension(course.ThumbnailUrl).ToLower().Contains("png") ? "image/png" : "image/jpeg";
            return (bytes, contentType);
        }

        public async Task<(byte[] Bytes, string ContentType, string FileName)?> GetDocumentContentAsync(int documentId, int? tenantId, CancellationToken cancellationToken = default)
        {
            var document = await _courseRepo.GetDocumentByIdAsync(documentId, tenantId, cancellationToken);
            if (document == null || string.IsNullOrEmpty(document.DocUrl)) return null;

            var rootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var fullPath = Path.Combine(rootPath, document.DocUrl.TrimStart('/'));

            if (!File.Exists(fullPath)) return null;

            var bytes = await GetDecryptedFileAsync(fullPath, cancellationToken);
            var contentType = GetContentTypeFromExtension(document.DocUrl);
            var fileName = $"{document.DocName}{GetOriginalExtension(document.DocUrl)}";
            
            return (bytes, contentType, fileName);
        }

        private string GetContentTypeFromExtension(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLower().Replace(".enc", "");
            return ext switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }

        private string GetOriginalExtension(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            if (fileName.EndsWith(".enc"))
            {
                fileName = fileName.Substring(0, fileName.Length - 4); // Remove .enc
                return Path.GetExtension(fileName);
            }
            return Path.GetExtension(filePath);
        }
    }
}
