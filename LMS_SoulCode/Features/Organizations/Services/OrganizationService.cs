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
        private readonly IConfiguration _config;

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
            IUserPermissionService permissionService,
            IConfiguration config)
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
            _config = config;
        }

        public async Task<ApiResponse<List<string>>> RegisterOrganizationAsync(OrgRegisterRequest request, CancellationToken cancellationToken = default)
        {
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
                await transaction.CommitAsync(cancellationToken);
                return ApiResponse<List<string>>.Success(new List<string>(), Messages.Created);
            });
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
            var gateway = _config["AppSettings:CourseVideoUrl"] ?? "/api/Crypto/get?path=";

            if (!string.IsNullOrEmpty(org.LogoUrl))
                dto.LogoUrl = $"{gateway}{Uri.EscapeDataString(org.LogoUrl)}";
            
            if (!string.IsNullOrEmpty(org.LogoThumbUrl))
                dto.LogoThumbUrl = $"{gateway}{Uri.EscapeDataString(org.LogoThumbUrl)}";
        }

        public async Task<ApiResponse<List<LoginResponse>>> OrgLoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepo.GetByEmailAsync(request.Email, cancellationToken);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return ApiResponse<List<LoginResponse>>.Fail(Messages.InvalidCredentials, StatusCodes.Unauthorized);

            if (user.Organization != null && !user.Organization.IsActive && !user.Organization.IsDeleted)
                return ApiResponse<List<LoginResponse>>.Fail(Messages.OrgInactive, StatusCodes.Unauthorized);

            var (token, expires) = await _jwtService.CreateTokenAsync(user, cancellationToken);
            var userDto = _mapper.Map<UserDto>(user);

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
            bool? isActiveFilter = request.IsActive ?? true;
            var (orgs, totalCount) = await _orgRepo.GetOrganizationsAsync(request.SearchTerm, request.PageNumber, request.PageSize, isActiveFilter, cancellationToken);
            
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

            var adminUser = await _userRepo.GetAdminUserByTenantIdAsync(id, cancellationToken);
            
            if (adminUser != null)
            {
                if (!string.IsNullOrEmpty(request.Email) && request.Email != adminUser.Email)
                {
                    if (await _userRepo.IsEmailTakenAsync(request.Email, null, cancellationToken))
                        return ApiResponse<List<OrganizationDto>>.Fail(Messages.EmailExists, StatusCodes.BadRequest);
                }

                if (!string.IsNullOrEmpty(request.Mobile) && request.Mobile != adminUser.Mobile)
                {
                    if (await _userRepo.IsMobileTakenAsync(request.Mobile, null, cancellationToken))
                        return ApiResponse<List<OrganizationDto>>.Fail(Messages.MobileExists, StatusCodes.BadRequest);
                }
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                _mapper.Map(request, org);
                if (request.Logo != null && request.Logo.Length > 0)
                {
                    var (logoPath, thumbPath) = await SaveLogosAsync(request.Logo, cancellationToken);
                    org.LogoUrl = logoPath;
                    org.LogoThumbUrl = thumbPath;
                }
                await _orgRepo.UpdateAsync(org, cancellationToken);

                if (adminUser != null)
                {
                    _mapper.Map(request, adminUser);
                    if (!string.IsNullOrEmpty(request.Password))
                    {
                        adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                    }
                    await _userRepo.UpdateAsync(adminUser, cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
                var dto = _mapper.Map<OrganizationDto>(org);
                FormatOrgUrls(dto, org);
                return ApiResponse<List<OrganizationDto>>.Success(new List<OrganizationDto> { dto }, Messages.Updated);
            });
        }

        public async Task<ApiResponse<List<OrganizationDto>>> UpdateOrganizationProfileAsync(int id, UpdateOrganizationRequest request, int? tenantId, int? userId, CancellationToken cancellationToken = default)
        {
            if (tenantId.HasValue && id != tenantId.Value)
                return ApiResponse<List<OrganizationDto>>.Fail(Messages.Forbidden, StatusCodes.Forbidden);

            if (!userId.HasValue) return ApiResponse<List<OrganizationDto>>.Fail(Messages.Unauthorized, StatusCodes.Unauthorized);

            var org = await _orgRepo.GetByIdAsync(id, cancellationToken);
            if (org == null) return ApiResponse<List<OrganizationDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            var user = await _userRepo.GetByIdAsync(userId.Value, cancellationToken);
            if (user == null) return ApiResponse<List<OrganizationDto>>.Fail(Messages.UserNotFound, StatusCodes.NotFound);

            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                if (await _userRepo.IsEmailTakenAsync(request.Email, null, cancellationToken))
                    return ApiResponse<List<OrganizationDto>>.Fail(Messages.EmailExists, StatusCodes.BadRequest);
            }

            if (!string.IsNullOrEmpty(request.Mobile) && request.Mobile != user.Mobile)
            {
                if (await _userRepo.IsMobileTakenAsync(request.Mobile, null, cancellationToken))
                    return ApiResponse<List<OrganizationDto>>.Fail(Messages.MobileExists, StatusCodes.BadRequest);
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                _mapper.Map(request, org);
                if (request.Logo != null && request.Logo.Length > 0)
                {
                    var (logoPath, thumbPath) = await SaveLogosAsync(request.Logo, cancellationToken);
                    org.LogoUrl = logoPath;
                    org.LogoThumbUrl = thumbPath;
                }
                await _orgRepo.UpdateAsync(org, cancellationToken);

                _mapper.Map(request, user);
                if (!string.IsNullOrEmpty(request.Password))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                }
                await _userRepo.UpdateAsync(user, cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                var dto = _mapper.Map<OrganizationDto>(org);
                FormatOrgUrls(dto, org);
                return ApiResponse<List<OrganizationDto>>.Success(new List<OrganizationDto> { dto }, Messages.Updated);
            });
        }

        public async Task<ApiResponse<List<string>>> DeleteOrganizationAsync(int id, CancellationToken cancellationToken = default)
        {
            var org = await _orgRepo.GetByIdAsync(id, cancellationToken);
            if (org == null) return ApiResponse<List<string>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                await _orgRepo.DeleteAsync(id, cancellationToken);

                var groups = await _context.Groups.Where(g => g.TenantId == id && !g.IsDeleted).ToListAsync(cancellationToken);
                foreach (var group in groups) { group.IsDeleted = true; group.DeletedAt = DateTime.UtcNow; _context.Groups.Update(group); }

                var users = await _context.Users.Where(u => u.TenantId == id && !u.IsDeleted).ToListAsync(cancellationToken);
                foreach (var user in users) { user.IsDeleted = true; user.DeletedAt = DateTime.UtcNow; _context.Users.Update(user); }

                var courses = await _context.Courses.Where(c => c.TenantId == id && !c.IsDeleted).ToListAsync(cancellationToken);
                foreach (var course in courses) { course.IsDeleted = true; course.DeletedAt = DateTime.UtcNow; _context.Courses.Update(course); }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return ApiResponse<List<string>>.Success(new List<string>(), Messages.Deleted);
            });
        }
    }
}
