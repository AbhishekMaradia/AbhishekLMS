using Microsoft.AspNetCore.Http;

namespace LMS_SoulCode.Features.Organizations.DTOs
{
    public class UpdateOrganizationRequest
    {
        // Org Details
        public string? OrgName { get; set; }
        public string? Website { get; set; }
        public IFormFile? Logo { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public bool? IsActive { get; set; }

        // Admin Profile Details
        public string? AdminFirstName { get; set; }
        public string? AdminLastName { get; set; }
        public string? AdminEmail { get; set; }
        public string? AdminMobile { get; set; }

    }
}
