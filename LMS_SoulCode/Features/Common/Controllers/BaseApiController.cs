using Microsoft.AspNetCore.Mvc;

namespace LMS_SoulCode.Features.Common
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        protected readonly ILogger _logger;

        protected BaseApiController(ILogger logger)
        {
            _logger = logger;
        }

        protected int? CurrentTenantId => User.GetTenantId();
        protected int? CurrentUserId => User.GetUserId();

    }
}