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
            // 1. Get Group details
            var group = await _groupRepo.GetByIdAsync(id, cancellationToken);
            if (group == null) return ApiResponse<List<GroupDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            if (tenantId.HasValue && group.TenantId != tenantId.Value)
                 return ApiResponse<List<GroupDto>>.Fail(Messages.Forbidden, StatusCodes.Forbidden);

            // 2. Get All Active Courses to show full list (Enabled + Disabled)
            var allCourses = await _courseRepo.GetAllActiveCoursesAsync(tenantId, cancellationToken);
            
            var existingGroupCourses = group.GroupCourses ?? new List<GroupCourse>();
            var existingMap = existingGroupCourses.ToDictionary(gc => gc.CourseId);

            var mergedCourses = new List<GroupCourseDto>();
            foreach (var course in allCourses)
            {
                var isAssigned = existingMap.TryGetValue(course.Id, out var gc);
                
                mergedCourses.Add(new GroupCourseDto
                {
                    Id = isAssigned ? gc!.Id : 0, 
                    GroupId = id,
                    CourseId = course.Id,
                    CourseName = course.Title,
                    IsEnable = isAssigned,
                    CreatedAt = isAssigned ? gc!.CreatedAt : DateTime.MinValue,
                    UpdatedAt = isAssigned ? gc!.UpdatedAt : DateTime.MinValue
                });
            }

            // 3. Prepare DTO
            var dto = _mapper.Map<GroupDto>(group);
            dto.GroupCourses = mergedCourses; // Override with full list

            return ApiResponse<List<GroupDto>>.Success(new List<GroupDto> { dto }, Messages.Success);
        }

        public async Task<ApiResponse<List<string>>> DeleteGroupAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var group = await _groupRepo.GetByIdAsync(id, cancellationToken);
            if (group == null) return ApiResponse<List<string>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            if (tenantId.HasValue && group.TenantId != tenantId.Value)
                 return ApiResponse<List<string>>.Fail(Messages.Forbidden, StatusCodes.Forbidden);

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

            // Update Name
            if (!string.IsNullOrEmpty(request.GroupName))
            {
                group.GroupName = request.GroupName;
            }

            // Update Courses
            if (request.Courses != null && request.Courses.Any())
            {
                // Ensure courses are loaded (GetByIdAsync override includes them)
                var currentCourses = group.GroupCourses.ToList(); 
                
                var coursesToAdd = new List<GroupCourse>();
                var coursesToRemove = new List<GroupCourse>();

                foreach (var item in request.Courses)
                {
                    // Skip invalid course IDs (e.g. 0 default from Swagger/Frontend)
                    if (item.CourseId <= 0) continue;

                    var existingLink = currentCourses.FirstOrDefault(gc => gc.CourseId == item.CourseId);

                    if (item.IsEnable)
                    {
                        if (existingLink == null)
                        {
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
                    else
                    {
                        if (existingLink != null)
                        {
                             coursesToRemove.Add(existingLink);
                        }
                    }
                }

                if (coursesToAdd.Any())
                {
                    await _groupRepo.AddGroupCoursesAsync(coursesToAdd, cancellationToken);
                }

                if (coursesToRemove.Any())
                {
                    await _groupRepo.DeleteGroupCoursesAsync(coursesToRemove, cancellationToken);
                }
            }
            
            await _groupRepo.UpdateAsync(group, cancellationToken); 

            // Return updated structure (Full list) via GetGroupById logic
            // Use internal logic or call GetGroupByIdAsync if needed, but here we can just replicate or call it.
            // Calling GetGroupByIdAsync directly:
            var getResponse = await GetGroupByIdAsync(id, tenantId, cancellationToken);
            var updatedDtoList = getResponse.Data;

            return ApiResponse<List<GroupDto>>.Success(updatedDtoList, Messages.Updated);
        }

        public async Task<ApiResponse<List<GroupCourseDto>>> GetGroupCoursesForEditAsync(int groupId, int? tenantId, CancellationToken cancellationToken = default)
        {
            // 1. Get All Active Courses
            var allCourses = await _courseRepo.GetAllActiveCoursesAsync(tenantId, cancellationToken);

            // 2. Get Group's Current Courses
            var group = await _groupRepo.GetByIdAsync(groupId, cancellationToken);
            if (group == null) return ApiResponse<List<GroupCourseDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            if (tenantId.HasValue && group.TenantId != tenantId.Value)
                 return ApiResponse<List<GroupCourseDto>>.Fail(Messages.Forbidden, StatusCodes.Forbidden);

            var existingGroupCourses = group.GroupCourses ?? new List<GroupCourse>();
            var existingMap = existingGroupCourses.ToDictionary(gc => gc.CourseId);

            // 3. Merge
            var result = new List<GroupCourseDto>();
            foreach (var course in allCourses)
            {
                var isAssigned = existingMap.TryGetValue(course.Id, out var gc);
                
                result.Add(new GroupCourseDto
                {
                    Id = isAssigned ? gc!.Id : 0, // 0 if not assigned
                    GroupId = groupId,
                    CourseId = course.Id,
                    CourseName = course.Title,
                    IsEnable = isAssigned, // If it exists in DB, it's enabled (based on new logic)
                    CreatedAt = isAssigned ? gc!.CreatedAt : DateTime.MinValue,
                    UpdatedAt = isAssigned ? gc!.UpdatedAt : DateTime.MinValue
                });
            }

            return ApiResponse<List<GroupCourseDto>>.Success(result, Messages.Success);
        }
    }
}
