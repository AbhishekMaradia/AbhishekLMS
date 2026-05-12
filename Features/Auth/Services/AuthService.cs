using AutoMapper;
using LMS_SoulCode.Features.Auth.DTOs;
using LMS_SoulCode.Features.Auth.Repositories;
using LMS_SoulCode.Features.Auth.Models;
using LMS_SoulCode.Features.Common;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;
using LMS_SoulCode.Features.UserPermissions.Services;
using LMS_SoulCode.Features.Security.Services;
using LMS_SoulCode.Features.Organizations.Repositories;

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

        public AuthService(IUserRepository userRepository, JwtTokenService jwtTokenService, IEmailService emailService, IMapper mapper, IUserPermissionService permissionService, CryptographyService cryptoService, IOrganizationRepository orgRepo)
        {
            _userRepo = userRepository;
            _jwtService = jwtTokenService;
            _emailService = emailService;
            _mapper = mapper;
            _permissionService = permissionService;
            _cryptoService = cryptoService;
            _orgRepo = orgRepo;
        }


        public async Task<ApiResponse<List<LoginResponse>>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepo.GetByEmailAsync(request.Email, cancellationToken);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return ApiResponse<List<LoginResponse>>.Fail(Messages.InvalidCredentials, StatusCodes.Unauthorized);

            if (!user.IsActive)
                return ApiResponse<List<LoginResponse>>.Fail(Messages.UserInactive, StatusCodes.Unauthorized);

            // Check if organization is active
            if (user.Organization != null && !user.Organization.IsActive && !user.Organization.IsDeleted)
                return ApiResponse<List<LoginResponse>>.Fail(Messages.OrgInactive, StatusCodes.Unauthorized);

            var (token, expires) = await _jwtService.CreateTokenAsync(user, cancellationToken);

            var userDto = await _userRepo.GetUserByIdAsync(user.Id, user.TenantId, cancellationToken);
            if (userDto == null)
            {
                // Fallback to simple mapping if for some reason GetUserByIdAsync fails
                userDto = _mapper.Map<UserDto>(user);
            }

            // Fetch and Encrypt Permissions
            string? encryptedPermissions = null;
            var permissionsResult = await _permissionService.GetUserPermissionsAsync(user.Id, user.TenantId, cancellationToken);
            if (permissionsResult.IsSuccess && permissionsResult.Data != null)
            {
                encryptedPermissions = _cryptoService.EncryptDynamic(permissionsResult.Data);
            }

            var dto = new LoginResponse(token, expires, userDto, encryptedPermissions);

            return ApiResponse<List<LoginResponse>>.Success(new List<LoginResponse> { dto }, Messages.LoginSuccess);
        }



        public async Task<ApiResponse<List<LoginResponse>>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            int? tenantId = request.TenantId;

            // If TenantId is missing but OrganizationCode is provided, look it up
            if ((!tenantId.HasValue || tenantId.Value == 0) && !string.IsNullOrEmpty(request.OrganizationCode))
            {
                var org = await _orgRepo.GetByCodeAsync(request.OrganizationCode, cancellationToken);
                if (org != null)
                {
                    tenantId = org.Id;
                }
            }

            // check uniqueness within the tenant
            if (await _userRepo.IsEmailTakenAsync(request.Email, tenantId, cancellationToken))
                return ApiResponse<List<LoginResponse>>.Fail(Messages.EmailExists, StatusCodes.BadRequest);

            if (await _userRepo.IsMobileTakenAsync(request.Mobile, tenantId, cancellationToken))
                return ApiResponse<List<LoginResponse>>.Fail(Messages.MobileExists, StatusCodes.BadRequest);

            var user = _mapper.Map<User>(request);
            user.TenantId = tenantId;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            await _userRepo.AddAsync(user, cancellationToken);


            var (token, expires) = await _jwtService.CreateTokenAsync(user, cancellationToken);

            var userDto = await _userRepo.GetUserByIdAsync(user.Id, user.TenantId, cancellationToken);
            if (userDto == null)
            {
                userDto = _mapper.Map<UserDto>(user);
            }

            // Fetch and Encrypt Permissions
            string? encryptedPermissions = null;
            var permissionsResult = await _permissionService.GetUserPermissionsAsync(user.Id, user.TenantId, cancellationToken);
            if (permissionsResult.IsSuccess && permissionsResult.Data != null)
            {
                encryptedPermissions = _cryptoService.EncryptDynamic(permissionsResult.Data);
            }

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

            var dto = new ForgotPasswordResponse(
                resetToken,
                DateTime.UtcNow.AddHours(1)
            );

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

            var (newAccess, _) = await _jwtService.CreateTokenAsync(user, cancellationToken); // Updated to await async call

            var dto = new ResetPasswordResponse(
                Messages.PasswordUpdated,
                newAccess,
                refreshToken
            );

            return ApiResponse<List<ResetPasswordResponse>>.Success(new List<ResetPasswordResponse> { dto }, Messages.PasswordUpdated);
        }
    }
}
