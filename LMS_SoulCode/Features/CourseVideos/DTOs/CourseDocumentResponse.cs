namespace LMS_SoulCode.Features.CourseVideos.DTOs
{
    public class CourseDocumentResponse
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string DocName { get; set; } = string.Empty;
        public string DocUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
