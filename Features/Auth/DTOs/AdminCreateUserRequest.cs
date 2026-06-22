namespace LMS_SoulCode.Features.Auth.DTOs
{
    public class AdminCreateUserRequest
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Mobile { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int? TenantId { get; set; }
        public int? GroupId { get; set; }
        public List<int> GroupIds { get; set; } = new();
        public List<int> RoleIds { get; set; } = new();
        public bool IsActive { get; set; } = true;
    }
}
