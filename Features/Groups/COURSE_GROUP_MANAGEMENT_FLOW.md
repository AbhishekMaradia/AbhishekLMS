# Course-Level Group Management Flow

## Overview
This flow allows administrators to manage which **Groups** have access to a specific **Course** directly from the Course Management module. Instead of going to each group to add a course, the admin can pick a course and "push" it to multiple groups at once.

---

## 1. Fetching Groups for a Specific Course
When an admin opens the "Groups" or "Audience" tab in the Course Edit screen, the frontend needs to know:
1. All available groups in the organization.
2. Which groups already have this course enabled.

**Endpoint:** `GET /api/Groups/assignment-status/{courseId}`

**Response:**
```json
{
  "data": [
    {
      "groupId": 1,
      "groupName": "Engineering Team",
      "isAssigned": true
    },
    {
      "groupId": 2,
      "groupName": "Marketing Team",
      "isAssigned": false
    },
    {
      "groupId": 3,
      "groupName": "Interns 2024",
      "isAssigned": true
    }
  ],
  "message": "Success",
  "code": 200
}
```

**Frontend Action:**
- Display a list of groups with checkboxes.
- Check the boxes where `isAssigned: true`.

---

## 2. Saving Course-to-Group Assignments
When the admin clicks the "Save Assignments" button on the Course page.

**Endpoint:** `PUT /api/Course/assign-to-groups`

**Request Body:**
```json
{
  "courseId": 105,
  "groupIds": [1, 3] 
}
```
*(Note: If a GroupId is present in this list, the course will be enabled for that group. If it is missing from the list but was previously assigned, it will be unassigned/disabled.)*

**Response:**
```json
{
  "data": null,
  "message": "Course assigned to groups successfully",
  "code": 200
}
```

---

## 3. Recommended Frontend Implementation

### Step-by-Step UI Flow:
1. **Open Course Edit:** Admin navigates to `Courses` -> `Edit Business Communication`.
2. **Switch to Groups Tab:** Admin clicks on the "Manage Groups" tab.
3. **Load Data:** Call `GET /api/Groups/assignment-status/105`.
4. **User Updates:** Admin ticks "Sales Team" and unticks "Support Team".
5. **Click Save:** Call `PUT /api/Course/assign-to-groups` with the current list of checked IDs.

---

## 4. Key Engineering Benefits
- **Effortless Targeting:** Perfect for when you launch a "New Hire Training" course and want to push it to all relevant "Department" groups immediately.
- **Single Source of Truth:** You manage access where you manage the content.
- **Reduced Steps:** No need to jump back and forth between "Course" and "Groups" modules.

---

## 5. Implementation Roadmap (Backend)
To make this work, we will:
1. Add `GetGroupsWithAssignmentStatusAsync` in `GroupService`.
2. Add a new endpoint in `GroupsController` to status check.
3. Add `AssignCourseToGroupsAsync` in a new/existing service to handle the many-to-many update logic.
