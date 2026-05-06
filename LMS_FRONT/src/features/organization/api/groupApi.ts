import { apiClient } from '../../../core/api/apiClient';
import type { ApiResponse } from '../../../core/types/api.types';

export interface GroupDto {
    groupId: number;
    groupName: string;
    description?: string;
    isActive: boolean;
    tenantId?: number;
    orgName?: string;
}

export const groupApi = {
    getAll: (page: number = 1, size: number = 10) =>
        apiClient.get<ApiResponse<GroupDto[]>>(`/Groups/list?PageNumber=${page}&PageSize=${size}`),

    getById: (id: number) =>
        apiClient.get<ApiResponse<GroupDto>>(`/Groups/${id}`),

    create: (data: Partial<GroupDto>) =>
        apiClient.post<ApiResponse<GroupDto>>('/Groups/create', data),

    update: (id: number, data: Partial<GroupDto>) =>
        apiClient.put<ApiResponse<GroupDto>>(`/Groups/update/${id}`, data),

    delete: (id: number) =>
        apiClient.delete<ApiResponse<string>>(`/Groups/delete/${id}`),
};
