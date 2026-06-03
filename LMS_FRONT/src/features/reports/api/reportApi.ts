import { apiClient } from '../../../core/api/apiClient';

export const reportApi = {
    list: (searchTerm: string = '', page: number = 1, size: number = 5, tenantId?: number, groupId?: number, generatedFrom?: string) => {
        const params = {
            searchTerm,
            pageNumber: page,
            pageSize: size,
            tenantId: tenantId || undefined,
            groupId: groupId || undefined,
            generatedFrom: generatedFrom || undefined
        };
        return apiClient.get('reports/list', { params });
    },
    getStudentReport: (userId: number, courseId: number) => {
        return apiClient.get(`reports/course/${userId}/${courseId}`);
    }
};
