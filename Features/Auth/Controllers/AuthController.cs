using Microsoft.AspNetCore.Mvc;
using LMS_SoulCode.Features.Auth.Models;
using LMS_SoulCode.Features.Auth.Services;
using LMS_SoulCode.Features.Auth.Validators;
using LMS_SoulCode.Features.Auth.DTOs;
using Microsoft.AspNetCore.Authorization;
using LMS_SoulCode.Features.Common;
using FluentValidation;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;

namespace LMS_SoulCode.Features.Auth.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;
        private readonly IValidator<LoginRequest> _loginValidator;
        private readonly IValidator<RegisterRequest> _registerValidator;
        private readonly IValidator<ForgotPasswordRequest> _forgotPasswordValidator;
        private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;

        public AuthController(
            IAuthService authService, 
            ILogger<AuthController> logger,
            IValidator<LoginRequest> loginValidator,
            IValidator<RegisterRequest> registerValidator,
            IValidator<ForgotPasswordRequest> forgotPasswordValidator,
            IValidator<ResetPasswordRequest> resetPasswordValidator) : base(logger)
        {
            _authService = authService;
            _loginValidator = loginValidator;
            _registerValidator = registerValidator;
            _forgotPasswordValidator = forgotPasswordValidator;
            _resetPasswordValidator = resetPasswordValidator;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var validation = await _loginValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)), StatusCodes.BadRequest));
            }

            var response = await _authService.LoginAsync(request, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            var validation = await _registerValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)), StatusCodes.BadRequest));
            }

            var response = await _authService.RegisterAsync(request, cancellationToken);
            return StatusCode(response.Code, response);
        }
        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
        {
            var validation = await _forgotPasswordValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)), StatusCodes.BadRequest));
            }

            var response = await _authService.ForgotPasswordAsync(request, cancellationToken);
            return StatusCode(response.Code, response);
        }
        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            var validation = await _resetPasswordValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)), StatusCodes.BadRequest));
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(ApiResponse<string>.Fail("Passwords do not match.", StatusCodes.BadRequest));
            }

            var response = await _authService.ResetPasswordAsync(request, cancellationToken);
            return StatusCode(response.Code, response);
        }
    }
}
