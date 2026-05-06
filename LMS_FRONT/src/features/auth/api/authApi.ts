import { apiClient } from '../../../core/api/apiClient';
import type { ApiResponse } from '../../../core/types/api.types';
import type { LoginRequest, LoginResponse, RegisterRequest } from '../types/auth.types';

export const authApi = {
    login: (data: LoginRequest) =>
        apiClient.post<ApiResponse<LoginResponse>>('/Auth/login', data),

    register: (data: RegisterRequest) =>
        apiClient.post<ApiResponse<LoginResponse>>('/Auth/register', data),

    forgotPassword: (email: string) =>
        apiClient.post<ApiResponse<any>>('/Auth/forgot-password', { email }),
};
