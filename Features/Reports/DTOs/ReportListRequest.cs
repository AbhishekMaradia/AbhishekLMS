using LMS_SoulCode.Features.Common;

namespace LMS_SoulCode.Features.Reports.DTOs
{
    public class ReportListRequest : BasePagedRequest
    {
        public string? SearchTerm { get; set; }
        public int? UserId { get; set; }
        public int? CourseId { get; set; }
        public DateTime? GeneratedFrom { get; set; }
        public DateTime? GeneratedTo { get; set; }
        public double? MinCompletionPercentage { get; set; }
        public double? MaxCompletionPercentage { get; set; }
        public int? GroupId { get; set; }
        public int? TenantId { get; set; }  // Optional - for SuperAdmin to specify org
    }
}