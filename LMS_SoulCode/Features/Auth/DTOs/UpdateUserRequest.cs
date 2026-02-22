namespace LMS_SoulCode.Features.Auth.DTOs
{
    public class UpdateUserRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int? TenantId { get; set; }
        public int? GroupId { get; set; }
    }
}