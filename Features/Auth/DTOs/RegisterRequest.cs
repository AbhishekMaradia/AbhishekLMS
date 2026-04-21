namespace LMS_SoulCode.Features.Auth.DTOs
{
    public record RegisterRequest(string FirstName, string LastName, string Mobile, string Email, string Password, int? TenantId);
}
