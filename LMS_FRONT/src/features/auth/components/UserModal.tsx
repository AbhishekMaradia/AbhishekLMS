import React from 'react';
import { Icons } from '../../../shared/components/lms/Icons';
import '../Auth.css';

interface UserModalProps {
    ui: any;
    user: any;
    isSuperAdmin: boolean;
    db: any;
    formTenantId: number | null;
    setFormTenantId: (val: number | null) => void;
    handleCrud: (action: string, entity: string, data: any) => void;
    hasPermission: (module: string, permission: string) => boolean;
}

export const UserModal: React.FC<UserModalProps> = ({
    ui,
    user,
    isSuperAdmin,
    db,
    formTenantId,
    setFormTenantId,
    handleCrud,
    hasPermission
}) => {
    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        const fd = new FormData(e.currentTarget);
        const rawData: any = Object.fromEntries(fd.entries());
        const isActiveCheckbox = e.currentTarget.elements.namedItem('IsActive') as HTMLInputElement;

        const getVal = (key: string) => {
            let val = rawData[key];

            // Fallback for fields not present in the DOM (e.g. hidden for self-edit)
            if (val === undefined && ui.target) {
                const t = ui.target;
                if (key === 'TenantId') val = t.tenantId ?? t.TenantId;
                else if (key === 'RoleId') val = (t.roleIds?.[0] || t.roleId || t.RoleId);
                else if (key === 'GroupId') val = t.groupId ?? t.GroupId;
            }

            if (val === "" || val === undefined || val === null) return null;
            const n = Number(val);
            if (isNaN(n) || n === 0) return null;
            return n;
        };

        const selectedRoleId = getVal('RoleId');
        const roleIds = selectedRoleId ? [selectedRoleId] : [];

        const data: any = {
            FirstName: rawData.FirstName || rawData.firstName || ui.target?.firstName || ui.target?.FirstName,
            LastName: rawData.LastName || rawData.lastName || ui.target?.lastName || ui.target?.LastName,
            Email: rawData.Email || rawData.email || ui.target?.email || ui.target?.Email,
            Mobile: rawData.Mobile || rawData.mobile || ui.target?.mobile || ui.target?.Mobile,
            TenantId: getVal('TenantId'),
            RoleIds: roleIds,
            GroupId: getVal('GroupId'),
            IsActive: isActiveCheckbox ? isActiveCheckbox.checked : (ui.target ? (ui.target.isActive !== false && ui.target.IsActive !== false) : true),
        };

        // Only include password if it's provided (don't overwrite with empty string)
        if (rawData.Password) {
            data.Password = rawData.Password;
        }

        if (ui.modal === 'user_edit' || ui.modal === 'user_update') {
            data.Id = Number(rawData.Id || rawData.id || ui.target?.id || ui.target?.Id) || 0;
        }

        const op = ui.modal.split('_')[1];
        const action = (op === 'edit' || op === 'update') ? 'update' : 'create';
        handleCrud(action, 'user', data);
    };

    const targetTenantId = formTenantId ?? ((user as any).tenantId ?? (user as any).TenantId ?? null);

    const targetUserId = ui.target?.id || ui.target?.Id;
    const currentUserId = user?.id || user?.Id;
    const isCurrentUser = Boolean(targetUserId && currentUserId && Number(targetUserId) === Number(currentUserId));

    return (
        <form onSubmit={handleSubmit} className="lms-fade-in lms-col-gap-md lms-user-modal-form" autoComplete="off">
            {/* Decoy fields to catch aggressive browser autofill */}
            <div style={{ position: 'absolute', top: '-9999px', left: '-9999px', width: '1px', height: '1px', opacity: 0.01, overflow: 'hidden', pointerEvents: 'none' }}>
                <input type="text" name="fake_username_autofill" tabIndex={-1} autoComplete="off" style={{ width: '1px', height: '1px' }} />
                <input type="email" name="fake_email_autofill" tabIndex={-1} autoComplete="off" style={{ width: '1px', height: '1px' }} />
                <input type="password" name="fake_password_autofill" tabIndex={-1} autoComplete="new-password" style={{ width: '1px', height: '1px' }} />
            </div>

            {ui.modal !== 'user_create' && <input type="hidden" name="Id" value={ui.target?.id || ui.target?.Id || 0} />}

            {isSuperAdmin && !isCurrentUser && (
                <>
                    <label className="lms-label-premium required">Organization</label>
                    <div className="lms-modal-panel-premium">
                        <select
                            name="TenantId"
                            key={ui.modal + (ui.target?.id || 'new')}
                            defaultValue={ui.target?.tenantId ?? ui.target?.TenantId ?? 0}
                            className="lms-select-premium"
                            onChange={(e) => setFormTenantId(Number(e.target.value))}
                            required
                        >
                            <option value={0}>Super Admin (Global)</option>
                            {(db?.orgs || []).filter((o: any) => (o.isActive ?? o.IsActive) !== false).map((o: any) => <option key={o.id || o.Id} value={o.id || o.Id}>{o.orgName || o.OrgName}</option>)}
                        </select>
                    </div>
                </>
            )}
            {(!isSuperAdmin || isCurrentUser) && <input type="hidden" name="TenantId" value={ui.target?.tenantId ?? (ui.target as any)?.TenantId ?? (user?.tenantId ?? (user as any)?.TenantId ?? 0)} />}

            <div className="lms-form-grid lms-user-modal-grid">
                <div>
                    <label className="lms-label-premium required">First Name</label>
                    <div className="lms-modal-panel-premium">
                        <input name="FirstName" defaultValue={ui.target?.firstName || ui.target?.FirstName} placeholder="e.g. John" className="lms-input-premium" required />
                    </div>
                </div>
                <div>
                    <label className="lms-label-premium required">Last Name</label>
                    <div className="lms-modal-panel-premium">
                        <input name="LastName" defaultValue={ui.target?.lastName || ui.target?.LastName} placeholder="e.g. Doe" className="lms-input-premium" required />
                    </div>
                </div>
            </div>

            <div className="lms-form-grid lms-user-modal-grid">
                <div>
                    <label className="lms-label-premium required">Email Address</label>
                    <div className="lms-modal-panel-premium">
                        <input name="Email" type="email" autoComplete="new-email" defaultValue={ui.target?.email || ui.target?.Email} placeholder="john@example.com" className="lms-input-premium" required />
                    </div>
                </div>
                <div>
                    <label className="lms-label-premium required">Mobile Number</label>
                    <div className="lms-modal-panel-premium">
                        <input name="Mobile" defaultValue={ui.target?.mobile || ui.target?.Mobile} placeholder="+1..." className="lms-input-premium" required />
                    </div>
                </div>
            </div>

            <label className="lms-label-premium">{ui.modal === 'user_create' ? 'Initial Password' : 'Change Password'}</label>
            <div className="lms-modal-panel-premium">
                <input name="Password" type="password" autoComplete="new-password" placeholder={ui.modal !== 'user_create' ? '•••••••• (Leave blank to keep current)' : 'Enter password'} className="lms-input-premium" required={ui.modal === 'user_create'} />
            </div>

            {((isSuperAdmin || hasPermission('USER_ROLE', 'USER_ROLE_ADD')) && !isCurrentUser) && (
                <div className="lms-form-grid lms-user-modal-grid">
                    <div>
                        <label className="lms-label-premium required">Assigned Role</label>
                        <div className="lms-modal-panel-premium">
                            <select name="RoleId" defaultValue={ui.target?.roleIds?.[0] || ui.target?.roleId || ui.target?.RoleId} className="lms-select-premium" required>
                                <option value="">-- No Role --</option>
                                {(db?.roles || [])
                                    .filter((r: any) => (r.tenantId ?? r.TenantId ?? 0) === Number(targetTenantId))
                                    .map((r: any) => <option key={r.id || r.Id} value={r.id || r.Id}>{r.name || r.Name}</option>)
                                }
                            </select>
                        </div>
                    </div>
                </div>
            )}

            {!isCurrentUser && (
                <div className="lms-switch-premium lms-user-modal-switch-wrap">
                    <span className="lms-user-modal-status-label">Status</span>
                    <input type="checkbox" name="IsActive" defaultChecked={ui.target ? (ui.target.isActive !== false) : true} className="lms-user-modal-status-checkbox" />
                </div>
            )}

            <button type="submit" disabled={ui.loading} className="lms-btn-commit lms-user-modal-submit">
                {ui.loading ? 'Saving...' : (ui.modal === 'user_create' ? 'Create User' : 'Save Changes')}
            </button>
        </form>

    );
};
