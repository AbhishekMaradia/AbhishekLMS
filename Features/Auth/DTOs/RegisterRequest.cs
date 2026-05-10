namespace LMS_SoulCode.Features.Auth.DTOs
{
    public class RegisterRequest
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Mobile { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int? TenantId { get; set; }
        public string? OrganizationCode { get; set; }
    }
}
