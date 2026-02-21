using LMS_SoulCode.Features.CourseVideos.Models;
using LMS_SoulCode.Features.CourseVideos.Repositories;
using LMS_SoulCode.Features.Common;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;

namespace LMS_SoulCode.Features.CourseVideos.Services
{
    public class CourseDocumentService
    {
        private readonly ICourseDocumentRepository _repository;
        private readonly IWebHostEnvironment _env;

        public CourseDocumentService(ICourseDocumentRepository repository, IWebHostEnvironment env)
        {
            _repository = repository;
            _env = env;
        }

        public async Task<ApiResponse<IEnumerable<CourseDocument>>> GetByCourseAsync(int courseId, int? tenantId = null, CancellationToken cancellationToken = default)
        {
            var docs = await _repository.GetByCourseIdAsync(courseId, tenantId, cancellationToken);
            if (docs == null || !docs.Any())
                return ApiResponse<IEnumerable<CourseDocument>>.Fail("No documents found for this course", StatusCodes.NotFound);

            return ApiResponse<IEnumerable<CourseDocument>>.Success(docs, "Documents retrieved successfully");
        }
    }
}
