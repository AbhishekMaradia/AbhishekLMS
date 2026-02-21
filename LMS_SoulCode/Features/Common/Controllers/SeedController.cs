using LMS_SoulCode.Features.Common;
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

        [HttpPost("reset-db")]
        public async Task<IActionResult> ResetDatabase()
        {
            await _seeder.SeedAsync();
            return Ok(ApiResponse<List<string>>.Success(new List<string> { "Database reset and seeded successfully. Super Admin: admin@lms.com / Admin@123" }, "Success"));
        }
    }
}
