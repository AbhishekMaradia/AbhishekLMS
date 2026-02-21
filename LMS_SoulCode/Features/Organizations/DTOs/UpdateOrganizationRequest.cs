using Microsoft.AspNetCore.Http;

namespace LMS_SoulCode.Features.Organizations.DTOs
{
    public class UpdateOrganizationRequest
    {
        public string? OrgName { get; set; }
        public string? OrgCode { get; set; }
        public string? Website { get; set; }
        public IFormFile? Logo { get; set; }
        public bool? IsActive { get; set; }
    }
}
