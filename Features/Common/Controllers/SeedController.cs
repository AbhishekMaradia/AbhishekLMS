using LMS_SoulCode.Features.Common.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace LMS_SoulCode.Features.Common.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly DatabaseSeeder _seeder;

        public SeedController(DatabaseSeeder seeder)
        {
            _seeder = seeder;
        }

        [HttpPost("sync-security")]
        public async Task<IActionResult> SyncSecurity()
        {
            await _seeder.SyncSecurityNodesAsync();
            return Ok(new { success = true, message = "Security Modules and Permissions synchronized via SP." });
        }
    }
}
