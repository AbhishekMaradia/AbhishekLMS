using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.Organizations.DTOs;
using LMS_SoulCode.Features.Organizations.Services;
using LMS_SoulCode.Features.Auth.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LMS_SoulCode.Features.Organizations.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganizationController : BaseApiController
    {
        private readonly IOrganizationService _orgService;

        public OrganizationController(IOrganizationService orgService, ILogger<OrganizationController> logger) : base(logger)
        {
            _orgService = orgService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] OrgRegisterRequest request, CancellationToken cancellationToken)
        {
            var result = await _orgService.RegisterOrganizationAsync(request, cancellationToken);
            return StatusCode(result.Code, result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _orgService.OrgLoginAsync(request, cancellationToken);
            return StatusCode(result.Code, result);
        }

        [HttpPost]
        [BackOfficePermission(ModuleCodes.ORGANIZATION, PermissionCodes.ORGANIZATION_ADD)]
        public async Task<IActionResult> CreateOrg([FromForm] OrgRegisterRequest request, CancellationToken cancellationToken)
        {
            var result = await _orgService.RegisterOrganizationAsync(request, cancellationToken);
            return StatusCode(result.Code, result);
        }

        [HttpGet]
        [BackOfficePermission(ModuleCodes.ORGANIZATION, PermissionCodes.ORGANIZATION_VIEW)]
        public async Task<IActionResult> GetAllOrg([FromQuery] OrganizationListRequest request, CancellationToken cancellationToken)
        {
            var result = await _orgService.GetAllOrganizationsAsync(request, cancellationToken);
            return StatusCode(result.Code, result);
        }

        [HttpGet("{id}")]
        [BackOfficePermission(ModuleCodes.ORGANIZATION, PermissionCodes.ORGANIZATION_VIEW)]
        public async Task<IActionResult> GetOrgById(int id, CancellationToken cancellationToken)
        {
            var result = await _orgService.GetOrganizationByIdAsync(id, CurrentTenantId, cancellationToken);
            return StatusCode(result.Code, result);
        }

        [HttpPut("{id}")]
        [BackOfficePermission(ModuleCodes.ORGANIZATION, PermissionCodes.ORGANIZATION_EDIT)]
        public async Task<IActionResult> UpdateOrg(int id, [FromForm] UpdateOrganizationRequest request, CancellationToken cancellationToken)
        {
            var result = await _orgService.UpdateOrganizationAsync(id, request, CurrentTenantId, cancellationToken);
            return StatusCode(result.Code, result);
        }

        [HttpDelete("{id}")]
        [BackOfficePermission(ModuleCodes.ORGANIZATION, PermissionCodes.ORGANIZATION_DELETE)]
        public async Task<IActionResult> DeleteOrg(int id, CancellationToken cancellationToken)
        {
            var result = await _orgService.DeleteOrganizationAsync(id, cancellationToken);
            return StatusCode(result.Code, result);
        }

        [HttpPut("profile")]
        [BackOfficePermission(ModuleCodes.ORGANIZATION, PermissionCodes.ORGANIZATION_EDIT)]
        public async Task<IActionResult> UpdateOrgProfile([FromForm] UpdateOrganizationRequest request, CancellationToken cancellationToken)
        {
            if (!CurrentTenantId.HasValue) return Unauthorized(ApiResponse<string>.Fail("Tenant not found in token", 401));

            var result = await _orgService.UpdateOrganizationProfileAsync(CurrentTenantId.Value, request, CurrentTenantId, CurrentUserId, cancellationToken);
            return StatusCode(result.Code, result);
        }
    }
}
