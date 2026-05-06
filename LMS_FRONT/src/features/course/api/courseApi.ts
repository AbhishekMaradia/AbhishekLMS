import { apiClient } from '../../../core/api/apiClient';
import type { ApiResponse } from '../../../core/types/api.types';
import type { CourseResponse } from '../types/course.types';

export const courseApi = {
    // READ: Centralized listing logic
    list: (searchTerm: string = '', page: number = 1, size: number = 10, tenantId?: number | null) =>
        apiClient.get<ApiResponse<CourseResponse[]>>(`Course/list?SearchTerm=${encodeURIComponent(searchTerm)}&PageNumber=${page}&PageSize=${size}${tenantId ? `&TenantId=${tenantId}` : ''}`),

    // Aliases to maintain backwards compatibility
    getCourses: (searchTerm: string = '', page: number = 1, size: number = 10, tenantId?: number | null) =>
        courseApi.list(searchTerm, page, size, tenantId),

    // READ: Individual resource
    getCourseById: (id: number) =>
        apiClient.get<ApiResponse<CourseResponse>>(`Course/${id}`),

    // Aliases for compatibility
    getById: (id: number) =>
        courseApi.getCourseById(id),

    // CREATE: Using FormData for multi-part (Curriculum images) support
    create: (formData: FormData) =>
        apiClient.post<ApiResponse<CourseResponse>>('Course/create', formData),

    // UPDATE: Resource-level mutation
    update: (id: number, formData: FormData) =>
        apiClient.put<ApiResponse<CourseResponse>>(`Course/update/${id}`, formData),

    // DELETE: Hard removal
    delete: (id: number) =>
        apiClient.delete<ApiResponse<string>>(`Course/delete/${id}`),

    // --- VIDEO ---
    uploadVideo: (courseId: number, formData: FormData) =>
        apiClient.post<ApiResponse<string>>(`CourseVideo/upload/${courseId}`, formData),

    getVideos: (courseId: number) =>
        apiClient.get<ApiResponse<any[]>>(`CourseVideo/list/${courseId}`),

    deleteVideo: (videoId: number) =>
        apiClient.delete<ApiResponse<string>>(`CourseVideo/delete/${videoId}`),

    updateVideo: (videoId: number, title: string, description: string) =>
        apiClient.put<ApiResponse<string>>(`CourseVideo/update/${videoId}`, { title, description }),

    // --- DOCUMENT ---
    uploadDocument: (courseId: number, formData: FormData) =>
        apiClient.post<ApiResponse<string>>(`CourseDocument/upload/${courseId}`, formData),

    getDocuments: (courseId: number) =>
        apiClient.get<ApiResponse<any[]>>(`CourseDocument/course/${courseId}`),

    deleteDocument: (documentId: number) =>
        apiClient.delete<ApiResponse<string>>(`CourseDocument/delete/${documentId}`),

    updateDocument: (documentId: number, docName: string, description: string) =>
        apiClient.put<ApiResponse<string>>(`CourseDocument/update/${documentId}`, { docName, description }),
};
