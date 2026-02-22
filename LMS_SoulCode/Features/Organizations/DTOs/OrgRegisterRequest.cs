using Microsoft.AspNetCore.Http;

namespace LMS_SoulCode.Features.Organizations.DTOs
{
    public record OrgRegisterRequest(
        // Org Details
        string OrgName,
        string OrgCode,
        string? Website,
        IFormFile? Logo,
        
        // Admin User Details
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string Mobile
    );
}
