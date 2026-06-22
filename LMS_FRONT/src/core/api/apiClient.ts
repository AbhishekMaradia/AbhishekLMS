import axios from 'axios';
import type { AxiosError, InternalAxiosRequestConfig } from 'axios';
import { toast } from 'react-toastify';
import type { ApiError } from '../types/api.types';

// 1. Environment-Aware Configuration Strategy
/** 
 * ARCHITECT NOTE: 
 * We use '/api' (no trailing slash) to support leading slashes in endpoint 
 * calls (e.g., '/Auth/login'), resulting in a clean '/api/Auth/login' path.
 */
export const API_ORIGIN = '/api';

// 2. Core Instance Configuration
export const apiClient = axios.create({
    baseURL: API_ORIGIN,
    headers: {
        'Content-Type': 'application/json',
        'ngrok-skip-browser-warning': 'true' 
    }
});

// 2. Gatekeeper: Request Interceptor (Security & Context Injection)
apiClient.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
        // FORM DATA HANDLING: Axios needs to set the boundary for FormData
        if (config.data instanceof FormData) {
            delete config.headers['Content-Type'];
        }

        // AUTH INJECTION: Centralized JWT logic (Sync direct read)
        const token = localStorage.getItem('token');
        if (token) config.headers.Authorization = `Bearer ${token}`;

        // TENANT INJECTION: Centralized Multi-tenant logic (Sync direct read)
        const userStr = localStorage.getItem('user');
        if (userStr) {
            try {
                const user = JSON.parse(userStr);
                const role = (user?.userRole || user?.UserRole || '').toUpperCase();
                const isSuperAdmin = role === 'SUPERADMIN' || role === 'SUPER ADMIN';

                // SuperAdmins operate globally; regular users must send Tenant UID
                if (!isSuperAdmin) {
                    const tid = user?.tenantId || user?.TenantId;
                    if (tid) config.headers['X-Tenant-Id'] = String(tid);
                }
            } catch (e) {
                console.warn('[LMS API] Context Extraction Failure:', e);
            }
        }

        console.log(`[LMS API] SENT: ${config.method?.toUpperCase()} ${config.url}`, {
            hasToken: !!token,
            hasTenant: !!config.headers['X-Tenant-Id']
        });
        return config;
    },
    (error) => Promise.reject(error)
);

// 3. Sentinel: Response Interceptor (Global Error Handling & Data Normalization)
apiClient.interceptors.response.use(
    (res) => {
        // DATA NORMALIZATION: Skip for binary/blob data
        const rawBody = res.data;
        if (rawBody instanceof Blob) return res;

        if (rawBody && typeof rawBody === 'object') {
            const normalized = {
                ...rawBody,
                success: res.status < 300,
                data: rawBody.data ?? rawBody.Data ?? null,
                message: rawBody.message ?? rawBody.Message ?? '',
                errors: rawBody.errors ?? rawBody.Errors ?? null,
                totalCount: rawBody.totalRecords ?? rawBody.TotalRecords ?? rawBody.totalCount ?? 0
            };
            res.data = normalized;
        }

        return res;
    },
    (error: AxiosError) => {
        const status = error.response?.status;
        if (status === 404) {
            console.error(`[LMS 404] PATH: ${error.config?.url} (Check Controller Action name)`);
            debugger;
        }
        const data = error.response?.data as any;

        let apiError: ApiError = {
            message: data?.message || error.message || 'System synchronization error occurred',
            status,
            errors: data?.errors
        };

        // 401 Session Handling: Enterprise Auto-Logout
        if (status === 401) {
            localStorage.clear();
            if (!window.location.pathname.startsWith('/login')) {
                toast.error('Session expired. Please re-authenticate.');
                window.location.href = '/login';
            }
        } else if (status === 403) {
            toast.warning('Access Denied: Insufficient permissions for this action.');
        } else if (status === 500) {
            console.error('[API 500 ERROR]', data);
        }

        return Promise.reject(apiError);
    }
);
