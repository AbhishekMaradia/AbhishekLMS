# User-to-Group Assignment Flow

## Overview
This flow allows administrators to manage which **Users** belong to a specific **Group**. This can be done by selecting multiple users and assigning them to a group in one action.

---

## 1. Fetching Users for Group Assignment
When an admin opens the "Add Users" or "Members" tab in the Group Edit screen, the frontend needs to know:
1. All users in the organization.
2. Which users are currently assigned to this group.
3. (Optional) Which users are currently unassigned.

**Endpoint:** `GET /api/Groups/group-users/{groupId}`

**Response:**
```json
{
  "data": [
    {
      "userId": 1,
      "userName": "Abhishek Maradia",
      "email": "abhishek@example.com",
      "isAssigned": true
    },
    {
      "userId": 2,
      "userName": "John Doe",
      "email": "john@example.com",
      "isAssigned": false
    }
  ],
  "message": "Success",
  "code": 200
}
```

---

## 2. Bulk Assigning Users to a Group
When the admin selects multiple users and clicks "Save Members".

**Endpoint:** `PUT /api/Groups/assign-users`

**Request Body:**
```json
{
  "groupId": 10,
  "userIds": [1, 5, 22]
}
```

**Backend Logic:**
1. Fetch all users in the organization (`TenantId`).
2. For all users whose `Id` is in `userIds`, set `GroupId = groupId`.
3. (Optional) For users previously in this group but NOT in `userIds`, set `GroupId = null`? 
   *Decision: Usually, "Assign to Group" in a multi-select picklist means "These are the members now". So we should clear others.*

**Response:**
```json
{
  "data": null,
  "message": "Users assigned to group successfully",
  "code": 200
}
```

---

## 3. Implementation Plan

### Backend Changes:
1. **DTOs**: Add `BulkAssignUsersRequest` and `GroupUserDto`.
2. **Repository**: 
   - Add `GetUsersByGroupIdAsync` or enhance `GetUsersAsync` in `UserRepository`.
   - Add `BulkUpdateUserGroupAsync` in `UserRepository`.
3. **Service**:
   - Add `GetGroupUsersAsync` in `GroupService`.
   - Add `AssignUsersToGroupAsync` in `GroupService`.
4. **Controller**:
   - Add `GET /api/Groups/group-users/{groupId}`.
   - Add `PUT /api/Groups/assign-users`.
