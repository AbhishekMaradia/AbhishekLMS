namespace LMS_SoulCode.Features.Organizations.DTOs
{
    public class GenerateLinkResponse
    {
        public string Url { get; set; } = null!;
        public DateTime Expiry { get; set; }
    }
}
