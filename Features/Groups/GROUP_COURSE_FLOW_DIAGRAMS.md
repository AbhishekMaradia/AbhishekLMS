# LMS Groups & Courses: Final Management Flow

This document explains the complete automated flow of how Courses and Groups interact in the LMS.

---

## 1. Automated Linkage Logic (The "Ever-Ready" List)

We use an **Auto-Assignment** strategy so that Admins never have to manually "search and add" courses to groups. Everything is pre-linked in a `Disabled` state.

### A. When a New Course is Created
```mermaid
sequenceDiagram
    participant Admin
    participant CourseService
    participant GroupService
    participant DB (GroupCourse)

    Admin->>CourseService: Create New Course ("React JS")
    CourseService->>CourseService: Save Course to DB
    CourseService->>GroupService: LinkCourseToAllGroups(CourseId)
    GroupService->>DB: Insert "React JS" into ALL existing Groups (IsEnable = false)
    Note over DB: Result: All groups now see "React JS" in their edit list but Students cannot see it yet.
```

### B. When a New Group is Created
```mermaid
sequenceDiagram
    participant Admin
    participant GroupService
    participant CourseRepo
    participant DB (GroupCourse)

    Admin->>GroupService: Create New Group ("Intern Batch B")
    GroupService->>GroupService: Save Group to DB
    GroupService->>CourseRepo: Get All Active Courses
    CourseRepo-->>GroupService: [C#, Java, Python, React]
    GroupService->>DB: Insert ALL courses into "Intern Batch B" mapping (IsEnable = false)
    Note over DB: Result: The new group is immediately ready with a full course list to enable.
```

---

## 2. Admin Management Flow (The "Bulk Update")

Admin decides which students (Groups) see which content.

```mermaid
graph TD
    A[Admin Opens Group Edit Popup] --> B[GET /api/Groups/group-courses/{groupId}]
    B --> C[Display Full Course List with Checkboxes]
    C --> D{Admin Toggles Checkboxes}
    D --> E[Click SAVE Button]
    E --> F[PUT /api/Groups/bulk-update-courses]
    F --> G[Update IsEnable: true/false in DB]
    G --> H[Success: Results visible to Students]
```

---

## 3. Student View Flow (The "End Result")

How the student actually sees the courses in their dashboard.

```mermaid
graph LR
    User((Student)) --> Login[Logs into App]
    Login --> JWT[JWT Token contains GroupId]
    JWT --> API[GET /api/Course/list]
    API --> Logic{Has GroupId?}
    Logic -- Yes --> Filter[Query: Only courses ENABLED for this GroupId]
    Logic -- No --> OrgCheck[Check if Org has ANY groups?]
    OrgCheck -- No Groups --> ShowAll[Show All Active Courses]
    OrgCheck -- Has Groups --> ShowNone[Show Empty List - Logic: Must be in a group]
    Filter --> Display[Student sees assigned courses]
```

---

## 4. Summary of Key APIs

| Purpose | Method | Endpoint |
| :--- | :--- | :--- |
| **List Group Courses** | `GET` | `/api/Groups/group-courses/{groupId}` |
| **Update Assignment** | `PUT` | `/api/Groups/bulk-update-courses` |
| **Student Catalog** | `GET` | `/api/Course/list` (Uses GroupId Claim) |

---

## 5. Engineering Benefits
1. **Consistency**: Mapping table is always up-to-date.
2. **Speed**: Filtering is a simple indexed query on `GroupId` and `IsEnable`.
3. **Control**: Admins can prepare courses in "Draft" (Disabled) and launch them organization-wide with single clicks.
