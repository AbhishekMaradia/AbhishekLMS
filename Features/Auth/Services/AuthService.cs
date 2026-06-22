using AutoMapper;
using LMS_SoulCode.Features.Auth.DTOs;
using LMS_SoulCode.Features.Auth.Repositories;
using LMS_SoulCode.Features.Auth.Models;
using LMS_SoulCode.Features.Common;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;
using LMS_SoulCode.Features.UserPermissions.Services;
using LMS_SoulCode.Features.Security.Services;
using LMS_SoulCode.Features.Organizations.Repositories;
using LMS_SoulCode.Features.UserPermissions.Models;
using LMS_SoulCode.Features.UserPermissions.Repositories;

namespace LMS_SoulCode.Features.Auth.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<List<LoginResponse>>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<LoginResponse>>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<ForgotPasswordResponse>>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<ResetPasswordResponse>>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly JwtTokenService _jwtService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly IUserPermissionService _permissionService;
        private readonly CryptographyService _cryptoService;
        private readonly IOrganizationRepository _orgRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IUserRoleRepository _userRoleRepo;

        public AuthService(
            IUserRepository userRepository, 
            JwtTokenService jwtTokenService, 
            IEmailService emailService, 
            IMapper mapper, 
            IUserPermissionService permissionService, 
            CryptographyService cryptoService, 
            IOrganizationRepository orgRepo,
            IRoleRepository roleRepo,
            IUserRoleRepository userRoleRepo)
        {
            _userRepo = userRepository;
            _jwtService = jwtTokenService;
            _emailService = emailService;
            _mapper = mapper;
            _permissionService = permissionService;
            _cryptoService = cryptoService;
            _orgRepo = orgRepo;
            _roleRepo = roleRepo;
            _userRoleRepo = userRoleRepo;
        }

        public async Task<ApiResponse<List<LoginResponse>>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepo.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return ApiResponse<List<LoginResponse>>.Fail(Messages.InvalidCredentials, StatusCodes.Unauthorized);
            if (!user.IsActive)
                return ApiResponse<List<LoginResponse>>.Fail(Messages.UserInactive, StatusCodes.Unauthorized);
            if (user.Organization != null && !user.Organization.IsActive && !user.Organization.IsDeleted)
                return ApiResponse<List<LoginResponse>>.Fail(Messages.OrgInactive, StatusCodes.Unauthorized);

            var (token, expires) = await _jwtService.CreateTokenAsync(user, cancellationToken);
            var userDto = await _userRepo.GetUserByIdAsync(user.Id, user.TenantId, cancellationToken) ?? _mapper.Map<UserDto>(user);
            string? encryptedPermissions = null;
            var permissionsResult = await _permissionService.GetUserPermissionsAsync(user.Id, user.TenantId, cancellationToken);
            if (permissionsResult.IsSuccess && permissionsResult.Data != null)
                encryptedPermissions = _cryptoService.EncryptDynamic(permissionsResult.Data);

            var dto = new LoginResponse(token, expires, userDto, encryptedPermissions);
            return ApiResponse<List<LoginResponse>>.Success(new List<LoginResponse> { dto }, Messages.LoginSuccess);
        }

        public async Task<ApiResponse<List<LoginResponse>>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            int? tenantId = request.TenantId;

            // Resolve via OrganizationCode
            if ((!tenantId.HasValue || tenantId.Value == 0) && !string.IsNullOrWhiteSpace(request.OrganizationCode))
            {
                var org = await _orgRepo.GetByCodeAsync(request.OrganizationCode.Trim().ToUpperInvariant(), cancellationToken);
                if (org == null)
                    return ApiResponse<List<LoginResponse>>.Fail(Messages.InvalidOrganizationCode, StatusCodes.BadRequest);
                tenantId = org.Id;
            }

            // Resolve via RegistrationToken (treated as organization code alias or encrypted token)
            if ((!tenantId.HasValue || tenantId.Value == 0) && !string.IsNullOrWhiteSpace(request.RegistrationToken))
            {
                string orgCode = request.RegistrationToken.Trim();
                try
                {
                    var decryptedBytes = _cryptoService.DecryptBytes(orgCode);
                    orgCode = System.Text.Encoding.UTF8.GetString(decryptedBytes);
                }
                catch
                {
                    // Fallback to raw value (for backwards compatibility if code is not encrypted)
                }

                var org = await _orgRepo.GetByCodeAsync(orgCode.ToUpperInvariant(), cancellationToken);
                if (org == null)
                    return ApiResponse<List<LoginResponse>>.Fail(Messages.InvalidOrganizationCode, StatusCodes.BadRequest);

                // NEW: Validate token expiry
                if (org.LinkExpiredAt.HasValue && DateTime.UtcNow > org.LinkExpiredAt.Value)
                    return ApiResponse<List<LoginResponse>>.Fail("Registration link has expired.", StatusCodes.BadRequest);

                tenantId = org.Id;
            }

            // Uniqueness checks within the resolved tenant (or globally if tenantId null)
            if (await _userRepo.IsEmailTakenAsync(request.Email, tenantId, cancellationToken))
                return ApiResponse<List<LoginResponse>>.Fail(Messages.EmailExists, StatusCodes.BadRequest);
            if (await _userRepo.IsMobileTakenAsync(request.Mobile, tenantId, cancellationToken))
                return ApiResponse<List<LoginResponse>>.Fail(Messages.MobileExists, StatusCodes.BadRequest);

            var user = _mapper.Map<User>(request);
            user.TenantId = tenantId;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            await _userRepo.AddAsync(user, cancellationToken);

            // Assign default STUDENT role if available
            var defaultRole = await _roleRepo.GetByCodeAsync("STUDENT", cancellationToken) 
                              ?? await _roleRepo.GetByCodeAsync("LEARNER", cancellationToken);
            if (defaultRole != null)
            {
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = defaultRole.Id,
                    TenantId = tenantId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _userRoleRepo.AddAsync(userRole, cancellationToken);
            }

            var (token, expires) = await _jwtService.CreateTokenAsync(user, cancellationToken);
            var userDto = await _userRepo.GetUserByIdAsync(user.Id, user.TenantId, cancellationToken) ?? _mapper.Map<UserDto>(user);

            string? encryptedPermissions = null;
            var permissionsResult = await _permissionService.GetUserPermissionsAsync(user.Id, user.TenantId, cancellationToken);
            if (permissionsResult.IsSuccess && permissionsResult.Data != null)
                encryptedPermissions = _cryptoService.EncryptDynamic(permissionsResult.Data);

            var dto = new LoginResponse(token, expires, userDto, encryptedPermissions);
            return ApiResponse<List<LoginResponse>>.Success(new List<LoginResponse> { dto }, Messages.Created);
        }

        public async Task<ApiResponse<List<ForgotPasswordResponse>>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepo.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
                return ApiResponse<List<ForgotPasswordResponse>>.Fail(Messages.EmailNotFound, StatusCodes.NotFound);
            var resetToken = await _userRepo.GenerateResetTokenAsync(request.Email, cancellationToken);
            if (resetToken == null)
                return ApiResponse<List<ForgotPasswordResponse>>.Fail(Messages.TokenCreationError, StatusCodes.ServerError);
            var dto = new ForgotPasswordResponse(resetToken, DateTime.UtcNow.AddHours(1));
            return ApiResponse<List<ForgotPasswordResponse>>.Success(new List<ForgotPasswordResponse> { dto }, Messages.TokenGenerated);
        }

        public async Task<ApiResponse<List<ResetPasswordResponse>>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepo.GetUserByResetTokenAsync(request.Token, cancellationToken);
            if (user == null)
                return ApiResponse<List<ResetPasswordResponse>>.Fail(Messages.InvalidToken, StatusCodes.BadRequest);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;
            await _userRepo.UpdatePasswordAsync(user, cancellationToken);
            var refreshToken = Guid.NewGuid().ToString();
            user.RefreshToken = refreshToken;
            var (newAccess, _) = await _jwtService.CreateTokenAsync(user, cancellationToken);
            var dto = new ResetPasswordResponse(Messages.PasswordUpdated, newAccess, refreshToken);
            return ApiResponse<List<ResetPasswordResponse>>.Success(new List<ResetPasswordResponse> { dto }, Messages.PasswordUpdated);
        }
    }
}
