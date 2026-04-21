# Groups Feature - Flow Documentation

## Overview
Group feature allows organizing courses into groups per organization/tenant. Each group can have multiple courses that can be enabled or disabled.

## API Flow

### 1. Display Groups List (Organization-wise)
**Endpoint:** `GET /api/Groups/list`

**Query Parameters:**
```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "searchTerm": "optional search text"
}
```

**Response:**
```json
{
  "data": [
    {
      "id": 1,
      "groupName": "Engineering Group",
      "createdAt": "2026-02-07T00:00:00Z"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 50,
  "totalPages": 5,
  "message": "Success",
  "code": 200
}
```

**Frontend Action:** 
- Display list of groups in a table/grid
- Show organization's groups only (backend filters by CurrentTenantId)

---

### 2. Show Group Details with Courses (Popup)
**Endpoint:** `GET /api/Groups/{id}`

**Response:**
```json
{
  "data": {
    "id": 1,
    "groupName": "Engineering Group",
    "groupCourses": [
      {
        "id": 101,
        "groupId": 1,
        "courseId": 5,
        "courseName": "C# Programming",
        "isEnable": true,
        "createdAt": "2026-02-07T00:00:00Z",
        "updatedAt": "2026-02-07T00:00:00Z"
      },
      {
        "id": 102,
        "groupId": 1,
        "courseId": 8,
        "courseName": "ASP.NET Core",
        "isEnable": false,
        "createdAt": "2026-02-07T00:00:00Z",
        "updatedAt": "2026-02-07T00:00:00Z"
      }
    ],
    "createdAt": "2026-02-05T00:00:00Z",
    "updatedAt": "2026-02-07T00:00:00Z"
  },
  "message": "Success",
  "code": 200
}
```

**Frontend Action:** 
- Open popup/modal when user clicks on a group row
- Show group name in editable input field
- Display all courses with checkboxes (checked if `isEnable: true`)
- Provide "Save" button

---

### 3. Update Group Name + Multiple Courses (Save from Popup)
**Endpoint:** `PUT /api/Groups/{id}`

**Request Body:**
```json
{
  "groupName": "Updated Engineering Group",
  "courses": [
    {
      "courseId": 5,
      "isEnable": true
    },
    {
      "courseId": 8,
      "isEnable": true
    },
    {
      "courseId": 12,
      "isEnable": false
    }
  ]
}
```

**Response:**
```json
{
  "data": {
    "id": 1,
    "groupName": "Updated Engineering Group",
    "groupCourses": [...],
    "createdAt": "2026-02-05T00:00:00Z",
    "updatedAt": "2026-02-07T00:28:00Z"
  },
  "message": "Updated successfully",
  "code": 200
}
```

**Frontend Action:** 
- When user clicks "Save" button in popup:
  1. Get the current group name from input field
  2. Get all checked/unchecked courses
  3. Build `UpdateGroupRequest` object
  4. Send PUT request
  5. Close popup on success
  6. Refresh groups list

---

### 4. Delete Group
**Endpoint:** `DELETE /api/Groups/Delete/{id}`

**Response:**
```json
{
  "data": null,
  "message": "Deleted successfully",
  "code": 200
}
```

---

### 5. Create New Group
**Endpoint:** `POST /api/Groups/create`

**Request Body:**
```json
{
  "groupName": "New Group Name"
}
```

**Response:**
```json
{
  "data": {
    "id": 10,
    "groupName": "New Group Name",
    "groupCourses": [],
    "createdAt": "2026-02-07T00:30:00Z",
    "updatedAt": "2026-02-07T00:30:00Z"
  },
  "message": "Created successfully",
  "code": 201
}
```

**Note:** New groups are created with all existing courses automatically added with `isEnable: false` (disabled by default).

---

## Frontend Implementation Guide

### Step-by-Step Flow:

1. **On "Groups" Tab Click:**
   ```javascript
   // Call API
   GET /api/Groups/list?pageNumber=1&pageSize=10
   
   // Display groups in table
   ```

2. **On Group Row Click:**
   ```javascript
   // Call API to get group details
   GET /api/Groups/{groupId}
   
   // Open popup with:
   // - Editable group name input
   // - List of courses with checkboxes (checked based on isEnable)
   // - Save button
   ```

3. **On Save Button Click (in Popup):**
   ```javascript
   // Prepare request
   const updateRequest = {
     groupName: groupNameInput.value,
     courses: selectedCourses.map(c => ({
       courseId: c.id,
       isEnable: c.checked
     }))
   };
   
   // Call API
   PUT /api/Groups/{groupId}
   Body: updateRequest
   
   // On success:
   // - Close popup
   // - Refresh groups list
   // - Show success message
   ```

---

## Important Notes

- **Tenant Isolation:** All APIs automatically filter by `CurrentTenantId` from JWT token
- **Validation:** Group name is required
- **Bulk Update:** Single API call updates both group name and all course statuses
- **Auto-Add Courses:** When a new course is created, it's automatically added to all existing groups (disabled by default)
- **Soft Delete:** Groups are soft-deleted (IsDeleted flag)

---

## Error Handling

| Status Code | Message | Action |
|------------|---------|--------|
| 200 | Success | Continue |
| 201 | Created | Show success, refresh list |
| 400 | Bad Request | Show validation errors |
| 403 | Forbidden | User doesn't have access to this group |
| 404 | Not Found | Group doesn't exist |
| 500 | Server Error | Show error, contact support |

---

## Example Frontend Popup Component (Pseudo-code)

```javascript
// When group is clicked
async function openGroupPopup(groupId) {
  // Fetch group details
  const response = await fetch(`/api/Groups/${groupId}`);
  const result = await response.json();
  
  const group = result.data;
  
  // Populate popup
  document.getElementById('groupName').value = group.groupName;
  
  // Render courses with checkboxes
  const coursesList = document.getElementById('coursesList');
  group.groupCourses.forEach(gc => {
    const checkbox = `
      <label>
        <input type="checkbox" 
               data-course-id="${gc.courseId}" 
               ${gc.isEnable ? 'checked' : ''}>
        ${gc.courseName}
      </label>
    `;
    coursesList.innerHTML += checkbox;
  });
  
  // Show popup
  showPopup();
}

// When save button is clicked
async function saveGroup(groupId) {
  const groupName = document.getElementById('groupName').value;
  
  // Collect all courses with their status
  const checkboxes = document.querySelectorAll('[data-course-id]');
  const courses = Array.from(checkboxes).map(cb => ({
    courseId: parseInt(cb.dataset.courseId),
    isEnable: cb.checked
  }));
  
  // Send update request
  const response = await fetch(`/api/Groups/${groupId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ groupName, courses })
  });
  
  if (response.ok) {
    closePopup();
    refreshGroupsList();
    showSuccessMessage('Group updated successfully!');
  } else {
    showErrorMessage('Failed to update group');
  }
}
```

---

## Database Schema Reference

### Groups Table
- `Id` (int, PK)
- `GroupName` (nvarchar)
- `TenantId` (int, nullable)
- `CreatedAt` (datetime2)
- `UpdatedAt` (datetime2)
- `IsDeleted` (bit)
- `DeletedAt` (datetime2, nullable)

### GroupCourses Table
- `Id` (int, PK)
- `GroupId` (int, FK to Groups)
- `CourseId` (int, FK to Courses)
- `IsEnable` (bit)
- `TenantId` (int, nullable)
- `CreatedAt` (datetime2)
- `UpdatedAt` (datetime2)
- `IsDeleted` (bit)
- `DeletedAt` (datetime2, nullable)
