using LMS_SoulCode.Features.Organizations.DTOs;
using LMS_SoulCode.Features.Auth.DTOs;

namespace LMS_SoulCode.Features.Organizations.Services
{
    public interface IOrganizationService
    {
        Task<ApiResponse<List<string>>> RegisterOrganizationAsync(OrgRegisterRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<LoginResponse>>> OrgLoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
        
        // CRUD for SuperAdmin
        Task<PagedApiResponse<OrganizationDto>> GetAllOrganizationsAsync(OrganizationListRequest request, bool isSuperAdmin, int? currentTenantId, CancellationToken cancellationToken);
        Task<ApiResponse<List<OrganizationDto>>> GetOrganizationByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<OrganizationDto>>> UpdateOrganizationAsync(int id, UpdateOrganizationRequest request, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<string>>> DeleteOrganizationAsync(int id, bool isSuperAdmin, CancellationToken cancellationToken = default);

        // For Organization Admin
        Task<ApiResponse<List<OrganizationDto>>> UpdateOrganizationProfileAsync(int id, UpdateOrganizationRequest request, int? tenantId, int? userId, CancellationToken cancellationToken = default);
        
        Task<ApiResponse<UserDto>> GetOrganizationAdminAsync(int tenantId, CancellationToken cancellationToken = default);
    }
}
