namespace LMS_SoulCode.Features.Reports.DTOs
{
    public record CourseProgressReportDto(int CourseId, int TotalVideos, int CompletedVideos, double CompletionPercentage);
}
