namespace LMS_SoulCode.Features.Common
{
    public class CacheModule
    {
        public string Code { get; set; } = null!;  // Module code like "COURSE"
        public List<string> RightCode { get; set; } = new();  // Permission codes like ["COURSE_VIEW", "COURSE_ADD"]
    }
}