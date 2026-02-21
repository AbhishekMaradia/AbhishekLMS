using LMS_SoulCode.Data;
using LMS_SoulCode.Features.Auth.Models;
using LMS_SoulCode.Features.Auth.Services;
using LMS_SoulCode.Features.Auth.Repositories;
using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.Organizations.DTOs;
using LMS_SoulCode.Features.Organizations.Models;
using LMS_SoulCode.Features.Organizations.Repositories;
using LMS_SoulCode.Features.Security.Services;
using LMS_SoulCode.Features.UserPermissions.Models;
using LMS_SoulCode.Features.UserPermissions.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;
using LMS_SoulCode.Features.UserPermissions.Services;
using LMS_SoulCode.Features.Auth.DTOs;

namespace LMS_SoulCode.Features.Organizations.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository _orgRepo;
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IUserRoleRepository _userRoleRepo;
        private readonly CryptographyService _cryptoService;
        private readonly JwtTokenService _jwtService;
        private readonly IWebHostEnvironment _env;
        private readonly LmsDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUserPermissionService _permissionService;

        public OrganizationService(
            IOrganizationRepository orgRepo,
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            IUserRoleRepository userRoleRepo,
            CryptographyService cryptoService,
            JwtTokenService jwtService,
            IWebHostEnvironment env,
            LmsDbContext context,
            IMapper mapper,
            IUserPermissionService permissionService)
        {
            _orgRepo = orgRepo;
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _userRoleRepo = userRoleRepo;
            _cryptoService = cryptoService;
            _jwtService = jwtService;
            _env = env;
            _context = context;
            _mapper = mapper;
            _permissionService = permissionService;
        }

        public async Task<ApiResponse<List<string>>> RegisterOrganizationAsync(OrgRegisterRequest request, CancellationToken cancellationToken = default)
        {
            // 1. Validation
            if (await _orgRepo.ExistsAsync(request.OrgCode, cancellationToken))
                return ApiResponse<List<string>>.Fail(Messages.OrgAlreadyExists, StatusCodes.BadRequest);

            if (await _userRepo.IsEmailTakenAsync(request.Email, null, cancellationToken))
                return ApiResponse<List<string>>.Fail(Messages.AlreadyExists, StatusCodes.BadRequest);

            if (await _userRepo.IsMobileTakenAsync(request.Mobile, null, cancellationToken))
                return ApiResponse<List<string>>.Fail(Messages.MobileExists, StatusCodes.BadRequest);

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // 2. Create Organization
                    var org = _mapper.Map<Organization>(request);
                    
                    if (request.Logo != null && request.Logo.Length > 0)
                    {
                        org.LogoUrl = await SaveLogoAsync(request.Logo, cancellationToken);
                    }

                    await _orgRepo.AddAsync(org, cancellationToken);

                    // 3. Create Admin User
                    var user = _mapper.Map<User>(request);
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                    user.TenantId = org.Id;

                    await _userRepo.AddAsync(user, cancellationToken);

                    // 4. Assign Role
                    var adminRole = await _roleRepo.GetByCodeAsync("ORGANIZATION_ADMIN", cancellationToken);
                    if (adminRole == null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return ApiResponse<List<string>>.Fail("Default ORGANIZATION_ADMIN role not found.", StatusCodes.ServerError);
                    }

                    var userRole = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = adminRole.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _userRoleRepo.AddAsync(userRole, cancellationToken);

                    // 5. Setup default roles and permissions for organization
                    // You can inject IOrganizationOnboardingService and call:
                    // await _onboardingService.SetupDefaultRolesAndPermissionsAsync(org.Id, cancellationToken);

                    await transaction.CommitAsync(cancellationToken);
                    return ApiResponse<List<string>>.Success(new List<string>(), Messages.Created);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return ApiResponse<List<string>>.Fail(Messages.Error, StatusCodes.ServerError);
                }
            });
        }

        private async Task<string> SaveLogoAsync(IFormFile file, CancellationToken cancellationToken)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();
            var folderName = "OrgLogos";
            var rootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var folderPath = Path.Combine(rootPath, "uploads", folderName);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, cancellationToken);
            var fileBytes = ms.ToArray();

            string encryptedBase64 = _cryptoService.EncryptBytes(fileBytes);
            
            string fileName = $"{Guid.NewGuid()}{ext}.enc";
            var filePath = Path.Combine(folderPath, fileName);
            
            await File.WriteAllTextAsync(filePath, encryptedBase64, cancellationToken);
            
            return $"/uploads/{folderName}/{fileName}";
        }

        //public async Task<ApiResponse<LMS_SoulCode.Features.Auth.DTOs.LoginResponse>> OrgLoginAsync(OrgLoginRequest request, CancellationToken cancellationToken = default)
        //{
        //    // 1. Find User by Email (Identifier)
        //    var user = await _userRepo.GetByUsernameOrEmailAsync(request.Email, cancellationToken);
        //    if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        //        return ApiResponse<LMS_SoulCode.Features.Auth.DTOs.LoginResponse>.Fail(Messages.InvalidCredentials, StatusCodes.Unauthorized);

        //    // 2. Validate Organization
        //    if (user.Organization != null && !user.Organization.IsActive)
        //         return ApiResponse<LMS_SoulCode.Features.Auth.DTOs.LoginResponse>.Fail(Messages.OrgInactive, StatusCodes.Unauthorized);

        //    var orgName = user.Organization?.Name ?? "Unknown";

        //    // 3. Generate Token
        //    var (token, expires) = await _jwtService.CreateTokenAsync(user);

        //    var userDto = _mapper.Map<LMS_SoulCode.Features.Auth.DTOs.UserDto>(user);

        //    // Fetch and Encrypt Permissions
        //    string? encryptedPermissions = null;
        //    var permissionsResult = await _permissionService.GetUserPermissionsAsync(user.Id, user.TenantId, cancellationToken);
        //    if (permissionsResult.IsSuccess && permissionsResult.Result != null)
        //    {
        //        encryptedPermissions = _cryptoService.EncryptDynamic(permissionsResult.Result);
        //    }

        //    var response = new LMS_SoulCode.Features.Auth.DTOs.LoginResponse(token, expires, userDto, encryptedPermissions);

        //    return ApiResponse<LMS_SoulCode.Features.Auth.DTOs.LoginResponse>.Success(response, Messages.LoginSuccess);
        //}

        public async Task<ApiResponse<List<LoginResponse>>> OrgLoginAsync(OrgLoginRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepo.GetByUsernameOrEmailAsync(request.Email, cancellationToken);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return ApiResponse<List<LoginResponse>>.Fail(Messages.InvalidCredentials, StatusCodes.Unauthorized);

            // Check if organization is active
            if (user.Organization != null && !user.Organization.IsActive && !user.Organization.IsDeleted)
                return ApiResponse<List<LoginResponse>>.Fail(Messages.OrgInactive, StatusCodes.Unauthorized);

            var (token, expires) = await _jwtService.CreateTokenAsync(user, cancellationToken);

            var userDto = _mapper.Map<UserDto>(user);

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

        public async Task<PagedApiResponse<OrganizationDto>> GetAllOrganizationsAsync(OrganizationListRequest request, CancellationToken cancellationToken)
        {
            var (orgs, totalCount) = await _orgRepo.GetOrganizationsAsync(request.SearchTerm, request.PageNumber, request.PageSize, request.IsActive, cancellationToken);
            
            var dtos = _mapper.Map<IEnumerable<OrganizationDto>>(orgs);

            return PagedApiResponse<OrganizationDto>.Success(dtos, request.PageNumber, request.PageSize, totalCount, Messages.Success);
        }

        public async Task<ApiResponse<List<OrganizationDto>>> GetOrganizationByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            if (tenantId.HasValue && id != tenantId.Value)
                 return ApiResponse<List<OrganizationDto>>.Fail(Messages.Forbidden, StatusCodes.Forbidden);

            var org = await _orgRepo.GetByIdAsync(id, cancellationToken);
            if (org == null) return ApiResponse<List<OrganizationDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            var dto = _mapper.Map<OrganizationDto>(org);

            return ApiResponse<List<OrganizationDto>>.Success(new List<OrganizationDto> { dto }, Messages.Success);
        }

        public async Task<ApiResponse<List<OrganizationDto>>> UpdateOrganizationAsync(int id, UpdateOrganizationRequest request, int? tenantId, CancellationToken cancellationToken = default)
        {
            if (tenantId.HasValue && id != tenantId.Value)
                 return ApiResponse<List<OrganizationDto>>.Fail(Messages.Forbidden, StatusCodes.Forbidden);

            var org = await _orgRepo.GetByIdAsync(id, cancellationToken);
            if (org == null) return ApiResponse<List<OrganizationDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            _mapper.Map(request, org);

            if (request.Logo != null && request.Logo.Length > 0)
            {
                org.LogoUrl = await SaveLogoAsync(request.Logo, cancellationToken);
            }

            await _orgRepo.UpdateAsync(org, cancellationToken); // BaseRepository handles Update

            var dto = _mapper.Map<OrganizationDto>(org);
            return ApiResponse<List<OrganizationDto>>.Success(new List<OrganizationDto> { dto }, Messages.Updated);
        }

        public async Task<ApiResponse<List<string>>> DeleteOrganizationAsync(int id, CancellationToken cancellationToken = default)
        {
            var org = await _orgRepo.GetByIdAsync(id, cancellationToken);
            if (org == null) return ApiResponse<List<string>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            await _orgRepo.DeleteAsync(id, cancellationToken); // Soft delete via BaseRepository
            // Optionally deletes valid users? - Left for future consideration

            return ApiResponse<List<string>>.Success(new List<string>(), Messages.Deleted);
        }

        public async Task<ApiResponse<List<OrganizationDto>>> UpdateOrganizationProfileAsync(int id, UpdateOrganizationRequest request, int? tenantId, CancellationToken cancellationToken = default)
        {
             if (tenantId.HasValue && id != tenantId.Value)
                 return ApiResponse<List<OrganizationDto>>.Fail(Messages.Forbidden, StatusCodes.Forbidden);

            // Similar to Update, but restrict fields if needed (e.g., prevent deactivating self)
            // For now, reuse same logic but ensure OrgAdmin can only update own ID (controller check)
            
            var org = await _orgRepo.GetByIdAsync(id, cancellationToken);
            if (org == null) return ApiResponse<List<OrganizationDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            _mapper.Map(request, org);
            // Ensure IsActive or OrgCode aren't accidentally updated if Profile update should be restricted
            // (The Map handles this if the DTO fields are null, and Mapper Profile has conditions)

            if (request.Logo != null && request.Logo.Length > 0)
            {
                org.LogoUrl = await SaveLogoAsync(request.Logo, cancellationToken);
            }

            await _orgRepo.UpdateAsync(org, cancellationToken);

            var dto = _mapper.Map<OrganizationDto>(org);
            return ApiResponse<List<OrganizationDto>>.Success(new List<OrganizationDto> { dto }, Messages.Updated);
        }
    }
}
