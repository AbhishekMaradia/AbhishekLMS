import { apiClient } from '../../../core/api/apiClient';
import type { ApiResponse } from '../../../core/types/api.types';

export interface RoleDto { id: number; code: string; name: string; isActive: boolean; isDefault: boolean; tenantId?: number; orgName?: string; }
export interface ModuleDto { id: number; name: string; code: string; isActive: boolean; }
export interface PermissionDto { id: number; name: string; code: string; isActive?: boolean; }
export interface ModulePermissionDto {
    id: number;
    moduleId: number;
    permissionId: number;
    moduleName: string;
    moduleCode: string;
    permissionName: string;
    permissionCode: string;
}
export interface RoleModulePermissionDto {
    id: number;
    roleModuleId: number;
    permissionId: number;
    roleName: string;
    roleCode: string;
    moduleName: string;
    moduleCode: string;
    permissionName: string;
    permissionCode: string;
    orgName?: string;
}
export interface RoleModuleDto {
    id: number;
    roleId: number;
    moduleId: number;
    roleName: string;
    moduleName: string;
}
export interface UserRoleDto {
    userId: number;
    roleId: number;
    roleName: string;
    roleCode: string;
    isActive: boolean;
    roleIsActive: boolean;
    userEmail?: string;
    tenantId?: number;
    orgName?: string;
}

