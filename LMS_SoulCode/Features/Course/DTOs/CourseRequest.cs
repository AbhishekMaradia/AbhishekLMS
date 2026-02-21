using Microsoft.AspNetCore.Http;

namespace LMS_SoulCode.Features.Course.DTOs
{
    public class CourseRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string Instructor { get; set; } = string.Empty;
        public IFormFile? ImageFile { get; set; }
        public string Difficulty { get; set; } = string.Empty;
        public double DurationHours { get; set; }
        public decimal Price { get; set; }
        public int Lectures { get; set; }
        public string? Materials { get; set; }
        public string? Tags { get; set; }
        public int? TenantId { get; set; }
    }
}
