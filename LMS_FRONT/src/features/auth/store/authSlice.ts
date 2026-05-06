import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { UserDto } from '../types/auth.types';

interface AuthState {
    user: any | null;
    token: string | null;
    isAuthenticated: boolean;
    loading: boolean;
    permissions: Record<string, string[]>;
}

const getInitialState = (): AuthState => {
    try {
        const token = localStorage.getItem('token');
        const userStr = localStorage.getItem('user');
        const permsStr = localStorage.getItem('permissions');
        if (token && userStr) {
            const user = JSON.parse(userStr);
            const permissions = permsStr ? JSON.parse(permsStr) : {};
            return { user, token, isAuthenticated: true, loading: false, permissions };
        }
    } catch (e) {
        console.error("[LMS AUTH] Initial State Sync Failed", e);
    }
    return { user: null, token: null, isAuthenticated: false, loading: false, permissions: {} };
};

const initialState: AuthState = getInitialState();

const authSlice = createSlice({
    name: 'auth',
    initialState,
    reducers: {
        setCredentials: (
            state,
            action: PayloadAction<{ user: any; token: string; permissions?: Record<string, string[]> }>
        ) => {
            const raw = action.payload.user;
            // Robust normalization for Backend <=> Frontend consistency
            state.user = {
                ...raw,
                id: raw.id || raw.Id,
                email: raw.email || raw.Email,
                firstName: raw.firstName || raw.FirstName,
                lastName: raw.lastName || raw.LastName,
                tenantId: raw.tenantId || raw.TenantId || null,
                userRole: raw.userRole || raw.UserRole,
                roleId: raw.roleId || raw.RoleId,
                roleCode: raw.roleCode || raw.RoleCode,
                roleName: raw.roleName || raw.RoleName,
                groupId: raw.groupId || raw.GroupId
            };
            const role = (state.user.userRole || "").toUpperCase();
            state.permissions = action.payload.permissions || {};
            state.token = action.payload.token || (action.payload as any).Token;
            state.isAuthenticated = true;
        },
        logout: (state) => {
            state.user = null;
            state.token = null;
            state.isAuthenticated = false;
            state.permissions = {};
        },
        setLoading: (state, action: PayloadAction<boolean>) => {
            state.loading = action.payload;
        },
    },
});

export const { setCredentials, logout, setLoading } = authSlice.actions;
export default authSlice.reducer;
