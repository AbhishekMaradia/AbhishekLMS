using LMS_SoulCode.Features.Certificates.DTOs;
using LMS_SoulCode.Features.Certificates.Services;
using LMS_SoulCode.Features.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_SoulCode.Features.Certificates.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CertificatesController : BaseApiController
    {
        private readonly ICertificateService _certificateService;
        public CertificatesController(ICertificateService certificateService, ILogger<CertificatesController> logger) : base(logger) 
            => _certificateService = certificateService;

        [HttpPost("generate")]
        [BackOfficePermission(ModuleCodes.CERTIFICATE, PermissionCodes.CERTIFICATE_ADD)]
        public async Task<IActionResult> Generate([FromBody] CreateCertificateRequest req, CancellationToken cancellationToken)
        {
            var targetTenantId = CurrentTenantId ?? req.TenantId;
            var response = await _certificateService.GenerateCertificateAsync(req, targetTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("{id}/download")]
        [BackOfficePermission(ModuleCodes.CERTIFICATE, PermissionCodes.CERTIFICATE_VIEW)]
        public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
        {
            var fileResult = await _certificateService.GetPdfAsync(id, CurrentTenantId, cancellationToken);
            if (fileResult == null) return NotFound(ApiResponse<List<string>>.Fail("File not found", 404));
            return fileResult;
        }

        [HttpPost("{id}/revoke")]
        [BackOfficePermission(ModuleCodes.CERTIFICATE, PermissionCodes.CERTIFICATE_DELETE)]
        public async Task<IActionResult> Revoke(int id, CancellationToken cancellationToken)
        {
            var response = await _certificateService.RevokeCertificateAsync(id, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("Certificate/{userId}")]
        [BackOfficePermission(ModuleCodes.CERTIFICATE, PermissionCodes.CERTIFICATE_VIEW)]
        public async Task<IActionResult> GetByUserId(int userId, CancellationToken cancellationToken)
        {
            var response = await _certificateService.GetCertificatesByUserAsync(userId, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("list")]
        [BackOfficePermission(ModuleCodes.CERTIFICATE, PermissionCodes.CERTIFICATE_VIEW)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var response = await _certificateService.GetAllCertificatesAsync(CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("paginated")]
        [BackOfficePermission(ModuleCodes.CERTIFICATE, PermissionCodes.CERTIFICATE_VIEW)]
        public async Task<IActionResult> GetCertificates([FromQuery] CertificateListRequest request, CancellationToken cancellationToken)
        {
            var response = await _certificateService.GetCertificatesAsync(request, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }
    }
}
