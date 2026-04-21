namespace LMS_SoulCode.Features.Course.DTOs
{
    public record CategoryRequest
    {
        public string CategoryName { get; set; } = string.Empty;
        public int? TenantId { get; set; }
    }
}
