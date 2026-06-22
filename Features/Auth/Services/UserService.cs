using AutoMapper;
using LMS_SoulCode.Features.Auth.DTOs;
using LMS_SoulCode.Features.Auth.Repositories;
using LMS_SoulCode.Features.Auth.Services;
using LMS_SoulCode.Features.Auth.Models;
using AuthModel = LMS_SoulCode.Features.Auth.DTOs.LoginRequest;
using RegisterModel = LMS_SoulCode.Features.Auth.DTOs.RegisterRequest;
using ForgotPasswordModel = LMS_SoulCode.Features.Auth.DTOs.ForgotPasswordRequest;
using ResetPasswordModel = LMS_SoulCode.Features.Auth.DTOs.ResetPasswordRequest;
using LMS_SoulCode.Features.Course.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using LMS_SoulCode.Features.Common;
using StatusCodes =LMS_SoulCode.Features.Common.StatusCodes;

namespace LMS_SoulCode.Features.Auth.Services
{
    public interface IUserService
    {
       
         Task<PagedApiResponse<UserDto>> GetAllUserAsync(UserListRequest request, int? tenantId, CancellationToken cancellationToken);
         Task<ApiResponse<List<UserDto>>> GetUserByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default);
         Task<ApiResponse<List<UserDto>>> UpdateUserAsync(int id, UpdateUserRequest request, int? tenantId, int? currentUserId = null, CancellationToken cancellationToken = default);
         Task<ApiResponse<List<UserDto>>> CreateUserAsync(RegisterRequest request, int? tenantId = null, CancellationToken cancellationToken = default);
         Task<ApiResponse<List<UserDto>>> CreateAdminUserAsync(AdminCreateUserRequest request, int? tenantId = null, CancellationToken cancellationToken = default);
         Task<ApiResponse<List<string>>> DeleteUserAsync(int id, int? tenantId, int? currentUserId = null, CancellationToken cancellationToken = default);
         Task<ApiResponse<List<string>>> RestoreUserAsync(int id, int? tenantId, CancellationToken cancellationToken = default);
         Task<ApiResponse<UserDto>> GetAdminByTenantIdAsync(int tenantId, CancellationToken cancellationToken = default);

    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;
      
        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepo = userRepository;
            _mapper = mapper;
        }

        public async Task<PagedApiResponse<UserDto>> GetAllUserAsync(UserListRequest request, int? tenantId, CancellationToken cancellationToken)
        {
          // Security: Org Admin can only see their own tenant, SuperAdmin can specify or see all
          var targetTenantId = tenantId.HasValue 
              ? tenantId                  // Org Admin - force their own tenant
              : request.TenantId;         // SuperAdmin - use request or null
          
          var (users, totalCount) = await _userRepo.GetUsersAsync(request.SearchTerm, request.PageNumber, request.PageSize, targetTenantId, cancellationToken);
            
            return PagedApiResponse<UserDto>.Success(users, request.PageNumber, request.PageSize, totalCount, Messages.Success);
        }

        public async Task<ApiResponse<List<UserDto>>> GetUserByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepo.GetUserByIdAsync(id, tenantId, cancellationToken);
            if (user == null)
                return ApiResponse<List<UserDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            return ApiResponse<List<UserDto>>.Success(new List<UserDto> { user }, Messages.Success);
        }

        public async Task<ApiResponse<List<UserDto>>> UpdateUserAsync(int id, UpdateUserRequest request, int? tenantId, int? currentUserId = null, CancellationToken cancellationToken = default)
        {
            // Security: Prevent self-deactivation (especially for Super Admin)
            if (currentUserId.HasValue && id == currentUserId.Value && request.IsActive.HasValue && !request.IsActive.Value)
            {
                 return ApiResponse<List<UserDto>>.Fail(Messages.SelfDeactivationError, StatusCodes.BadRequest);
            }

            if (request.GroupIds == null && request.GroupId.HasValue)
            {
                request.GroupIds = new List<int> { request.GroupId.Value };
            }

            var user = await _userRepo.UpdateUserAsync(id, request, tenantId, cancellationToken);
            if (user == null)
                return ApiResponse<List<UserDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            return ApiResponse<List<UserDto>>.Success(new List<UserDto> { user }, Messages.Updated);
        }

        public async Task<ApiResponse<List<UserDto>>> CreateUserAsync(RegisterRequest request, int? tenantId = null, CancellationToken cancellationToken = default)
        {
            if (await _userRepo.IsEmailTakenAsync(request.Email, tenantId, cancellationToken))
                return ApiResponse<List<UserDto>>.Fail(Messages.AlreadyExists, StatusCodes.BadRequest);

            if (await _userRepo.IsMobileTakenAsync(request.Mobile, tenantId, cancellationToken))
                return ApiResponse<List<UserDto>>.Fail(Messages.MobileExists, StatusCodes.BadRequest);

            var user = _mapper.Map<User>(request);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            user.TenantId = tenantId;

            await _userRepo.AddAsync(user, cancellationToken);

            return await GetUserByIdAsync(user.Id, tenantId, cancellationToken);
        }

        public async Task<ApiResponse<List<UserDto>>> CreateAdminUserAsync(AdminCreateUserRequest request, int? tenantId = null, CancellationToken cancellationToken = default)
        {
            if (await _userRepo.IsEmailTakenAsync(request.Email, tenantId, cancellationToken))
                return ApiResponse<List<UserDto>>.Fail(Messages.AlreadyExists, StatusCodes.BadRequest);

            if (await _userRepo.IsMobileTakenAsync(request.Mobile, tenantId, cancellationToken))
                return ApiResponse<List<UserDto>>.Fail(Messages.MobileExists, StatusCodes.BadRequest);

            var user = _mapper.Map<User>(request);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            user.TenantId = tenantId;

            if ((request.GroupIds == null || !request.GroupIds.Any()) && request.GroupId.HasValue)
            {
                request.GroupIds = new List<int> { request.GroupId.Value };
            }

            var success = await _userRepo.AddUserWithSecurityAsync(user, request.RoleIds, request.GroupIds, request.IsActive, cancellationToken);
            if (!success)
                return ApiResponse<List<UserDto>>.Fail("Failed to create user with security roles.", StatusCodes.ServerError);

            return await GetUserByIdAsync(user.Id, tenantId, cancellationToken);
        }

        public async Task<ApiResponse<List<string>>> DeleteUserAsync(int id, int? tenantId, int? currentUserId = null, CancellationToken cancellationToken = default)
        {
            // Security: Prevent self-deletion
            if (currentUserId.HasValue && id == currentUserId.Value)
            {
                return ApiResponse<List<string>>.Fail(Messages.SelfDeletionError, StatusCodes.BadRequest);
            }

            var user = await _userRepo.GetUserByIdAsync(id, tenantId, cancellationToken);
            if (user == null)
                return ApiResponse<List<string>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            // Strict Validation: Cannot delete Org Admin UNLESS you are a SuperAdmin
            if (tenantId.HasValue && await _userRepo.IsUserOrgAdminAsync(id, cancellationToken))
            {
                return ApiResponse<List<string>>.Fail(Messages.DeleteBlockedUserAdmin, StatusCodes.BadRequest);
            }

            var success = await _userRepo.SoftDeleteUserAsync(id, tenantId, cancellationToken);
            if (!success)
                return ApiResponse<List<string>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            return ApiResponse<List<string>>.Success(null, Messages.Deleted);
        }

        public async Task<ApiResponse<List<string>>> RestoreUserAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var success = await _userRepo.RestoreUserAsync(id, tenantId, cancellationToken);
            if (!success)
                return ApiResponse<List<string>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            return ApiResponse<List<string>>.Success(null, Messages.Success);
        }

        public async Task<ApiResponse<UserDto>> GetAdminByTenantIdAsync(int tenantId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepo.GetAdminUserByTenantIdAsync(tenantId, cancellationToken);
            if (user == null)
            {
                // Note: We return 404 if no admin is found, which is handled by the frontend to show empty form
                return ApiResponse<UserDto>.Fail(Messages.NotFound, StatusCodes.NotFound);
            }

            var userDto = _mapper.Map<UserDto>(user);
            return ApiResponse<UserDto>.Success(userDto, Messages.Success);
        }
    }
}
