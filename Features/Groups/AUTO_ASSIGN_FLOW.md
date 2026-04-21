# Auto-Assignment (Draft Mode) Flow

## Overview
This flow ensures that whenever a **New Course** is created, it is automatically linked to **All Existing Groups** within the same organization (Tenant). However, by default, its status is set to `Disabled`. 

This is like a "Draft" assignment—the course is "available" to be enabled for any group, but it won't be visible to students until an administrator manually toggles it.

---

## 1. Trigger: New Course Creation
When an admin fills out the "Create Course" form and clicks "Save".

**Action:** `POST /api/Course/create`

**Backend Logic:**
1. Save the new Course to the `Courses` table.
2. Fetch all active **Groups** belonging to the `CurrentTenantId`.
3. For each group, create a record in the `GroupCourse` table:
   - `GroupId`: [Existing Group Id]
   - `CourseId`: [New Course Id]
   - `IsEnable`: `false` (Disabled by default)
   - `TenantId`: `CurrentTenantId`

---

## 2. Admin Toggles Visibility (Later)
The admin later decides which groups should see this new course.

**Workflow:**
1. Admin goes to **Groups** module.
2. Admin selects a specific Group (e.g., "Engineering Team").
3. Admin sees the new course in the list (it will be unchecked).
4. Admin ticks the checkbox and clicks "Save".

**API Call:** `PUT /api/Groups/bulk-update-courses` (Existing API)

---

## 3. Key Benefits

- **Zero Manual Setup**: Admin doesn't need to "search and add" the course into groups one by one. It's already there in the list waiting to be enabled.
- **Consistent Lists**: Every group always sees the full catalog of courses in their "Admin/Edit" view, maintaining consistency across the organization.
- **Controlled Rollout**: You can create a course today, and slowly enable it for different departments over the week.

---

## 4. Implementation Details (Backend)

To implement this, we need to modify the `CourseService`:

```csharp
// Inside CourseService.cs -> AddAsync method
public async Task<ApiResponse<List<CourseResponse>>> AddAsync(CourseRequest request, int? tenantId...)
{
    // 1. Create Course
    var course = await _courseRepo.AddAsync(courseEntity);

    // 2. AUTO-ADD TO GROUPS (Idea 2 Logic)
    var allGroups = await _groupRepo.GetGroupsByTenantIdAsync(tenantId);
    var groupCourseDrafts = allGroups.Select(g => new GroupCourse 
    {
        GroupId = g.Id,
        CourseId = course.Id,
        IsEnable = false, // Always disabled initially
        TenantId = tenantId
    });
    
    await _groupRepo.AddGroupCoursesAsync(groupCourseDrafts);
    
    return Success;
}
```

---

## 5. UI Expectation

- **Course Module**: Remains simple (standard form).
- **Groups Module**: Whenever you open a group's course list, **all** courses of that organization will appear. Some will be enabled (assigned), and new ones will be disabled (pending assignment).
