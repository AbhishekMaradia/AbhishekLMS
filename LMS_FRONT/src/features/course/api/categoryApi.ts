import { apiClient } from '../../../core/api/apiClient';
import type { ApiResponse } from '../../../core/types/api.types';

export interface CategoryDto {
    categoryId: number;
    categoryName: string;
    orgId?: number;
    orgName?: string;
    tenantId?: number;
}

export const categoryApi = {
    // READ: Centralized listing logic
    list: (searchTerm: string = '', page: number = 1, size: number = 10, tenantId?: number | null) => {
        const q = new URLSearchParams();
        if (searchTerm) q.append('SearchTerm', searchTerm);
        q.append('PageNumber', String(page));
        q.append('PageSize', String(size));
        if (tenantId) q.append('TenantId', String(tenantId));
        return apiClient.get<ApiResponse<CategoryDto[]>>(`Category/list?${q.toString()}`);
    },

    // Aliases to maintain backwards compatibility
    getAll: (searchTerm: string = '', page: number = 1, size: number = 10, tenantId?: number | null) =>
        categoryApi.list(searchTerm, page, size, tenantId),

    // READ: Individual resource
    getById: (id: number) =>
        apiClient.get<ApiResponse<CategoryDto>>(`Category/${id}`),

    // CREATE: Using plain state support
    create: (data: { CategoryName: string; orgId?: number; tenantId?: number }) =>
        apiClient.post<ApiResponse<CategoryDto>>('Category/create', data),

    // UPDATE: Resource mutation
    update: (id: number, data: { CategoryName: string; orgId?: number; tenantId?: number }) =>
        apiClient.put<ApiResponse<CategoryDto>>(`Category/update/${id}`, data),

    // DELETE: Hard removal
    delete: (id: number) =>
        apiClient.delete<ApiResponse<string>>(`Category/delete/${id}`),
};
