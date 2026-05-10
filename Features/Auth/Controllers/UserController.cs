using Microsoft.AspNetCore.Mvc;
using LMS_SoulCode.Features.Auth.Models;
using LMS_SoulCode.Features.Auth.Services;
using LMS_SoulCode.Features.Auth.Validators;
using FluentValidation;
using LMS_SoulCode.Features.Common;
using AuthModel = LMS_SoulCode.Features.Auth.DTOs.LoginRequest;
using RegisterModel = LMS_SoulCode.Features.Auth.DTOs.RegisterRequest;
using ForgotPasswordModel = LMS_SoulCode.Features.Auth.DTOs.ForgotPasswordRequest;
using ResetPasswordModel = LMS_SoulCode.Features.Auth.DTOs.ResetPasswordRequest;
using Microsoft.AspNetCore.Authorization;
using LMS_SoulCode.Features.Auth.DTOs;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;

namespace LMS_SoulCode.Features.Auth.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IValidator<RegisterRequest> _registerValidator;
        private readonly IValidator<AdminCreateUserRequest> _adminCreateValidator;

        public UserController(IUserService userService, 
            IValidator<RegisterRequest> registerValidator, 
            IValidator<AdminCreateUserRequest> adminCreateValidator,
            ILogger<UserController> logger) : base(logger)
        {
            _userService = userService;
            _registerValidator = registerValidator;
            _adminCreateValidator = adminCreateValidator;
        }


        [HttpGet("userlist")]
        [BackOfficePermission(ModuleCodes.USER, PermissionCodes.USER_VIEW)]
        public async Task<IActionResult> GetUserList([FromQuery] UserListRequest request, CancellationToken cancellationToken)
        {
            var response = await _userService.GetAllUserAsync(request, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("{id:int}")]
        [BackOfficePermission(ModuleCodes.USER, PermissionCodes.USER_VIEW, PermissionCodes.USER_EDIT)]
        public async Task<IActionResult> GetUserById(int id, CancellationToken cancellationToken)
        {
            var response = await _userService.GetUserByIdAsync(id, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPost("create")]
        [BackOfficePermission(ModuleCodes.USER, PermissionCodes.USER_ADD)]
        public async Task<IActionResult> CreateUser([FromBody] AdminCreateUserRequest request, CancellationToken cancellationToken)
        {
            var validation = await _adminCreateValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)), StatusCodes.BadRequest));
            }

            var targetTenantId = (CurrentTenantId == null || CurrentTenantId == 0) ? request.TenantId : CurrentTenantId;
            var response = await _userService.CreateAdminUserAsync(request, targetTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPut("{id:int}")]
        [BackOfficePermission(ModuleCodes.USER, PermissionCodes.USER_EDIT)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
        {
            var response = await _userService.UpdateUserAsync(id, request, CurrentTenantId, CurrentUserId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpDelete("{id:int}")]
        [BackOfficePermission(ModuleCodes.USER, PermissionCodes.USER_DELETE)]
        public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
        {
            var response = await _userService.DeleteUserAsync(id, CurrentTenantId, CurrentUserId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPost("{id:int}/restore")]
        [BackOfficePermission(ModuleCodes.USER, PermissionCodes.USER_EDIT)]
        public async Task<IActionResult> RestoreUser(int id, CancellationToken cancellationToken)
        {
            var response = await _userService.RestoreUserAsync(id, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("admin/{tenantId:int}")]
        [BackOfficePermission(ModuleCodes.ORGANIZATION, PermissionCodes.ORGANIZATION_EDIT)]
        public async Task<IActionResult> GetAdminByTenantId(int tenantId, CancellationToken cancellationToken)
        {
            var response = await _userService.GetAdminByTenantIdAsync(tenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }
    }
}
