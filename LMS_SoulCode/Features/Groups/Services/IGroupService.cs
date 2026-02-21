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
        Task<ApiResponse<List<GroupCourseDto>>> GetGroupCoursesForEditAsync(int groupId, int? tenantId, CancellationToken cancellationToken = default);
    }
}
