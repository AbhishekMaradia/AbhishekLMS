using System.Collections.Generic;

namespace LMS_SoulCode.Features.Reports.DTOs
{
    public record VideoProgressDto(int VideoId, string Title, double WatchedPercentage, bool IsCompleted);
    public record CourseProgressReportDto(int CourseId, int TotalVideos, int CompletedVideos, double CompletionPercentage, List<VideoProgressDto> VideoDetails);
}
