using System.Security.Claims;

namespace LMS_SoulCode.Features.Common
{
    public static class ClaimsExtensions
    {
        public static string? FindFirstValue(this ClaimsPrincipal user, string claimType)
        {
            return user?.FindFirst(claimType)?.Value;
        }

        public static int? GetTenantId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue("TenantId");
            return int.TryParse(value, out var id) ? id : null;
        }

        public static int? GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : null;
        }
    }
}