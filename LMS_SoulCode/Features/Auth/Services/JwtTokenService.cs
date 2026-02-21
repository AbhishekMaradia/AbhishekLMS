//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Tokens;
//using LMS_SoulCode.Features.Auth.Models;
//using LMS_SoulCode.Features.UserPermissions.Repositories; 
//using Microsoft.Extensions.Caching.Memory;
//using System.Linq; 

//namespace LMS_SoulCode.Features.Auth.Services;

//public class JwtTokenService
//{
//    private readonly string _key;
//    private readonly string _issuer;
//    private readonly string _audience;
//    private readonly int _expiryMinutes;
//    private readonly IUserPermissionRepository _repo; // Still needed for querying Roles

//    public JwtTokenService(IConfiguration config, IUserPermissionRepository repo)
//    {
//        _key = config["Jwt:Key"]!;
//        _issuer = config["Jwt:Issuer"]!;
//        _audience = config["Jwt:Audience"]!;
//        _expiryMinutes = int.Parse(config["Jwt:ExpiryMinutes"] ?? "60");
//        _repo = repo;
//    }

//    public async Task<(string Token, DateTime ExpiresAt)> CreateTokenAsync(User user)
//    {
//        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
//        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

//        // We no longer fetch granular permissions here.
//        // Permissions are now handled server-side via UserPermissionService & MemoryCache directly in the Attribute.

//        var claims = new List<Claim>
//        {
//            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
//            new Claim("username", user.UserName),
//            new Claim(JwtRegisteredClaimNames.Email, user.Email),
//            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
//        };

//        // Add User Roles to Claims (using Distinct to avoid duplicates)
//        var roles = await _repo.GetUserRolesAsync(user.Id);
//        // roles is List<string> where strings are Role Names
//        bool isSuperAdmin = roles.Any(r => string.Equals(r, "SUPER_ADMIN", StringComparison.OrdinalIgnoreCase));

//        foreach (var role in roles)
//        {
//            claims.Add(new Claim(ClaimTypes.Role, role));
//        }

//        // Add SuperAdmin flag if applicable - KEEPING THIS as it's a critical fast-path check
//        if (isSuperAdmin)
//        {
//            claims.Add(new Claim(LMS_SoulCode.Features.Common.SecurityConstants.IsSuperAdmin, "true"));
//        }

//        var expires = DateTime.UtcNow.AddMinutes(_expiryMinutes);
//        var token = new JwtSecurityToken(
//            issuer: _issuer,
//            audience: _audience,
//            claims: claims,
//            expires: expires,
//            signingCredentials: creds);

//        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
//    }
//}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using LMS_SoulCode.Features.Auth.Models;
using LMS_SoulCode.Features.UserPermissions.Repositories;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;

namespace LMS_SoulCode.Features.Auth.Services;

public class JwtTokenService
{
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryMinutes;
    private readonly IUserPermissionRepository _repo;
    private readonly IMemoryCache _cache;

    public JwtTokenService(IConfiguration config, IUserPermissionRepository repo, IMemoryCache cache)
    {
        _key = config["Jwt:Key"]!;
        _issuer = config["Jwt:Issuer"]!;
        _audience = config["Jwt:Audience"]!;
        _expiryMinutes = int.Parse(config["Jwt:ExpiryMinutes"] ?? "60");
        _repo = repo;
        _cache = cache;
    }

    public async Task<(string Token, DateTime ExpiresAt)> CreateTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        // Check cache first
        var cacheKey = $"jwt_token_{user.Id}";
        if (_cache.TryGetValue(cacheKey, out (string Token, DateTime ExpiresAt) cachedToken))
        {
            // Check if token is still valid (not expiring in next 5 minutes)
            if (cachedToken.ExpiresAt > DateTime.UtcNow.AddMinutes(5))
            {
                return cachedToken;
            }
        }

        // Generate new token
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("username", user.UserName),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.TenantId.HasValue)
        {
            claims.Add(new Claim("TenantId", user.TenantId.Value.ToString()));
        }

        if (user.GroupId.HasValue)
        {
            claims.Add(new Claim("GroupId", user.GroupId.Value.ToString()));
        }

        // Add User Roles to Claims (using cached method)
        var roles = await _repo.GetUserRolesAsync(user.Id, cancellationToken);
        bool isSuperAdmin = roles.Any(r => string.Equals(r, "SUPER_ADMIN", StringComparison.OrdinalIgnoreCase));

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add SuperAdmin flag if applicable
        if (isSuperAdmin)
        {
            claims.Add(new Claim(LMS_SoulCode.Features.Common.SecurityConstants.IsSuperAdmin, "true"));
        }

        var expires = DateTime.UtcNow.AddMinutes(_expiryMinutes);
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        var result = (new JwtSecurityTokenHandler().WriteToken(token), expires);

        // Cache token for 50 minutes (10 minutes before expiry) with size
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(50),
            Size = 2 // Larger size for JWT token
        };
        _cache.Set(cacheKey, result, cacheOptions);

        return result;
    }
}