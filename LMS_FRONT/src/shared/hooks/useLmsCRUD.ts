import { useCallback } from 'react';
import { toast } from 'react-toastify';
import { organizationApi } from '../../features/organization/api/organizationApi';
import { courseApi } from '../../features/course/api/courseApi';
import { userApi } from '../../features/auth/api/userApi';
import { securityApi } from '../../features/auth/api/securityApi';
import { categoryApi } from '../../features/course/api/categoryApi';
import { groupApi } from '../../features/group/api/groupApi';

export const extractData = (res: any) => {
    // res is AxiosResponse, res.data is ApiResponse<T>
    if (res?.data?.success || res?.status === 200 || res?.status === 201) {
        const body = res.data; // ApiResponse body
        // Sometimes the API returns { success: true, data: [...] }
        // Sometimes it might be { success: true, items: [...] }
        return body.data ?? body.items ?? body.Items ?? body.Data ?? body;
    }
    return null;
};

export const useLmsCRUD = (user: any, isSuperAdmin: boolean, db: any, setDb: any, setUi: any, setOrgAdminData?: any) => {

    const fetchOrgAdmin = useCallback(async (org: any) => {
        if (!org) return;
        setUi((prev: any) => ({ ...prev, loading: true }));
        try {
            const orgId = org.id || org.Id;
            const res = await userApi.getAdminByTenant(orgId);
            const data = extractData(res);
            if (data && setOrgAdminData) {
                setOrgAdminData(data);
            } else {
                toast.error("Administrator profile not found for this organization.");
            }
        } catch (err: any) {
            toast.error(err.message || "Failed to fetch admin details.");
        } finally {
            setUi((prev: any) => ({ ...prev, loading: false }));
        }
    }, [setUi, setOrgAdminData]);

    const getOrgNameByTenant = useCallback((tenantId: number | null) => {
        if (!tenantId || tenantId === 0) return 'Super Admin';
        const org = db.orgs.find((o: any) => Number(o.id || o.Id) === Number(tenantId));
        return org?.orgName || (org as any)?.OrgName || 'Unknown Organization';
    }, [db.orgs]);

    const syncOrgs = useCallback(async () => {
        try {
            const tid = Number(user?.tenantId ?? user?.TenantId ?? 0);
            if (isSuperAdmin) {
                const res = await organizationApi.getAll('', 1, 1000);
                setDb((prev: any) => ({ ...prev, orgs: extractData(res) || [] }));
            } else if (tid > 0) {
                let data;
                try {
                    const res = await organizationApi.getById(tid);
                    data = extractData(res);
                } catch (apiErr) {
                    console.warn("API restricted or failed, falling back to session orgName.", apiErr);
                }

                // Fallback: If API doesn't return data (permissions?) but user has it in session
                const seedOrg = data || { id: tid, Id: tid, orgName: (user as any).orgName || (user as any).OrgName || 'My Organization' };
                setDb((prev: any) => ({ ...prev, orgs: [seedOrg] }));
            }
        } catch (err) { console.error("Sync Orgs Failed", err); }
    }, [setDb, isSuperAdmin, user]);

    const syncUsers = useCallback(async () => {
        try {
            const tid = isSuperAdmin ? undefined : Number(user?.tenantId ?? user?.TenantId ?? 0);
            const res = await userApi.getAll('', 1, 1000, tid);
            setDb((prev: any) => ({ ...prev, users: extractData(res) || [] }));
        } catch (err) { console.error("Sync Users Failed", err); }
    }, [setDb, isSuperAdmin, user]);

    const syncCourses = useCallback(async () => {
        try {
            const tid = isSuperAdmin ? undefined : Number(user?.tenantId ?? user?.TenantId ?? 0);
            const res = await courseApi.getCourses('', 1, 1000, tid);
            setDb((prev: any) => ({ ...prev, courses: extractData(res) || [] }));
        } catch (err) { console.error("Sync Courses Failed", err); }
    }, [setDb, isSuperAdmin, user]);

    const syncCmCourses = syncCourses;

    const syncCats = useCallback(async () => {
        try {
            const tid = isSuperAdmin ? undefined : Number(user?.tenantId ?? user?.TenantId ?? 0);
            const res = await categoryApi.getAll('', 1, 1000, tid);
            setDb((prev: any) => ({ ...prev, cats: extractData(res) || [] }));
        } catch (err) { console.error("Sync Cats Failed", err); }
    }, [setDb, isSuperAdmin, user]);

    const syncGroups = useCallback(async () => {
        try {
            const tid = isSuperAdmin ? undefined : Number(user?.tenantId ?? user?.TenantId ?? 0);
            const res = await groupApi.getGroups('', 1, 1000, tid);
            setDb((prev: any) => ({ ...prev, groups: extractData(res) || [] }));
        } catch (err) { console.error("Sync Groups Failed", err); }
    }, [setDb, isSuperAdmin, user]);

    const syncRoles = useCallback(async (searchTerm: string = '', page: number = 1, size: number = 1000, isActive?: boolean, tenantId?: number | null) => {
        try {
            const tid = tenantId !== undefined ? tenantId : (isSuperAdmin ? undefined : Number(user?.tenantId ?? user?.TenantId ?? 0));
            const res = await securityApi.getRoles(searchTerm, page, size, isActive, tid);
            setDb((prev: any) => ({ ...prev, roles: extractData(res) || [] }));
        } catch (err) { console.error("Sync Roles Failed", err); }
    }, [setDb, isSuperAdmin, user]);

    const syncModules = useCallback(async (searchTerm: string = '', page: number = 1, size: number = 1000) => {
        try {
            const res = await securityApi.getModules(searchTerm, page, size);
            setDb((prev: any) => ({ ...prev, modules: extractData(res) || [] }));
        } catch (err) { console.error("Sync Modules Failed", err); }
    }, [setDb]);

    const syncPermissions = useCallback(async (searchTerm: string = '', page: number = 1, size: number = 1000) => {
        try {
            const res = await securityApi.getPermissions(searchTerm, page, size);
            setDb((prev: any) => ({ ...prev, perms: extractData(res) || [] }));
        } catch (err) { console.error("Sync Permissions Failed", err); }
    }, [setDb]);

    const syncModPerms = useCallback(async (searchTerm: string = '', page: number = 1, size: number = 1000, moduleId?: number, permissionId?: number, tenantId?: number | null) => {
        try {
            const tid = tenantId !== undefined ? tenantId : (isSuperAdmin ? undefined : Number(user?.tenantId ?? user?.TenantId ?? 0));
            const res = await securityApi.getModulePermissionsList(searchTerm, page, size, moduleId, permissionId, tid);
            setDb((prev: any) => ({ ...prev, modPerms: extractData(res) || [] }));
        } catch (err) { console.error("Sync ModPerms Failed", err); }
    }, [setDb, isSuperAdmin, user]);

    const syncRoleModules = useCallback(async (searchTerm: string = '', page: number = 1, size: number = 1000, roleId?: number, tenantId?: number | null) => {
        try {
            const tid = tenantId !== undefined ? tenantId : (isSuperAdmin ? undefined : Number(user?.tenantId ?? user?.TenantId ?? 0));
            const res = await securityApi.getRoleModulesList(searchTerm, page, size, roleId, tid);
            setDb((prev: any) => ({ ...prev, roleModules: extractData(res) || [] }));
        } catch (err) { console.error("Sync RoleModules Failed", err); }
    }, [setDb, isSuperAdmin, user]);

    const syncRoleModPerms = useCallback(async (searchTerm: string = '', page: number = 1, size: number = 1000, tenantId?: number | null) => {
        try {
            const tid = tenantId !== undefined ? tenantId : (isSuperAdmin ? undefined : Number(user?.tenantId ?? user?.TenantId ?? 0));
            const res = await securityApi.getRoleModulePermissionsList(searchTerm, page, size, tid);
            setDb((prev: any) => ({ ...prev, roleModPerms: extractData(res) || [] }));
        } catch (err) { console.error("Sync RoleModPerms Failed", err); }
    }, [setDb, isSuperAdmin, user]);

    const syncUserRoles = useCallback(async (searchTerm: string = '', page: number = 1, size: number = 1000, tenantId?: number | null) => {
        try {
            const tid = tenantId !== undefined ? tenantId : (isSuperAdmin ? undefined : Number(user?.tenantId ?? user?.TenantId ?? 0));
            const res = await securityApi.getUserRolesList(searchTerm, page, size, tid);
            setDb((prev: any) => ({ ...prev, userRoles: extractData(res) || [] }));
        } catch (err) { console.error("Sync UserRoles Failed", err); }
    }, [setDb, isSuperAdmin, user]);

    const syncAll = useCallback(() => {
        syncOrgs();
        syncUsers();
        syncCourses();
        syncCats();
        syncGroups();
        syncRoles();
        syncModules();
        syncPermissions();
        syncModPerms();
        syncRoleModules();
        syncRoleModPerms();
        syncUserRoles();
    }, [syncOrgs, syncUsers, syncCourses, syncCats, syncGroups, syncRoles, syncModules, syncPermissions, syncModPerms, syncRoleModules, syncRoleModPerms, syncUserRoles]);

    const HandleCRUD = async (rawAction: string, entity: string, data: any) => {
        const action = rawAction === 'edit' ? 'update' : rawAction;
        const tid = Number((user as any)?.tenantId ?? (user as any)?.TenantId ?? 0);

        // Injection of TenantId if OrgAdmin
        const body = { ...data };
        if (!isSuperAdmin && tid > 0) {
            body.TenantId = tid;
            body.tenantId = tid;
        }
        // Helper to extract ID from data (which could be a raw number/string or an object)
        const getId = (d: any) => {
            if (typeof d === 'number' || typeof d === 'string') return d;
            return d?.id || d?.Id || d?.userId || d?.UserId || d?.roleId || d?.RoleId || d?.moduleId || d?.ModuleId || d?.permissionId || d?.PermissionId || d?.id;
        };
        const targetId = getId(data);
        setUi((prev: any) => ({ ...prev, loading: true }));

        try {
            let res: any;

            switch (`${action}_${entity}`) {
                // Organizations
                case 'create_org': res = await organizationApi.register(body); break;
                case 'update_org': res = await organizationApi.update(body.id || body.Id, body); break;
                case 'delete_org': res = await organizationApi.delete(targetId); break;

                // Courses
                case 'create_course': res = await courseApi.create(body); break;
                case 'update_course': res = await courseApi.update(body.id || body.Id, body); break;
                case 'delete_course': res = await courseApi.delete(targetId); break;

                // Categories
                case 'create_cat': res = await categoryApi.create(body); break;
                case 'update_cat': res = await categoryApi.update(body.id || body.Id, body); break;
                case 'delete_cat': res = await categoryApi.delete(targetId); break;

                // Groups
                case 'create_group': res = await groupApi.create(body); break;
                case 'update_group': res = await groupApi.update(body.id || body.Id, body); break;
                case 'delete_group': res = await groupApi.delete(targetId); break;

                // Users
                case 'create_user': res = await userApi.create(body); break;
                case 'update_user': res = await userApi.update(body.id || body.Id || body.userId || body.UserId, body); break;
                case 'delete_user': res = await userApi.delete(targetId); break;
                case 'assign_user': res = await userApi.assign({ userId: body.userId, RoleId: body.roleId || body.RoleId }); break;

                // Roles
                case 'create_role': res = await securityApi.createRole(body); break;
                case 'update_role': res = await securityApi.updateRole(body.id || body.Id || body.roleId || body.RoleId, body); break;
                case 'delete_role': res = await securityApi.deleteRole(targetId); break;

                // Modules
                case 'create_module': res = await securityApi.createModule(body); break;
                case 'update_module': res = await securityApi.updateModule(body.id || body.Id || body.moduleId || body.ModuleId, body); break;
                case 'delete_module': res = await securityApi.deleteModule(targetId); break;

                // Permissions
                case 'create_perm': res = await securityApi.createPermission(body); break;
                case 'update_perm': res = await securityApi.updatePermission(body.id || body.Id || body.permissionId || body.PermissionId, body); break;
                case 'delete_perm': res = await securityApi.deletePermission(targetId); break;

                // Module Permissions (Collective assignment)
                case 'create_modPerm': res = await securityApi.assignModulePermissions(data.moduleId, [data.permissionId]); break;
                case 'delete_modPerm': res = await securityApi.assignModulePermissions(data.moduleId, []); break; // Note: Typically you would remove just one but the API uses a list here.

                // Role Modules
                case 'create_roleModule': res = await securityApi.createRoleModule(data); break;
                case 'delete_roleModule': res = await securityApi.deleteRoleModule(data.id || data); break;

                // Role Module Permissions (Collective assignment)
                case 'create_roleModPerm': res = await securityApi.assignPermissionsToRole(data.RoleId || data.roleId, data.ModuleId || data.moduleId, [data.PermissionId || data.permissionId], data.TenantId || data.tenantId); break;
                case 'update_roleModPerm': res = await securityApi.assignPermissionsToRole(data.RoleId || data.roleId, data.ModuleId || data.moduleId, [data.PermissionId || data.permissionId], data.TenantId || data.tenantId); break;
                case 'delete_roleModPerm': res = await securityApi.deleteRoleModulePermission(data.id || data); break;

                // User Role Link
                case 'delete_userRole': res = await securityApi.removeUserRole(data.userId || (data as any).UserId, data.roleId || (data as any).RoleId); break;

                default: toast.error("Unknown Entity Action Unit"); break;
            }

            if (extractData(res)) {
                toast.success(`${entity.toUpperCase()} operation completed successfully.`);
                setUi((prev: any) => ({ ...prev, modal: null, target: null }));
                syncAll();
            } else {
                toast.error(res?.message || "Operation failed at server level.");
            }
        } catch (err: any) {
            toast.error(err.message || "Network Synchronization Error.");
        } finally {
            setUi((prev: any) => ({ ...prev, loading: false }));
        }
    };

    return {
        HandleCRUD,
        syncAll,
        syncOrgs,
        syncUsers,
        syncCourses,
        syncCmCourses,
        syncCats,
        syncGroups,
        syncRoles,
        syncModules,
        syncPermissions,
        syncModPerms,
        syncRoleModules,
        syncRoleModPerms,
        syncUserRoles,
        fetchOrgAdmin,
        getOrgNameByTenant
    };
};
