namespace LMS_SoulCode.Features.Auth.DTOs
{
    public record ResetPasswordRequest(string Token, string NewPassword, string ConfirmPassword);
}
