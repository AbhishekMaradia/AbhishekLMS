import { apiClient } from '../../../core/api/apiClient';
import type { ApiResponse } from '../../../core/types/api.types';

export interface RoleDto {
    id: number;
    roleName: string;
    description?: string;
    isActive: boolean;
    tenantId?: number;
}

export const roleApi = {
    getAll: (pageNumber: number = 1, pageSize: number = 10) =>
        apiClient.get<ApiResponse<RoleDto[]>>(`/Roles/list?PageNumber=${pageNumber}&PageSize=${pageSize}`),

    create: (data: Partial<RoleDto>) =>
        apiClient.post<ApiResponse<RoleDto>>('/Roles/create', data),

    update: (id: number, data: Partial<RoleDto>) =>
        apiClient.put<ApiResponse<RoleDto>>(`/Roles/update/${id}`, data),

    delete: (id: number) =>
        apiClient.delete<ApiResponse<string>>(`/Roles/delete/${id}`),
};
