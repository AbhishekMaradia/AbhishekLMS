export interface ApiResponse<T> {
    data: T;
    message?: string;
    success: boolean;
}

export interface PaginatedResponse<T> {
    data: T;
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    message?: string;
    success: boolean;
}

export interface ApiError {
    message: string;
    status?: number;
    errors?: Record<string, string[]>;
}
