using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.Groups.DTOs;

namespace LMS_SoulCode.Features.Groups.Services
{
    public interface IGroupService
    {
        Task<ApiResponse<List<GroupDto>>> CreateGroupAsync(CreateGroupRequest request, int? tenantId, CancellationToken cancellationToken = default);
        Task<PagedApiResponse<GroupDto>> GetGroupsAsync(GroupListRequest request, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<GroupDto>>> GetGroupByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<string>>> DeleteGroupAsync(int id, int? tenantId, CancellationToken cancellationToken = default);
        Task RemoveCourseFromAllGroupsAsync(int courseId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<GroupDto>>> UpdateGroupAsync(int id, UpdateGroupRequest request, int? tenantId, CancellationToken cancellationToken = default);
        Task<PagedApiResponse<GroupCourseDto>> GetGroupCoursesByGroupIdAsync(int groupId, GroupCourseListRequest request, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> BulkUpdateGroupCoursesAsync(BulkUpdateCoursesRequest request, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<GroupUserDto>>> GetGroupUsersAsync(int groupId, int? tenantId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Bulk assign users to a group with organization validation.
        /// IMPORTANT: Only users from the same organization as the group can be assigned.
        /// </summary>
        /// <param name="request">Assignment request containing GroupId and UserIds</param>
        /// <param name="tenantId">The organization ID (null for superadmin, populated for org admin)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>API Response with success or error message about organization mismatch</returns>
        Task<ApiResponse<string>> BulkAssignUsersAsync(BulkAssignUsersRequest request, int? tenantId, CancellationToken cancellationToken = default);

    }
}
