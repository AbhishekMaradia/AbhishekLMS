import { apiClient } from '../../../core/api/apiClient';
import type { ApiResponse } from '../../../core/types/api.types';

export const groupApi = {
    // READ: Dashboard group listing
    list: (searchTerm: string = '', page: number = 1, size: number = 10, tenantId?: number | null) => {
        const q = new URLSearchParams();
        if (searchTerm) q.append('SearchTerm', searchTerm);
        q.append('PageNumber', String(page));
        q.append('PageSize', String(size));
        if (tenantId) q.append('TenantId', String(tenantId));
        return apiClient.get<ApiResponse<any>>(`/Groups/list?${q.toString()}`);
    },

    getGroups: (searchTerm: string = '', page: number = 1, size: number = 10, tenantId?: number | null) =>
        groupApi.list(searchTerm, page, size, tenantId),

    // READ: Individual resource
    getById: (id: number) =>
        apiClient.get<ApiResponse<any>>(`/Groups/${id}`),

    // CREATE: Using dynamic data
    create: (data: any) =>
        apiClient.post<ApiResponse<any>>('/Groups/create', data),

    // UPDATE: Resource mutation
    update: (id: number, data: any) =>
        apiClient.put<ApiResponse<any>>(`/Groups/update/${id}`, data),

    // DELETE: Hard removal
    delete: (id: number) =>
        apiClient.delete<ApiResponse<string>>(`/Groups/Delete/${id}`),

    // CORE: Group context management
    getGroupCourses: (groupId: number, page: number = 1, size: number = 50) =>
        apiClient.get<ApiResponse<any>>(`/Groups/group-courses/${groupId}?PageNumber=${page}&PageSize=${size}`),

    bulkUpdateCourses: (data: any) =>
        apiClient.put<ApiResponse<any>>(`/Groups/bulk-update-courses`, data),

    getGroupUsers: (groupId: number) =>
        apiClient.get<ApiResponse<any>>(`/Groups/group-users/${groupId}`),

    assignUsers: (data: any) =>
        apiClient.put<ApiResponse<any>>(`/Groups/assign-users`, data),

    submitAttendance: (data: any) => apiClient.post<ApiResponse<any>>('/Attendance/submit', data),
    getAttendanceList: (tenantId?: number, groupId?: number, courseId?: number) => {
        let url = `/Attendance/list?`;
        if (tenantId) url += `tenantId=${tenantId}&`;
        if (groupId) url += `groupId=${groupId}&`;
        if (courseId) url += `courseId=${courseId}&`;
        return apiClient.get<ApiResponse<any>>(url);
    },
    getAttendanceByFilters: (groupId: number, courseId: number, date: string) => apiClient.get<ApiResponse<any>>(`/Attendance/get?groupId=${groupId}&courseId=${courseId}&date=${date}`),
    deleteAttendance: (id: number) => apiClient.delete<ApiResponse<string>>(`/Attendance/delete/${id}`)
};
