namespace LMS_SoulCode.Features.CourseVideos.DTOs
{
    public record CourseDocumentResponse(int Id, int CourseId, string DocName, string DocUrl, DateTime CreatedAt);   

}
