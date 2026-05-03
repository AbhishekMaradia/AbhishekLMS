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
using Microsoft.EntityFrameworkCore;
using LMS_SoulCode.Features.Groups.Repositories;
using LMS_SoulCode.Features.Course.Repositories;

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
        private readonly IMapper _mapper;
        private readonly IUserPermissionService _permissionService;
        private readonly IConfiguration _config;
        private readonly IGroupRepository _groupRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly ICategoryRepository _categoryRepo;

        public OrganizationService(
            IOrganizationRepository orgRepo,
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            IUserRoleRepository userRoleRepo,
            CryptographyService cryptoService,
            JwtTokenService jwtService,
            IWebHostEnvironment env,
            IMapper mapper,
            IUserPermissionService permissionService,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor,
            IGroupRepository groupRepo,
            ICourseRepository courseRepo,
            ICategoryRepository categoryRepo)
        {
            _orgRepo = orgRepo;
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _userRoleRepo = userRoleRepo;
            _cryptoService = cryptoService;
            _jwtService = jwtService;
            _env = env;
            _mapper = mapper;
            _permissionService = permissionService;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _groupRepo = groupRepo;
            _courseRepo = courseRepo;
            _categoryRepo = categoryRepo;
        }

        private readonly IHttpContextAccessor _httpContextAccessor;

        private string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null) return string.Empty;
            return $"{request.Scheme}://{request.Host}";
        }

        public async Task<ApiResponse<List<string>>> RegisterOrganizationAsync(OrgRegisterRequest request, CancellationToken cancellationToken = default)
        {
            if (await _orgRepo.ExistsAsync(request.OrgCode, cancellationToken))
                return ApiResponse<List<string>>.Fail(Messages.OrgAlreadyExists, StatusCodes.BadRequest);

            if (await _userRepo.IsEmailTakenAsync(request.Email, null, cancellationToken))
                return ApiResponse<List<string>>.Fail(Messages.AlreadyExists, StatusCodes.BadRequest);

            if (await _userRepo.IsMobileTakenAsync(request.Mobile, null, cancellationToken))
                return ApiResponse<List<string>>.Fail(Messages.MobileExists, StatusCodes.BadRequest);

            return await _orgRepo.ExecuteInTransactionAsync(async () =>
            {
                var org = _mapper.Map<Organization>(request);
                
                if (request.Logo != null && request.Logo.Length > 0)
                {
                    var (logoPath, thumbPath) = await SaveLogosAsync(request.Logo, cancellationToken);
                    org.LogoUrl = logoPath;
                    org.LogoThumbUrl = thumbPath;
                }

                await _orgRepo.AddAsync(org, cancellationToken);

                var user = _mapper.Map<User>(request);
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                user.TenantId = org.Id;

                await _userRepo.AddAsync(user, cancellationToken);

                var adminRole = await _roleRepo.GetByCodeAsync("ORGANIZATION_ADMIN", cancellationToken);
                if (adminRole == null) throw new Exception("Default ORGANIZATION_ADMIN role not found.");

                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = adminRole.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _userRoleRepo.AddAsync(userRole, cancellationToken);
                return ApiResponse<List<string>>.Success(new List<string>(), Messages.Created);
            }, cancellationToken);
        }

        private async Task<(string LogoUrl, string LogoThumbUrl)> SaveLogosAsync(IFormFile file, CancellationToken cancellationToken)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();
            var mainFolderPath = _config["AppSettings:OrgLogosMainPath"] ?? "wwwroot/uploads/OrgLogos/main";
            var thumbFolderPath = _config["AppSettings:OrgLogosThumbPath"] ?? "wwwroot/uploads/OrgLogos/thumb";

            if (!Directory.Exists(mainFolderPath)) Directory.CreateDirectory(mainFolderPath);
            if (!Directory.Exists(thumbFolderPath)) Directory.CreateDirectory(thumbFolderPath);

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, cancellationToken);
            
            var thumbSize = int.Parse(_config["AppSettings:OrgLogoThumbSize"] ?? "150");
    var (mainBytes, thumbBytes) = await _cryptoService.ProcessImageWithThumbAsync(ms, thumbSize, cancellationToken);


            string fileName = $"{Guid.NewGuid()}{ext}.enc";

            await _cryptoService.EncryptLargeFileAsync(new MemoryStream(mainBytes), Path.Combine(mainFolderPath, fileName), cancellationToken);
            await _cryptoService.EncryptLargeFileAsync(new MemoryStream(thumbBytes), Path.Combine(thumbFolderPath, fileName), cancellationToken);
            
            return (
                Path.Combine(mainFolderPath, fileName).Replace("\\", "/"),
                Path.Combine(thumbFolderPath, fileName).Replace("\\", "/")
            );
        }

        private void FormatOrgUrls(OrganizationDto dto, Organization? org)
        {
            if (org == null) return;
            var baseUrl = GetBaseUrl();
            var gateway = _config["AppSettings:CourseVideoUrl"] ?? "/api/Crypto/get?path=";

            if (!string.IsNullOrEmpty(org.LogoUrl))
                dto.LogoUrl = $"{baseUrl}{gateway}{Uri.EscapeDataString(org.LogoUrl)}";
            
            if (!string.IsNullOrEmpty(org.LogoThumbUrl))
                dto.LogoThumbUrl = $"{baseUrl}{gateway}{Uri.EscapeDataString(org.LogoThumbUrl)}";
        }

        public async Task<ApiResponse<List<LoginResponse>>> OrgLoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepo.GetByEmailAsync(request.Email, cancellationToken);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return ApiResponse<List<LoginResponse>>.Fail(Messages.InvalidCredentials, StatusCodes.Unauthorized);

            var (token, expires) = await _jwtService.CreateTokenAsync(user, cancellationToken);
            var userDto = _mapper.Map<UserDto>(user);

            // Fetch Org Details if not SuperAdmin to check active status and get name
            if (user.TenantId.HasValue && user.TenantId.Value > 0)
            {
                var org = await _orgRepo.GetByIdAsync(user.TenantId.Value, cancellationToken);
                if (org != null)
                {
                    if (!org.IsActive && !org.IsDeleted)
                        return ApiResponse<List<LoginResponse>>.Fail(Messages.OrgInactive, StatusCodes.Unauthorized);
                    
                    userDto.OrgName = org.Name;
                }
            }

            string? encryptedPermissions = null;
            var permissionsResult = await _permissionService.GetUserPermissionsAsync(user.Id, user.TenantId, cancellationToken);
            if (permissionsResult.IsSuccess && permissionsResult.Data != null)
            {
                encryptedPermissions = _cryptoService.EncryptDynamic(permissionsResult.Data);
            }

            var dto = new LoginResponse(token, expires, userDto, encryptedPermissions);
            return ApiResponse<List<LoginResponse>>.Success(new List<LoginResponse> { dto }, Messages.LoginSuccess);
        }

        public async Task<PagedApiResponse<OrganizationDto>> GetAllOrganizationsAsync(OrganizationListRequest request, bool isSuperAdmin, int? currentTenantId, CancellationToken cancellationToken)
        {
            // If not super-admin, we restrict the filter to their own tenantId instead of returning 403
            int? effectiveTenantId = isSuperAdmin ? null : currentTenantId;
            
            // Show all (Active + Inactive) for SuperAdmin by default
            bool? isActiveFilter = request.IsActive; 
            var (orgs, totalCount) = await _orgRepo.GetOrganizationsAsync(request.SearchTerm, request.PageNumber, request.PageSize, isActiveFilter, effectiveTenantId, cancellationToken);
            
            var dtos = _mapper.Map<IEnumerable<OrganizationDto>>(orgs).ToList();
            foreach (var dto in dtos)
            {
                FormatOrgUrls(dto, orgs.FirstOrDefault(o => o.Id == dto.Id));
            }

            return PagedApiResponse<OrganizationDto>.Success(dtos, request.PageNumber, request.PageSize, totalCount, Messages.Success);
        }

        public async Task<ApiResponse<List<OrganizationDto>>> GetOrganizationByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            if (tenantId.HasValue && id != tenantId.Value)
                 return ApiResponse<List<OrganizationDto>>.Fail(Messages.Forbidden, StatusCodes.Forbidden);

            var org = await _orgRepo.GetByIdAsync(id, cancellationToken);
            if (org == null) return ApiResponse<List<OrganizationDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            var dto = _mapper.Map<OrganizationDto>(org);
            FormatOrgUrls(dto, org);

            return ApiResponse<List<OrganizationDto>>.Success(new List<OrganizationDto> { dto }, Messages.Success);
        }

        public async Task<ApiResponse<List<OrganizationDto>>> UpdateOrganizationAsync(int id, UpdateOrganizationRequest request, int? tenantId, CancellationToken cancellationToken = default)
        {
            if (tenantId.HasValue && id != tenantId.Value)
                 return ApiResponse<List<OrganizationDto>>.Fail(Messages.Forbidden, StatusCodes.Forbidden);

            var org = await _orgRepo.GetByIdAsync(id, cancellationToken);
            if (org == null) return ApiResponse<List<OrganizationDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            var currentStatus = org.IsActive;
            _mapper.Map(request, org);
            if (!request.IsActive.HasValue) org.IsActive = currentStatus;

            if (request.Logo != null && request.Logo.Length > 0)
            {
                var (logoPath, thumbPath) = await SaveLogosAsync(request.Logo, cancellationToken);
                org.LogoUrl = logoPath;
                org.LogoThumbUrl = thumbPath;
            }
            await _orgRepo.UpdateAsync(org, cancellationToken);

            // Also update Admin Contact if provided
            var admin = await _userRepo.GetAdminUserByTenantIdAsync(org.Id, cancellationToken);
            if (admin != null)
            {
                if (!string.IsNullOrEmpty(request.AdminFirstName)) admin.FirstName = request.AdminFirstName;
                if (!string.IsNullOrEmpty(request.AdminLastName)) admin.LastName = request.AdminLastName;
                if (!string.IsNullOrEmpty(request.AdminEmail)) admin.Email = request.AdminEmail;
                if (!string.IsNullOrEmpty(request.AdminMobile)) admin.Mobile = request.AdminMobile;
                
                await _userRepo.UpdateAsync(admin, cancellationToken);
            }

            var dto = _mapper.Map<OrganizationDto>(org);
            FormatOrgUrls(dto, org);

            return ApiResponse<List<OrganizationDto>>.Success(new List<OrganizationDto> { dto }, Messages.Updated);
        }

        public async Task<ApiResponse<List<OrganizationDto>>> UpdateOrganizationProfileAsync(int id, UpdateOrganizationRequest request, int? tenantId, int? userId, CancellationToken cancellationToken = default)
        {
            if (tenantId.HasValue && id != tenantId.Value)
                return ApiResponse<List<OrganizationDto>>.Fail(Messages.Forbidden, StatusCodes.Forbidden);

            var org = await _orgRepo.GetByIdAsync(id, cancellationToken);
            if (org == null) return ApiResponse<List<OrganizationDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            var currentStatus = org.IsActive;
            _mapper.Map(request, org);
            if (!request.IsActive.HasValue) org.IsActive = currentStatus;

            if (request.Logo != null && request.Logo.Length > 0)
            {
                var (logoPath, thumbPath) = await SaveLogosAsync(request.Logo, cancellationToken);
                org.LogoUrl = logoPath;
                org.LogoThumbUrl = thumbPath;
            }
            await _orgRepo.UpdateAsync(org, cancellationToken);

            // Also update the specific user profile (if userId is provided) or the tenant admin
            var targetUserId = userId ?? (await _userRepo.GetAdminUserByTenantIdAsync(org.Id, cancellationToken))?.Id;
            if (targetUserId.HasValue)
            {
                var user = await _userRepo.GetByIdAsync(targetUserId.Value, cancellationToken);
                if (user != null)
                {
                    if (!string.IsNullOrEmpty(request.AdminFirstName)) user.FirstName = request.AdminFirstName;
                    if (!string.IsNullOrEmpty(request.AdminLastName)) user.LastName = request.AdminLastName;
                    if (!string.IsNullOrEmpty(request.AdminEmail)) user.Email = request.AdminEmail;
                    if (!string.IsNullOrEmpty(request.AdminMobile)) user.Mobile = request.AdminMobile;
                    
                    await _userRepo.UpdateAsync(user, cancellationToken);
                }
            }

            var dto = _mapper.Map<OrganizationDto>(org);
            FormatOrgUrls(dto, org);

            return ApiResponse<List<OrganizationDto>>.Success(new List<OrganizationDto> { dto }, Messages.Updated);
        }

        public async Task<ApiResponse<List<string>>> DeleteOrganizationAsync(int id, bool isSuperAdmin, CancellationToken cancellationToken = default)
        {
            if (!isSuperAdmin)
            {
                return ApiResponse<List<string>>.Fail(Messages.Forbidden, StatusCodes.Forbidden);
            }
            
            var org = await _orgRepo.GetByIdAsync(id, cancellationToken);
            if (org == null) return ApiResponse<List<string>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            // Strict Validation: Cannot delete if children exist
            if (await _groupRepo.AnyInTenantAsync(id, cancellationToken))
                return ApiResponse<List<string>>.Fail(Messages.DeleteBlockedOrgGroups, StatusCodes.BadRequest);

            if (await _userRepo.AnyUsersInTenantAsync(id, cancellationToken))
                return ApiResponse<List<string>>.Fail(Messages.DeleteBlockedOrgUsers, StatusCodes.BadRequest);

            if (await _courseRepo.AnyInTenantAsync(id, cancellationToken))
                return ApiResponse<List<string>>.Fail(Messages.DeleteBlockedOrgCourses, StatusCodes.BadRequest);

            if (await _categoryRepo.AnyInTenantAsync(id, cancellationToken))
                return ApiResponse<List<string>>.Fail(Messages.DeleteBlockedOrgCategories, StatusCodes.BadRequest);

            await _orgRepo.DeleteAsync(id, cancellationToken);
            return ApiResponse<List<string>>.Success(new List<string>(), Messages.Deleted);
        }

        public async Task<ApiResponse<UserDto>> GetOrganizationAdminAsync(int tenantId, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepo.GetAdminUserByTenantIdAsync(tenantId, cancellationToken);
            if (admin == null) return ApiResponse<UserDto>.Fail(Messages.NotFound, StatusCodes.NotFound);
            
            var dto = _mapper.Map<UserDto>(admin);
            return ApiResponse<UserDto>.Success(dto, Messages.Success);
        }
    }
}
