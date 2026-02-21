using LMS_SoulCode.Features.Groups.DTOs;
using LMS_SoulCode.Features.Groups.Models;
using LMS_SoulCode.Features.Common.Repositories;

namespace LMS_SoulCode.Features.Groups.Repositories
{
    public interface IGroupRepository : IBaseRepository<Group>
    {
        Task<IEnumerable<Group>> GetGroupsByTenantIdAsync(int? tenantId, CancellationToken cancellationToken = default);
        Task<(IEnumerable<GroupDto> Items, int TotalCount)> GetGroupsPagedAsync(string? searchTerm, int pageNumber, int pageSize, int? tenantId, CancellationToken cancellationToken);
        
        // Group Courses
        Task AddGroupCoursesAsync(IEnumerable<GroupCourse> groupCourses, CancellationToken cancellationToken = default);
        Task UpdateGroupCourseAsync(GroupCourse groupCourse, CancellationToken cancellationToken = default);
        Task UpdateGroupCoursesAsync(IEnumerable<GroupCourse> groupCourses, CancellationToken cancellationToken = default);
        Task<GroupCourse?> GetGroupCourseByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<GroupCourse>> GetGroupCoursesByGroupIdAsync(int groupId, CancellationToken cancellationToken = default);
        Task DeleteGroupCoursesByCourseIdAsync(int courseId, CancellationToken cancellationToken = default);
        Task DeleteGroupCoursesAsync(IEnumerable<GroupCourse> groupCourses, CancellationToken cancellationToken = default);
    }
}
