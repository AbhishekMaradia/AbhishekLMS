using Microsoft.AspNetCore.Http;

namespace LMS_SoulCode.Features.Course.DTOs
{
    public class UpdateCourseRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string? Instructor { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? Difficulty { get; set; }
        public double? DurationHours { get; set; }
        public decimal? Price { get; set; }
        public int? Lectures { get; set; }
        public string? Materials { get; set; }
        public string? Tags { get; set; }
        public bool? IsActive { get; set; }
        public int? TenantId { get; set; }
    }
}