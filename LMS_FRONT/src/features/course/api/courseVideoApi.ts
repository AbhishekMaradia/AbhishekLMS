import { apiClient } from '../../../core/api/apiClient';
import type { ApiResponse } from '../../../core/types/api.types';
import type { CourseVideoDto, UserVideoProgressDto } from '../types/course.types';

export const courseVideoApi = {
    getVideosByCourse: (courseId: number) =>
        apiClient.get<ApiResponse<CourseVideoDto[]>>(`/CourseVideos/course/${courseId}`),

    getVideoById: (id: number) =>
        apiClient.get<ApiResponse<CourseVideoDto>>(`/CourseVideos/${id}`),

    getUserVideoProgress: () =>
        apiClient.get<ApiResponse<UserVideoProgressDto[]>>('/CourseVideos/user-progress'),
};
