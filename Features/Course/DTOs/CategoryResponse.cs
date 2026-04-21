namespace LMS_SoulCode.Features.Course.DTOs
{
    public class CategoryResponse
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public int? TenantId { get; set; }
        public string? OrgName { get; set; }
    }
}