export const securityApi = {
    // ROLES
    getRoles: (searchTerm: string = '', page: number = 1, size: number = 10, isActive?: boolean, tenantId?: number | null) =>
        apiClient.get<ApiResponse<RoleDto[]>>(`Roles/list?SearchTerm=${encodeURIComponent(searchTerm)}&PageNumber=${page}&PageSize=${size}${isActive !== undefined ? `&IsActive=${isActive}` : ''}${(tenantId !== undefined && tenantId !== null) ? `&TenantId=${tenantId}` : ''}`),
    createRole: (data: { code: string; name: string; isActive?: boolean; tenantId?: number | null }) =>
        apiClient.post<ApiResponse<RoleDto>>('Roles/create', data),
    updateRole: (id: number, data: { name: string; isActive?: boolean; tenantId?: number | null }) =>
        apiClient.put<ApiResponse<RoleDto>>(`Roles/update/${id}`, data),
    deleteRole: (id: number) =>
        apiClient.delete<ApiResponse<string>>(`Roles/delete/${id}`),

    // MODULES
    getModules: (searchTerm: string = '', page: number = 1, size: number = 100, isActive?: boolean) =>
        apiClient.get<ApiResponse<ModuleDto[]>>(`Modules/list?SearchTerm=${encodeURIComponent(searchTerm)}&PageNumber=${page}&PageSize=${size}${isActive !== undefined ? `&IsActive=${isActive}` : ''}`),
    createModule: (data: { code: string; name: string }) =>
        apiClient.post<ApiResponse<ModuleDto>>('Modules/create', data),
    updateModule: (id: number, data: { name: string; isActive?: boolean }) =>
        apiClient.put<ApiResponse<ModuleDto>>(`Modules/update/${id}`, data),
    deleteModule: (id: number) =>
        apiClient.delete<ApiResponse<string>>(`Modules/delete/${id}`),
    assignModulePermissions: (moduleId: number, permissionIds: number[]) =>
        apiClient.post<ApiResponse<string>>('Modules/assign-permissions', { moduleId, permissionIds }),

    // PERMISSIONS
    getPermissions: (searchTerm: string = '', page: number = 1, size: number = 100, isActive?: boolean) =>
        apiClient.get<ApiResponse<PermissionDto[]>>(`Permissions/list?SearchTerm=${encodeURIComponent(searchTerm)}&PageNumber=${page}&PageSize=${size}${isActive !== undefined ? `&IsActive=${isActive}` : ''}`),
    getModulePermissions: (moduleId: number) =>
        apiClient.get<ApiResponse<PermissionDto[]>>(`Modules/${moduleId}/permissions`),
    createPermission: (data: { code: string; name: string }) =>
        apiClient.post<ApiResponse<PermissionDto>>('Permissions/create', data),
    updatePermission: (id: number, data: { name: string; isActive?: boolean }) =>
        apiClient.put<ApiResponse<PermissionDto>>(`Permissions/update/${id}`, data),

    // ASSIGNMENTS (User-Role)
    assignUserRole: (userId: number, roleId: number, tenantId?: number | null) =>
        apiClient.post<ApiResponse<string>>('user-permissions/assign-role', { userId, roleId, tenantId }),

    // ASSIGNMENTS (Role-Module-Permission)
    getRolePermissions: (roleId: number, moduleId: number, tenantId?: number | null) =>
        apiClient.get<ApiResponse<any[]>>(`user-permissions/role-module/${roleId}/${moduleId}/permissions${(tenantId !== undefined && tenantId !== null) ? `?tenantId=${tenantId}` : ''}`),

    assignPermissionsToRole: (roleId: number, moduleId: number, permissionIds: number[], tenantId?: number | null) =>
        apiClient.post<ApiResponse<string>>('user-permissions/assign-permissions', { roleId, moduleId, permissionIds, tenantId }),

    getUserPermissions: (userId: number) =>
        apiClient.get<ApiResponse<any>>(`user-permissions/user/${userId}`),

    updateUserRole: (userId: number, roleId: number, data: { isActive: boolean; newRoleId?: number; newTenantId?: number | null }, tenantId?: number | null) =>
        apiClient.put<ApiResponse<string>>(`user-permissions/user/${userId}/role/${roleId}${tenantId !== undefined && tenantId !== null ? `?tenantId=${tenantId}` : ''}`, data),
    
    updateUserRoleStatus: (userId: number, roleId: number, isActive: boolean, tenantId?: number | null) =>
        apiClient.put<ApiResponse<string>>(`user-permissions/user/${userId}/role/${roleId}${tenantId !== undefined && tenantId !== null ? `?tenantId=${tenantId}` : ''}`, { isActive }),
    removeUserRole: (userId: number, roleId: number, tenantId?: number | null) =>
        apiClient.delete<ApiResponse<string>>(`user-permissions/user/${userId}/role/${roleId}${tenantId !== undefined && tenantId !== null ? `?tenantId=${tenantId}` : ''}`),
    getUserRoles: (userId: number) =>
        apiClient.get<ApiResponse<UserRoleDto[]>>(`user-permissions/user/${userId}/roles`),
    getUserRolesList: (searchTerm: string = '', page: number = 1, size: number = 10, tenantId?: number | null, isActive?: boolean) =>
        apiClient.get<ApiResponse<UserRoleDto[]>>(`user-permissions/user-roles/list?SearchTerm=${encodeURIComponent(searchTerm)}&PageNumber=${page}&PageSize=${size}${(tenantId !== undefined && tenantId !== null) ? `&TenantId=${tenantId}` : ''}${isActive !== undefined ? `&IsActive=${isActive}` : ''}`),
    checkUserPermission: (userId: number, moduleCode: string, permissionCode: string) =>
        apiClient.get<ApiResponse<any>>(`user-permissions/user/${userId}/check/${moduleCode}/${permissionCode}`),

    deletePermission: (id: number) =>
        apiClient.delete<ApiResponse<string>>(`Permissions/delete/${id}`),

    // MODULE-PERMISSIONS (List)
    getModulePermissionsList: (searchTerm: string = '', page: number = 1, size: number = 10, moduleId?: number, permissionId?: number, tenantId?: number | null) =>
        apiClient.get<ApiResponse<ModulePermissionDto[]>>(
            `module-permissions/list?SearchTerm=${encodeURIComponent(searchTerm)}&PageNumber=${page}&PageSize=${size}${moduleId ? `&ModuleId=${moduleId}` : ''}${permissionId ? `&PermissionId=${permissionId}` : ''}${(tenantId !== undefined && tenantId !== null) ? `&TenantId=${tenantId}` : ''}`
        ),

    // ROLE-MODULES
    getRoleModulesList: (searchTerm: string = '', page: number = 1, size: number = 10, roleId?: number, tenantId?: number | null) =>
        apiClient.get<ApiResponse<RoleModuleDto[]>>(`RoleModules/list?SearchTerm=${encodeURIComponent(searchTerm)}&PageNumber=${page}&PageSize=${size}${roleId ? `&RoleId=${roleId}` : ''}${(tenantId !== undefined && tenantId !== null) ? `&TenantId=${tenantId}` : ''}`),
    getRoleModulesByRole: (roleId: number) =>
        apiClient.get<ApiResponse<RoleModuleDto[]>>(`RoleModules/role/${roleId}`),
    createRoleModule: (data: { roleId: number; moduleId: number }) =>
        apiClient.post<ApiResponse<RoleModuleDto>>('RoleModules/create', data),
    deleteRoleModule: (id: number) =>
        apiClient.delete<ApiResponse<string>>(`RoleModules/delete/${id}`),

    // ROLE-MODULE-PERMISSIONS
    getRoleModulePermissionsList: (searchTerm: string = '', page: number = 1, size: number = 10, tenantId?: number | null) =>
        apiClient.get<ApiResponse<RoleModulePermissionDto[]>>(`role-module-permissions/list?SearchTerm=${encodeURIComponent(searchTerm)}&PageNumber=${page}&PageSize=${size}${(tenantId !== undefined && tenantId !== null) ? `&TenantId=${tenantId}` : ''}`),
    getRoleModulePermission: (id: number) =>
        apiClient.get<ApiResponse<RoleModulePermissionDto>>(`role-module-permissions/${id}`),
    deleteRoleModulePermission: (id: number) =>
        apiClient.delete<ApiResponse<string>>(`role-module-permissions/${id}`),
};
