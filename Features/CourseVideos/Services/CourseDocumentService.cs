using LMS_SoulCode.Features.CourseVideos.Models;
using LMS_SoulCode.Features.CourseVideos.Repositories;
using LMS_SoulCode.Features.CourseVideos.DTOs;
using LMS_SoulCode.Features.Security.Services;
using LMS_SoulCode.Features.Course.Repositories;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;
using LMS_SoulCode.Features.Common;
using Microsoft.AspNetCore.Http;
using AutoMapper;

namespace LMS_SoulCode.Features.CourseVideos.Services
{
    public class CourseDocumentService
    {
        private readonly ICourseDocumentRepository _repository;
        private readonly ICourseRepository _courseRepo;
        private readonly IWebHostEnvironment _env;
        private readonly CryptographyService _cryptoService;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public CourseDocumentService(
            ICourseDocumentRepository repository, 
            ICourseRepository courseRepo,
            IWebHostEnvironment env,
            CryptographyService cryptoService,
            IConfiguration config,
            IMapper mapper)
        {
            _repository = repository;
            _courseRepo = courseRepo;
            _env = env;
            _cryptoService = cryptoService;
            _config = config;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<CourseDocumentResponse>>> GetByCourseAsync(int courseId, int? tenantId = null, CancellationToken cancellationToken = default)
        {
            var docs = await _repository.GetByCourseIdAsync(courseId, tenantId, cancellationToken);
            var responseList = _mapper.Map<IEnumerable<CourseDocumentResponse>>(docs).ToList();
            
            foreach (var doc in responseList)
            {
                doc.DocUrl = $"/api/CourseDocument/download/{doc.Id}";
            }

            return ApiResponse<IEnumerable<CourseDocumentResponse>>.Success(responseList, "Documents retrieved successfully");
        }

        public async Task<CourseDocument?> GetRawByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var doc = await _repository.GetByIdAsync(id, cancellationToken);
            if (doc != null && (!tenantId.HasValue || tenantId == 0 || doc.TenantId == tenantId.Value))
                return doc;
            return null;
        }

        public async Task<ApiResponse<List<string>>> UploadAsync(int courseId, IFormFile file, string docName, string description, int? tenantId, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                return ApiResponse<List<string>>.Fail(Messages.NoFileSelected, StatusCodes.BadRequest);

            var course = await _courseRepo.GetByIdForAdminAsync(courseId, tenantId, cancellationToken);
            if (course == null) return ApiResponse<List<string>>.Fail("Course not found", StatusCodes.NotFound);

            var ext = Path.GetExtension(file.FileName).ToLower();
            // All extensions are allowed as per user request
            
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
                DocName = string.IsNullOrEmpty(docName) ? Path.GetFileNameWithoutExtension(file.FileName) : docName,
                Description = description ?? string.Empty,
                DocUrl = dbPath,
                TenantId = course.TenantId
            };

            await _repository.AddAsync(doc, cancellationToken);
            var gateway = _config["AppSettings:CourseVideoUrl"] ?? "/api/Crypto/get?path=";

            var publicUrl = $"{gateway}{Uri.EscapeDataString(doc.DocUrl)}";
            return ApiResponse<List<string>>.Success(new List<string> { publicUrl }, Messages.Uploaded);
        }

        public async Task<ApiResponse<string>> DeleteAsync(int documentId, int? tenantId, CancellationToken cancellationToken = default)
        {
            var doc = await _repository.GetByIdAsync(documentId, cancellationToken);
            if (doc == null || (tenantId.HasValue && tenantId != 0 && doc.TenantId != tenantId.Value && doc.TenantId != null))
                return ApiResponse<string>.Fail(Messages.NotFound, StatusCodes.NotFound);

            var fullPath = Path.Combine(_env.ContentRootPath, doc.DocUrl.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            await _repository.DeleteAsync(doc.Id, cancellationToken);
            return ApiResponse<string>.Success(Messages.Success);
        }

        public async Task<ApiResponse<string>> UpdateAsync(int documentId, string docName, string description, int? tenantId, CancellationToken cancellationToken = default)
        {
            var doc = await _repository.GetByIdAsync(documentId, cancellationToken);
            if (doc == null || (tenantId.HasValue && tenantId != 0 && doc.TenantId != tenantId.Value && doc.TenantId != null))
                return ApiResponse<string>.Fail(Messages.NotFound, StatusCodes.NotFound);

            doc.DocName = docName;
            doc.Description = description;
            await _repository.UpdateAsync(doc, cancellationToken);
            return ApiResponse<string>.Success(Messages.Success);
        }
    }
}
