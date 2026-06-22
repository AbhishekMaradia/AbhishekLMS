import { apiClient } from '../../../core/api/apiClient';
import type { ApiResponse } from '../../../core/types/api.types';

export interface OrganizationDto {
    id: number;
    orgName: string;
    orgCode: string;
    logoUrl?: string;
    logoThumbUrl?: string;
    website?: string;
    description?: string;
    primaryColor?: string;
    secondaryColor?: string;
    isActive: boolean;
    linkExpiredAt?: string;
    createdAt: string;
}

export const organizationApi = {
    // READ: Centralized listing logic
    list: (searchTerm: string = '', page: number = 1, size: number = 10) =>
        apiClient.get<ApiResponse<OrganizationDto[]>>(`Organization?SearchTerm=${encodeURIComponent(searchTerm)}&PageNumber=${page}&PageSize=${size}`, {
            headers: { 'Content-Type': 'application/json' }
        }),

    // Aliases to maintain backwards compatibility
    getAll: (searchTerm: string = '', page: number = 1, size: number = 10) => 
        organizationApi.list(searchTerm, page, size),

    getById: (id: number) => 
        apiClient.get<ApiResponse<OrganizationDto>>(`Organization/${id}`, {
            headers: { 'Content-Type': 'application/json' }
        }),

    // CREATE: Using FormData for multi-part (logo) support
    register: (data: FormData) =>
        apiClient.post<ApiResponse<OrganizationDto>>('Organization/register', data, {
            headers: { 'Content-Type': 'multipart/form-data' }
        }),

    generateLink: (orgCode?: string, expiry?: string) =>
        apiClient.post<ApiResponse<{ url: string; expiry: string }>>('Organization/generate-link', { orgCode, expiry }, {
            headers: { 'Content-Type': 'application/json' }
        }),

    // UPDATE: Resource-specific put
    update: (id: number, data: FormData) =>
        apiClient.put<ApiResponse<OrganizationDto>>(`Organization/${id}`, data, {
            headers: { 'Content-Type': 'multipart/form-data' }
        }),

    // DELETE: Hard removal
    delete: (id: number) =>
        apiClient.delete<ApiResponse<string>>(`Organization/${id}`, {
            headers: { 'Content-Type': 'application/json' }
        }),
};
