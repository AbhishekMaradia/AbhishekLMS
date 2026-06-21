namespace LMS_SoulCode.Features.Organizations.DTOs
{
    public class GenerateLinkRequest
    {
        public string? OrgCode { get; set; } // organization code or registration token
        public DateTimeOffset? Expiry { get; set; } // optional exact expiry datetime (UTC)
    }
}
