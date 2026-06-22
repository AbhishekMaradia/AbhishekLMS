# LMS Multi-Tenant System: Functionality & API Flow Guide

This document outlines the complete end-to-end flow of the LMS, identifying key actors, their actions, and the specific API endpoints responsible for each step.

Required Diagram Reference: `LMS_Full_Flow_Diagram.png` (Located in this folder).

---

## 🎭 1. Super Admin (System Owner)
**Goal:** Setup the environment for tenants.

| Step | Action | API Endpoint | Payload / Notes |
| :--- | :--- | :--- | :--- |
| **1.1** | **Create Organization** | `POST /api/Organization` | `{ "Name": "SoulCode", "Code": "soul01", "Domain": "..." }` |
| **1.2** | **Create Org Admin** | `POST /api/Auth/register` | `{ "Email": "admin@soulcode.com", "Role": "Admin", "TenantId": <NewOrgId> }` |

---

## 🏢 2. Organization Admin (Tenant Manager)
**Goal:** Create content and manage users.

| Step | Action | API Endpoint | Payload / Notes |
| :--- | :--- | :--- | :--- |
| **2.1** | **Login** | `POST /api/Auth/login` | `{ "Email": "...", "Password": "..." }` <br> *Returns JWT with TenantId* |
| **2.2** | **Create Course** | `POST /api/Course` | `{ "Title": "C# Masterclass", "Description": "...", "Price": 0 }` |
| **2.3** | **Upload Video** | `POST /api/CourseVideo` | `{ "CourseId": 10, "Title": "Intro", "VideoFile": <Binary> }` |
| **2.4** | **Create Group** | `POST /api/Groups/create` | `{ "GroupName": "Batch 2024" }` |
| **2.5** | **Enable Courses** | `PUT /api/Groups/{id}` | `{ "GroupName": "Batch 2024", "Courses": [{ "CourseId": 10, "IsEnable": true }] }` <br> **Critical Step for Visibility** |
| **2.6** | **Register Student** | `POST /api/Auth/register` | `{ "Email": "student@soulcode.com", "TenantId": <MyTenantId> }` <br> *Admin manually registers user* |

---

## 🎓 3. Student (End User)
**Goal:** Consume content and get certified.

| Step | Action | API Endpoint | Payload / Notes |
| :--- | :--- | :--- | :--- |
| **3.1** | **Public Registration** | `POST /api/Auth/register` | `{ "Email": "...", "OrganizationCode": "soul01" }` <br> *Uses OrgCode to link to Tenant* |
| **3.2** | **Login** | `POST /api/Auth/login` | *System authenticates & returns Token with specific TenantId* |
| **3.3** | **View Dashboard** | `GET /api/UserCourse/my-courses` | *Lists courses from Assigned Group + Subscribed Courses* |
| **3.4** | **Watch Video** | `GET /api/CourseVideo/stream/{id}` | *Streams encrypted video content* |
| **3.5** | **Track Progress** | `POST /api/CourseVideo/progress` | `{ "VideoId": 5, "Percentage": 100 }` |
| **3.6** | **Get Certificate** | `GET /api/Certificates/download` | *Available only when Course Progress = 100%* |

---

## 🔐 Security & Architecture Keypoints
1.  **Tenant Isolation:** All GET requests (Courses, Users) are automatically filtered by `TenantId` present in the JWT Token. An Admin of Tenant A cannot query Tenant B's data.
2.  **Permissions:** `[BackOfficePermission]` attributes ensure Students cannot access Admin APIs (like Create Course).
3.  **Group Visibility:** A Course added to a Group is **Hidden** by default. Admin must explicitly set `IsEnable: true` for students to see it.

---

## 🚀 Frontend Implementation Tip
For Frontend Developers:
- **Rule 1:** Always store the JWT Token from Login response.
- **Rule 2:** Attach `Authorization: Bearer <Token>` to EVERY API call.
- **Rule 3:** Use `OrganizationCode` field in registration forms for public users.
