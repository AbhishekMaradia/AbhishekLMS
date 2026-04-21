using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using LMS_SoulCode.Features.UserPermissions.Repositories;
using LMS_SoulCode.Features.UserPermissions.DTOs;
using System.Security.Claims;

namespace LMS_SoulCode.Features.Common
{
    public class BaseOfficePermissionAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _moduleCode;
        private readonly bool _isSuperAdminRequired;
        private readonly List<string> _permissionCodes;

        public BaseOfficePermissionAttribute(string moduleCode,bool isSuperAdminRequired = false,params string[] permissionCodes)
        {
            _moduleCode = moduleCode;
            _isSuperAdminRequired = isSuperAdminRequired;
            _permissionCodes = permissionCodes.ToList();
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 1. Basic authenticated check
            if (context.HttpContext.User.Identity?.IsAuthenticated != true)
            {
                context.Result = new ChallengeResult();
                return;
            }

            var user = context.HttpContext.User;

            // 2. Check Super Admin Fast Pass
            var isSuperAdminClaim = user.FindFirst(SecurityConstants.IsSuperAdmin)?.Value;
            bool isSuperAdmin = string.Equals(isSuperAdminClaim, "true", StringComparison.OrdinalIgnoreCase);

            if (isSuperAdmin)
            {
                await next();
                return;
            }

            // 3. If Super Admin is explicitly required
            if (_isSuperAdminRequired)
            {
                context.Result = new ForbidResult();
                return;
            }

            var requestServices = context.HttpContext.RequestServices;
            var cache = requestServices.GetRequiredService<IMemoryCache>();
            var repo = requestServices.GetRequiredService<IUserPermissionRepository>();

            // 5. Get User ID
            var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                context.Result = new ForbidResult();
                return;
            }

            try
            {
                // 6. Fetch Permissions (Cache -> DB)
                var cacheKey = $"user_permissions_{userId}";
                if (!cache.TryGetValue(cacheKey, out List<UserPermissionDto>? permissions))
                {
                    permissions = await repo.GetUserPermissionsAsync(userId);
                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                        Size = 1 
                    };
                    cache.Set(cacheKey, permissions, cacheOptions);
                }

                if (permissions != null && permissions.Any())
                {
                    // 7. Validate Permissions
                    // Filter by Module Code
                    var modulePerms = permissions.Where(p => 
                        string.Equals(p.ModuleCode, _moduleCode, StringComparison.OrdinalIgnoreCase) 
                        || p.ModuleCode == "*").ToList();

                    if (modulePerms.Any())
                    {
                        if (modulePerms.Any(p => p.PermissionCode == "*"))
                        {
                            await next();
                            return;
                        }

                        // Check for Specific Permissions
                        bool hasPermission = modulePerms.Any(p => 
                            _permissionCodes.Any(required => 
                                string.Equals(p.PermissionCode, required, StringComparison.OrdinalIgnoreCase)));

                        if (hasPermission)
                        {
                            await next();
                            return;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Fallthrough to Forbid on error
            }

            context.Result = new ForbidResult();
        }
    }
}
