using LMS_SoulCode.Features.Reports.Services;
using LMS_SoulCode.Features.Reports.DTOs;
using LMS_SoulCode.Features.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_SoulCode.Features.Reports.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : BaseApiController
    {
        private readonly CourseReportService _report;

        public ReportsController(CourseReportService report, ILogger<ReportsController> logger) : base(logger)
        {
            _report = report;
        }

        [HttpGet("course/{userId}/{courseId}")]
        [BackOfficePermission(ModuleCodes.REPORT, PermissionCodes.REPORT_VIEW, PermissionCodes.REPORT_GENERATE)]
        public async Task<IActionResult> GetReport(int userId, int courseId, CancellationToken cancellationToken)
        {       
           var response = await _report.GetUserCourseReport(userId, courseId, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("list")]
        [BackOfficePermission(ModuleCodes.REPORT, PermissionCodes.REPORT_VIEW)]
        public async Task<IActionResult> GetReports([FromQuery] ReportListRequest request, CancellationToken cancellationToken)
        {
            var response = await _report.GetReportsAsync(request, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }
    }
}
