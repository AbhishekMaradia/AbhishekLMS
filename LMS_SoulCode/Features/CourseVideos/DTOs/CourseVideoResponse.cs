namespace LMS_SoulCode.Features.CourseVideos.DTOs
{
    public record CourseVideoResponse(int Id, int CourseId, string Title, string VideoUrl, DateTime CreatedAt);   

}
