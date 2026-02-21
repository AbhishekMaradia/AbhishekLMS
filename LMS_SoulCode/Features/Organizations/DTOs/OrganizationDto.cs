namespace LMS_SoulCode.Features.Organizations.DTOs
{
    public class OrganizationDto
    {
        public int Id { get; set; }
        public string OrgName { get; set; } = null!;
        public string OrgCode { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
