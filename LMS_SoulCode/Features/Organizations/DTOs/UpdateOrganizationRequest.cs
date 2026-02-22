using Microsoft.AspNetCore.Http;

namespace LMS_SoulCode.Features.Organizations.DTOs
{
    public class UpdateOrganizationRequest
    {
        // Org Details
        public string? OrgName { get; set; }
        public string? OrgCode { get; set; }
        public string? Website { get; set; }
        public IFormFile? Logo { get; set; }
        public bool? IsActive { get; set; }

        // Admin User Details
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Mobile { get; set; }
        
        // Security
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}
