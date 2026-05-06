import React, { useState } from 'react';
import { Icons } from '../../../shared/components/lms/Icons';
import '../Security.css';

interface UserRoleModalProps {
    ui: any;
    user: any;
    isSuperAdmin: boolean;
    db: any;
    handleCrud: (action: string, type: string, data?: any) => Promise<void>;
    securityApi: any;
    extractData: (res: any) => any;
    toast: any;
    syncUserRoles: () => Promise<void>;
    setUi: (val: any) => void;
}

export const UserRoleModal: React.FC<UserRoleModalProps> = ({
    ui,
    user,
    isSuperAdmin,
    db,
    handleCrud,
    securityApi,
    extractData,
    toast,
    syncUserRoles,
    setUi
}) => {
    const [selectedTenant, setSelectedTenant] = useState<number | null>(null);

    // User Assign (Legacy single role assign)
    if (ui.modal === 'user_assign') {
        const target = ui.target;
        return (
            <form key={target?.id} className="lms-fade-in lms-col-gap-md" onSubmit={(e) => {
                e.preventDefault();
                const RoleId = Number((e.currentTarget.elements.namedItem('RoleId') as HTMLSelectElement).value);
                handleCrud('assign', 'user', { userId: target.id || target.Id, roleId: RoleId });
            }}>
                <label className="lms-label-premium">Profile Node</label>
                <div className="lms-modal-panel-premium lms-user-role-panel-flex">
                    <h4 className="lms-user-role-heading">{target.firstName || target.FirstName} {target.lastName || target.LastName}</h4>
                    {target.userRole && <div className="lms-tag accent lms-user-role-tag">Current: {target.userRole}</div>}
                </div>

                <label className="lms-label-premium required">Role</label>
                <div className="lms-modal-panel-premium">
                    <select
                        name="RoleId"
                        defaultValue={db.roles.find((r: any) => (r.name || '').toLowerCase() === (target?.userRole || '').toLowerCase() || (r.code || '').toLowerCase() === (target?.userRole || '').toLowerCase())?.id || ""}
                        className="lms-select-premium"
                        required
                    >
                        <option value="">-- Choose Role --</option>
                        {db.roles.filter((r: any) => {
                            const rTenant = r.tenantId ?? r.TenantId;
                            const userTenant = (user as any).tenantId ?? (user as any).TenantId;
                            const isActive = (r.isActive ?? r.IsActive) !== false;
                            return isActive && (isSuperAdmin || rTenant === null || rTenant === 0 || Number(rTenant) === Number(userTenant));
                        }).map((r: any) => (
                            <option key={r.id || r.Id} value={r.id || r.Id}>{r.name || r.Name} ({r.code || r.Code})</option>
                        ))}
                    </select>
                </div>

                <button type="submit" disabled={ui.loading} className="lms-btn-commit">
                    {ui.loading ? 'Saving...' : 'Save Changes'}
                </button>
            </form>
        );
    }

    const [selectedUserId, setSelectedUserId] = useState<number | null>(null);
    const [selectedRoleId, setSelectedRoleId] = useState<number | null>(null);

    // Role Assign (Bulk style)
    if (ui.modal === 'user_role_assign') {
        const targetTenant = selectedTenant ?? (user?.tenantId ?? user?.TenantId ?? 0);

        const targetUser = db.users.find((u: any) => Number(u.id || u.Id) === Number(selectedUserId));
        const userRoleIds = targetUser?.roleIds || (targetUser?.roleId ? [targetUser.roleId] : []);
        const alreadyHasRole = selectedRoleId && userRoleIds.includes(Number(selectedRoleId));

        return (
            <form className="lms-fade-in lms-col-gap-md" onSubmit={async (e) => {
                e.preventDefault();
                const fd = new FormData(e.currentTarget);
                const uid = Number(fd.get('UserId'));
                const rid = Number(fd.get('RoleId'));
                if (!uid || !rid) { toast.error("Please select both User and Role"); return; }
                if (alreadyHasRole) { toast.warning("User already holds this security role!"); return; }

                setUi({ ...ui, loading: true });
                try {
                    await securityApi.assignUserRole(uid, rid);
                    toast.success("Role Assigned Successfully!");
                    setUi({ ...ui, modal: null, loading: false });
                    syncUserRoles();
                } catch (err: any) {
                    toast.error(err.message || "Assignment failed");
                    setUi({ ...ui, loading: false });
                }
            }}>
                {isSuperAdmin && (
                    <>
                        <label className="lms-label-premium">Organization Filter</label>
                        <div className="lms-modal-panel-premium">
                            <select
                                className="lms-select-premium lms-user-role-select-flat"
                                onChange={(e) => {
                                    setSelectedTenant(e.target.value === "" ? null : Number(e.target.value));
                                    setSelectedUserId(null); // Reset user when tenant changes
                                }}
                                value={selectedTenant ?? ""}
                            >
                                <option value="">Super Admin</option>
                                <option value={0}>Super Admin</option>
                                {db.orgs.filter((o: any) => (o.isActive ?? o.IsActive) !== false).map((o: any) => <option key={o.id || o.Id} value={o.id || o.Id}>{o.orgName || o.OrgName}</option>)}
                            </select>
                        </div>
                    </>
                )}

                <label className="lms-label-premium required">User</label>
                <div className={`lms-modal-panel-premium ${selectedUserId ? 'active' : ''}`}>
                    <select
                        name="UserId"
                        className="lms-select-premium"
                        required
                        onChange={(e) => setSelectedUserId(Number(e.target.value))}
                        value={selectedUserId ?? ""}
                    >
                        <option value="">-- Choose User --</option>
                        {db.users.filter((u: any) => {
                            const uid = u.id || u.Id;
                            const currentUid = user?.id || user?.Id;
                            const isSelf = Number(uid) === Number(currentUid);
                            const isActive = u.isActive !== false;
                            const uTenant = u.tenantId ?? u.TenantId ?? u.orgId ?? u.OrgId;
                            const hasOrgAccess = isSuperAdmin
                                ? (selectedTenant === null || Number(uTenant) === Number(selectedTenant))
                                : Number(uTenant) === Number(targetTenant);
                            return !isSelf && isActive && hasOrgAccess;
                        }).map((u: any) => (
                            <option key={u.id || u.Id} value={u.id || u.Id}>
                                {u.firstName || u.FirstName} {u.lastName || u.LastName} ({u.email || u.Email})
                            </option>
                        ))}
                    </select>
                </div>

                <label className="lms-label-premium required">Role</label>
                <div className={`lms-modal-panel-premium ${selectedUserId ? 'active' : ''}`}>
                    <select
                        name="RoleId"
                        className="lms-select-premium"
                        required
                        onChange={(e) => setSelectedRoleId(Number(e.target.value))}
                        value={selectedRoleId ?? ""}
                    >
                        <option value="">-- Choose Role --</option>
                        {db.roles.filter((r: any) => {
                            const roleTenant = r.tenantId ?? r.TenantId;
                            const isActive = (r.isActive ?? r.IsActive) !== false;
                            if (!isActive) return false;
                            if (isSuperAdmin) return selectedTenant === null || Number(roleTenant) === Number(selectedTenant);
                            const isGlobal = roleTenant === null || roleTenant === 0 || roleTenant === undefined;
                            return isGlobal || Number(roleTenant) === Number(targetTenant);
                        }).map((r: any) => (
                            <option key={r.id || r.Id} value={r.id || r.Id}>
                                {r.name || r.Name} ({r.code || r.Code})
                            </option>
                        ))}
                    </select>
                </div>

                {alreadyHasRole && (
                    <div className="lms-alert-warning lms-fade-in lms-user-role-alert-box">
                        <Icons.Alert s={18} />
                        <span className="lms-user-role-alert-text">This user already possesses the selected security role.</span>
                    </div>
                )}

                <button type="submit" disabled={ui.loading || !!alreadyHasRole} className="lms-btn-commit">
                    {ui.loading ? 'Saving...' : 'Save Changes'}
                </button>
            </form>
        );
    }

    // Role Status Edit (Soft assign/revoke toggle)
    if (ui.modal === 'user_role_edit' && ui.target) {
        return (
            <form className="lms-fade-in lms-col-gap-md" onSubmit={async (e) => {
                e.preventDefault();
                const fd = new FormData(e.currentTarget);
                const userId = ui.target.userId || ui.target.UserId;
                const oldRoleId = ui.target.roleId || ui.target.RoleId;
                const newRoleId = Number(fd.get('RoleId'));
                const isActive = (e.currentTarget.elements.namedItem('IsActive') as HTMLInputElement).checked;

                setUi({ ...ui, loading: true });
                try {
                    if (newRoleId !== oldRoleId) {
                        await securityApi.removeUserRole(userId, oldRoleId);
                        await securityApi.assignUserRole(userId, newRoleId);
                        await securityApi.updateUserRoleStatus(userId, newRoleId, isActive);
                        toast.success("Role Reassigned!");
                    } else {
                        const res = await securityApi.updateUserRoleStatus(userId, oldRoleId, isActive);
                        if (extractData(res)) toast.success(`Status updated to ${isActive ? 'Active' : 'Inactive'}`);
                    }
                    setUi({ ...ui, modal: null, loading: false });
                    syncUserRoles();
                } catch (err: any) {
                    toast.error(err.message || "Update failed.");
                    setUi({ ...ui, loading: false });
                }
            }}>
                <label className="lms-label-premium">User Integrity</label>
                <div className="lms-modal-panel-premium lms-user-role-panel-flex sm">
                    <h3 className="lms-modal-title lms-user-role-title">{ui.target.userEmail || 'User'}</h3>
                    <div className="lms-tag info lms-user-role-tag xs">UPDATING SECURITY CONTEXT</div>
                </div>

                <label className="lms-label-premium required">Role</label>
                <div className="lms-modal-panel-premium">
                    <select
                        name="RoleId"
                        className="lms-select-premium"
                        defaultValue={ui.target.roleId || ui.target.RoleId}
                        required
                    >
                        <option value="">-- Choose Role --</option>
                        {db.roles.filter((r: any) => {
                            const targetT = ui.target?.tenantId ?? ui.target?.TenantId;
                            const roleT = r.tenantId ?? r.TenantId;
                            const isActive = (r.isActive ?? r.IsActive) !== false;
                            const isGlobal = !roleT || roleT === 0;
                            const isMatch = targetT && roleT === targetT;
                            return isActive && (isSuperAdmin || isGlobal || isMatch);
                        }).map((r: any) => (
                            <option key={r.id || r.Id} value={r.id || r.Id}>
                                {r.name || r.Name} {(!r.tenantId && !r.TenantId) ? '(Global)' : `(${r.orgName || 'Tenant'})`}
                            </option>
                        ))}
                    </select>
                </div>

                <div className="lms-switch-premium">
                    <span className="lms-user-role-switch-label">Status</span>
                    <input type="checkbox" name="IsActive" defaultChecked={ui.target.isActive !== false} className="lms-user-role-checkbox" />
                </div>

                <button type="submit" disabled={ui.loading} className="lms-btn-commit">
                    {ui.loading ? 'Saving...' : 'Save Changes'}
                </button>
            </form>
        );
    }

    return null;
};
