import { apiClient } from '../../../core/api/apiClient';
import type { ApiResponse } from '../../../core/types/api.types';

export interface UserDto {
    id: number;
    email: string;
    firstName: string;
    lastName: string;
    mobile?: string;
    userRole?: string;
    orgId?: number;
    orgName?: string;
    isActive: boolean;
}

export const userApi = {
    // READ: Multi-tenant listing
    list: (params: { SearchTerm?: string; PageNumber?: number; PageSize?: number; TenantId?: number | null }) => {
        const q = new URLSearchParams();
        if (params.SearchTerm) q.append('SearchTerm', params.SearchTerm);
        if (params.PageNumber) q.append('PageNumber', String(params.PageNumber));
        if (params.PageSize) q.append('PageSize', String(params.PageSize));
        if (params.TenantId) q.append('TenantId', String(params.TenantId));
        return apiClient.get<ApiResponse<UserDto[]>>(`User/userlist?${q.toString()}`, {
            headers: { 'Content-Type': 'application/json' }
        });
    },

    // Aliases for backwards compatibility
    getAll: (searchTerm: string = '', page: number = 1, size: number = 10, tenantId?: number | null) =>
        userApi.list({ SearchTerm: searchTerm, PageNumber: page, PageSize: size, TenantId: tenantId }),

    getAdminByTenant: (tenantId: number) =>
        apiClient.get<ApiResponse<UserDto>>(`User/admin/${tenantId}`, {
            headers: { 'Content-Type': 'application/json' }
        }),

    // READ: Individual resource
    getById: (id: number) =>
        apiClient.get<ApiResponse<UserDto>>(`User/${id}`),

    // CREATE: Using plain object or FormData support
    create: (data: any) =>
        apiClient.post<ApiResponse<UserDto>>('User/create', data, {
            headers: { 'Content-Type': 'application/json' }
        }),

    // UPDATE: Resource-level mutation
    update: (id: number, data: any) =>
        apiClient.put<ApiResponse<UserDto>>(`User/${id}`, data, {
            headers: { 'Content-Type': 'application/json' }
        }),

    // ASSIGN
    assign: (data: { userId: number; RoleId: number }) =>
        apiClient.post<ApiResponse<string>>('user-permissions/assign-role', data),

    // DELETE: Hard removal
    delete: (id: number) =>
        apiClient.delete<ApiResponse<string>>(`User/${id}`),
};
