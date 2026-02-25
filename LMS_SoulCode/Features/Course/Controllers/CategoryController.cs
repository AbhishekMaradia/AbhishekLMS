using LMS_SoulCode.Features.Course.DTOs;
using LMS_SoulCode.Features.Course.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMS_SoulCode.Features.Common;

namespace LMS_SoulCode.Features.Course.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : BaseApiController
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger) : base(logger)
            => _categoryService = categoryService;

        [HttpGet("list")]
        [BackOfficePermission(ModuleCodes.CATEGORY, PermissionCodes.CATEGORY_VIEW)]
        public async Task<IActionResult> GetAll([FromQuery] CategoryListRequest request, CancellationToken cancellationToken)
        {
            var response = await _categoryService.GetAllAsync(request, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("{id}")]
        [BackOfficePermission(ModuleCodes.CATEGORY, PermissionCodes.CATEGORY_VIEW, PermissionCodes.CATEGORY_EDIT)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var response = await _categoryService.GetByIdAsync(id, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPost("create")]
        [BackOfficePermission(ModuleCodes.CATEGORY, PermissionCodes.CATEGORY_ADD)]
        public async Task<IActionResult> Create([FromBody] CategoryRequest request, CancellationToken cancellationToken)
        {
            var targetTenantId = CurrentTenantId ?? request.TenantId;
            var response = await _categoryService.CreateAsync(request.CategoryName, targetTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPut("update/{id}")]
        [BackOfficePermission(ModuleCodes.CATEGORY, PermissionCodes.CATEGORY_EDIT)]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryRequest request, CancellationToken cancellationToken)
        {
            var response = await _categoryService.UpdateAsync(id, request.CategoryName, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpDelete("delete/{id}")]
        [BackOfficePermission(ModuleCodes.CATEGORY, PermissionCodes.CATEGORY_DELETE)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var response = await _categoryService.DeleteAsync(id, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }
    }
}
