namespace LMS_SoulCode.Features.Course.DTOs
{
    public class UploadVideoRequest
    {
        public IFormFile File { get; set; } = null!;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
