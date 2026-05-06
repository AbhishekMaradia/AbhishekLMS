export interface LoginRequest {
    email: string;
    password: string;
}

export interface RegisterRequest {
    firstName: string;
    lastName: string;
    mobile: string;
    email: string;
    password: string;
    tenantId?: number;
    organizationCode?: string;
}

export interface UserDto {
    id: number;
    firstName: string;
    lastName: string;
    email: string;
    mobile: string;
    userRole: string;
    roleId?: number;       // Current active role ID
    tenantId?: number;
    orgName?: string;
    groupId?: number;
    groupName?: string;
    createdAt: string;
    isSuperAdmin?: boolean;
    isActive?: boolean;
}

export interface LoginResponse {
    token: string;
    expiresAt: string;
    user: UserDto;
    encryptedPermissions?: string;
}
