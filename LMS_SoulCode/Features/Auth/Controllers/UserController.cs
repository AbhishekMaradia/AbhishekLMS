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

namespace LMS_SoulCode.Features.Auth.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IValidator<RegisterRequest> _registerValidator;

        public UserController(IUserService userService, IValidator<RegisterRequest> registerValidator, ILogger<UserController> logger) : base(logger)
        {
            _userService = userService;
            _registerValidator = registerValidator;
        }


        [HttpGet("userlist")]
        [BackOfficePermission(ModuleCodes.USER, PermissionCodes.USER_VIEW)]
        public async Task<IActionResult> GetUserList([FromQuery] UserListRequest request, CancellationToken cancellationToken)
        {
            var response = await _userService.GetAllUserAsync(request, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("{id:int}")]
        [BackOfficePermission(ModuleCodes.USER, PermissionCodes.USER_VIEW)]
        public async Task<IActionResult> GetUserById(int id, CancellationToken cancellationToken)
        {
            var response = await _userService.GetUserByIdAsync(id, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPost("create")]
        [BackOfficePermission(ModuleCodes.USER, PermissionCodes.USER_ADD)]
        public async Task<IActionResult> CreateUser([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            var validation = await _registerValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)), LMS_SoulCode.Features.Common.StatusCodes.BadRequest));
            }
            var targetTenantId = CurrentTenantId ?? request.TenantId;
            var response = await _userService.CreateUserAsync(request, targetTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPut("{id:int}")]
        [BackOfficePermission(ModuleCodes.USER, PermissionCodes.USER_EDIT)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
        {
            var response = await _userService.UpdateUserAsync(id, request, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpDelete("{id:int}")]
        [BackOfficePermission(ModuleCodes.USER, PermissionCodes.USER_DELETE)]
        public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
        {
            var response = await _userService.DeleteUserAsync(id, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPost("{id:int}/restore")]
        [BackOfficePermission(ModuleCodes.USER, PermissionCodes.USER_EDIT)]
        public async Task<IActionResult> RestoreUser(int id, CancellationToken cancellationToken)
        {
            var response = await _userService.RestoreUserAsync(id, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }
    }
}
