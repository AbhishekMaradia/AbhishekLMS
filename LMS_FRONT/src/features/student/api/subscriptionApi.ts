import { apiClient } from '../../../core/api/apiClient';
import type { ApiResponse } from '../../../core/types/api.types';

export const subscriptionApi = {
    subscribe: (courseId: number) =>
        apiClient.post<ApiResponse<any>>('UserCourse/subscribe', { courseId }),

    unsubscribe: (courseId: number) =>
        apiClient.post<ApiResponse<any>>('UserCourse/unsubscribe', { courseId }),

    getMyCourses: () =>
        apiClient.get<ApiResponse<any[]>>('UserCourse/my-courses'),

    checkSubscription: (courseId: number) =>
        apiClient.get<ApiResponse<boolean>>(`UserCourse/check/${courseId}`),

    getList: (searchTerm: string = '', page: number = 1, size: number = 10, tenantId?: number | null) => {
        const q = new URLSearchParams();
        if (searchTerm) q.append('SearchTerm', searchTerm);
        q.append('PageNumber', String(page));
        q.append('PageSize', String(size));
        if (tenantId !== undefined && tenantId !== null) q.append('TenantId', String(tenantId));
        return apiClient.get<ApiResponse<any>>(`UserCourse/list?${q.toString()}`);
    },
    
    revoke: (userId: number, courseId: number) =>
        apiClient.post<ApiResponse<any>>('UserCourse/revoke', { userId, courseId })
};
