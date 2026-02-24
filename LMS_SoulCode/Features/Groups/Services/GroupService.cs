using LMS_SoulCode.Features.Groups.Models;
using LMS_SoulCode.Features.Groups.DTOs;
using LMS_SoulCode.Features.Groups.Repositories;
using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Data; // For LmsDbContext if needed, or via Repository
using LMS_SoulCode.Features.Common.Models;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;

namespace LMS_SoulCode.Features.Groups.Services
{
    using AutoMapper;
    using FluentValidation;
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _groupRepo;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateGroupRequest> _validator;
        private readonly LMS_SoulCode.Features.Course.Repositories.ICourseRepository _courseRepo;

        public GroupService(IGroupRepository groupRepo, IMapper mapper, IValidator<CreateGroupRequest> validator,
            LMS_SoulCode.Features.Course.Repositories.ICourseRepository courseRepo)
        {
            _groupRepo = groupRepo;
            _mapper = mapper;
            _validator = validator;
            _courseRepo = courseRepo;
        }

        public async Task<ApiResponse<List<GroupDto>>> CreateGroupAsync(CreateGroupRequest request, int? tenantId, CancellationToken cancellationToken = default)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
                return ApiResponse<List<GroupDto>>.Fail(string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)), StatusCodes.BadRequest);

            var group = _mapper.Map<Group>(request);
            group.TenantId = tenantId;
            
            await _groupRepo.AddAsync(group, cancellationToken);
            
            var dto = _mapper.Map<GroupDto>(group);

            return ApiResponse<List<GroupDto>>.Success(new List<GroupDto> { dto }, Messages.Created);
        }

        public async Task<PagedApiResponse<GroupDto>> GetGroupsAsync(GroupListRequest request, int? tenantId, CancellationToken cancellationToken = default)
        {
            var (items, totalCount) = await _groupRepo.GetGroupsPagedAsync(request.SearchTerm, request.PageNumber, request.PageSize, tenantId, cancellationToken);
            return PagedApiResponse<GroupDto>.Success(items, request.PageNumber, request.PageSize, totalCount, Messages.Success);
        }

        public async Task<ApiResponse<List<GroupDto>>> GetGroupByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var group = await _groupRepo.GetByIdAsync(id, cancellationToken);
            if (group == null) return ApiResponse<List<GroupDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            if (tenantId.HasValue && group.TenantId != tenantId.Value)
                 return ApiResponse<List<GroupDto>>.Fail(Messages.Forbidden, StatusCodes.Forbidden);

            // Sync courses before returning
            await SyncCoursesAsync(group, cancellationToken);

            var dto = _mapper.Map<GroupDto>(group);
            return ApiResponse<List<GroupDto>>.Success(new List<GroupDto> { dto }, Messages.Success);
        }

        private async Task SyncCoursesAsync(Group group, CancellationToken ct)
        {
            var courses = await _courseRepo.GetAllActiveCoursesAsync(group.TenantId, ct);
            var newLinks = courses.Where(c => group.GroupCourses.All(gc => gc.CourseId != c.Id))
                .Select(c => new GroupCourse { GroupId = group.Id, CourseId = c.Id, TenantId = group.TenantId, Course = c })
                .ToList();

            if (newLinks.Count == 0) return;
            await _groupRepo.AddGroupCoursesAsync(newLinks, ct);
            newLinks.ForEach(group.GroupCourses.Add);
        }

        public async Task<ApiResponse<List<string>>> DeleteGroupAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var group = await _groupRepo.GetByIdAsync(id, cancellationToken);
            if (group == null) return ApiResponse<List<string>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            if (tenantId.HasValue && group.TenantId != tenantId.Value)
                 return ApiResponse<List<string>>.Fail(Messages.Forbidden, StatusCodes.Forbidden);

            // Also delete all course assignments for this group
            if (group.GroupCourses != null && group.GroupCourses.Any())
            {
                await _groupRepo.DeleteGroupCoursesAsync(group.GroupCourses, cancellationToken);
            }

            await _groupRepo.DeleteAsync(id, cancellationToken);
            return ApiResponse<List<string>>.Success(new List<string>(), Messages.Deleted);
        }

        public async Task RemoveCourseFromAllGroupsAsync(int courseId, CancellationToken cancellationToken = default)
        {
            await _groupRepo.DeleteGroupCoursesByCourseIdAsync(courseId, cancellationToken);
        }

        public async Task<ApiResponse<List<GroupDto>>> UpdateGroupAsync(int id, UpdateGroupRequest request, int? tenantId, CancellationToken cancellationToken = default)
        {
            var group = await _groupRepo.GetByIdAsync(id, cancellationToken);
            if (group == null) return ApiResponse<List<GroupDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            if (tenantId.HasValue && group.TenantId != tenantId.Value)
                  return ApiResponse<List<GroupDto>>.Fail(Messages.Forbidden, StatusCodes.Forbidden);

            if (!string.IsNullOrEmpty(request.GroupName))
            {
                group.GroupName = request.GroupName;
            }

            // Courses are now handled exclusively by BulkUpdateGroupCoursesAsync for better clarity
            
            await _groupRepo.UpdateAsync(group, cancellationToken); 

            var updatedDto = _mapper.Map<GroupDto>(group);
            return ApiResponse<List<GroupDto>>.Success(new List<GroupDto> { updatedDto }, Messages.Updated);
        }

        public async Task<PagedApiResponse<GroupCourseDto>> GetGroupCoursesByGroupIdAsync(int groupId, GroupCourseListRequest request, int? tenantId, CancellationToken cancellationToken = default)
        {
            var group = await _groupRepo.GetByIdAsync(groupId, cancellationToken);
            if (group == null) return PagedApiResponse<GroupCourseDto>.Fail(Messages.NotFound, StatusCodes.NotFound);

            if (tenantId.HasValue && group.TenantId != tenantId.Value)
                 return PagedApiResponse<GroupCourseDto>.Fail(Messages.Forbidden, StatusCodes.Forbidden);

            // Sync courses before returning
            await SyncCoursesAsync(group, cancellationToken);

            var (items, totalCount) = await _groupRepo.GetGroupCoursesPagedByGroupIdAsync(groupId, tenantId, request.SearchTerm, request.PageNumber, request.PageSize, cancellationToken);
            var dtos = _mapper.Map<List<GroupCourseDto>>(items);

            return PagedApiResponse<GroupCourseDto>.Success(dtos, request.PageNumber, request.PageSize, totalCount, Messages.Success);
        }



        public async Task<ApiResponse<string>> BulkUpdateGroupCoursesAsync(BulkUpdateCoursesRequest request, int? tenantId, CancellationToken cancellationToken = default)
        {
            var group = await _groupRepo.GetByIdAsync(request.GroupId, cancellationToken);
            if (group == null) return ApiResponse<string>.Fail(Messages.NotFound, StatusCodes.NotFound);

            if (tenantId.HasValue && group.TenantId != tenantId.Value)
                 return ApiResponse<string>.Fail(Messages.Forbidden, StatusCodes.Forbidden);

            if (request.Courses == null || !request.Courses.Any())
                return ApiResponse<string>.Fail("No courses provided for update.", StatusCodes.BadRequest);

            var currentCourses = group.GroupCourses.ToList();
            var coursesToAdd = new List<GroupCourse>();
            var coursesToUpdate = new List<GroupCourse>();

            foreach (var item in request.Courses)
            {
                if (item.CourseId <= 0) continue;

                var existingLink = currentCourses.FirstOrDefault(gc => gc.CourseId == item.CourseId);

                if (existingLink != null)
                {
                    // Update existing flag
                    if (existingLink.IsEnable != item.IsEnable)
                    {
                        existingLink.IsEnable = item.IsEnable;
                        existingLink.UpdatedAt = DateTime.UtcNow;
                        coursesToUpdate.Add(existingLink);
                    }
                }
                else if (item.IsEnable)
                {
                    // Add if missing (safety check for old data)
                    coursesToAdd.Add(new GroupCourse
                    {
                        GroupId = group.Id,
                        CourseId = item.CourseId,
                        TenantId = group.TenantId,
                        IsEnable = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            if (coursesToAdd.Any()) await _groupRepo.AddGroupCoursesAsync(coursesToAdd, cancellationToken);
            if (coursesToUpdate.Any()) await _groupRepo.UpdateGroupCoursesAsync(coursesToUpdate, cancellationToken);

            return ApiResponse<string>.Success(string.Empty, "Courses updated successfully in bulk.");
        }
    }
}
