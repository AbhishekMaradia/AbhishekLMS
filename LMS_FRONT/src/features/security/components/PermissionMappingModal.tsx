import React from 'react';
import { Icons } from '../../../shared/components/lms/Icons';
import '../Security.css';

interface PermissionMappingModalProps {
    ui: any;
    setUi: (val: any) => void;
    user: any;
    isSuperAdmin: boolean;
    db: any;
    setDb: (val: any) => void;
    formTenantId: number | null;
    setFormTenantId: (val: number | null) => void;
    handleCrud: (action: string, type: string, data?: any) => Promise<void>;
    securityApi: any;
    extractData: (res: any) => any;
    setPm: (val: any) => void;
    openModPM: (m: any, r?: any, tId?: number | null) => Promise<void>;
    toast: any;
    permissions: any;
}

export const PermissionMappingModal: React.FC<PermissionMappingModalProps> = ({
    ui,
    setUi,
    user,
    isSuperAdmin,
    db,
    setDb,
    formTenantId,
    setFormTenantId,
    handleCrud,
    securityApi,
    extractData,
    setPm,
    openModPM,
    toast,
    permissions
}) => {
    // Utility: Normalize module codes by stripping 'MOD_' prefix
    const normalizeCode = (code: string) => {
        const c = String(code || '').toUpperCase();
        return c.startsWith('MOD_') ? c.substring(4) : c;
    };

    // Determine allotted modules based on user permissions
    const getMyModuleCodes = () => {
        if (!permissions) return [];
        let keys: string[] = [];

        // Handle case where permissions might be an array of objects
        if (Array.isArray(permissions)) {
            keys = permissions.map(p => (p.moduleCode || p.ModuleCode || '').toUpperCase());
        }
        // Handle case where permissions is the decrypted map { MODULE_CODE: [PERM_CODES] } or object
        else if (typeof permissions === 'object') {
            keys = Object.keys(permissions).map(k => k.toUpperCase());
        }

        // Strip "MOD_" prefix to match standard module codes
        return keys.map(k => normalizeCode(k));
    };

    const myModuleCodes = getMyModuleCodes();

    // Role Module Create (Assign Module to Role)
    if (ui.modal === 'role_module_create') {
        return (
            <form className="lms-fade-in lms-col-gap-md" onSubmit={(e) => {
                e.preventDefault();
                const fd = new FormData(e.currentTarget);
                const data = {
                    roleId: Number(fd.get('RoleId')),
                    moduleId: Number(fd.get('ModuleId'))
                };
                handleCrud('create', 'roleModule', data);
                // The orchestrator's handleCrud will close the modal and sync.
                // We trust the orchestrator to resolve the UI state.
            }}>
                {isSuperAdmin ? (
                    <>
                        <label className="lms-label-premium">Organization</label>
                        <div className="lms-modal-panel-dashed">
                            <select
                                name="TenantId"
                                value={formTenantId ?? ""}
                                onChange={(e) => setFormTenantId(e.target.value === "" ? null : Number(e.target.value))}
                                className="lms-select-premium"
                            >
                                <option value="">Super Admin (Global)</option>
                                {db.orgs.filter((o: any) => (o.isActive ?? o.IsActive) !== false).map((o: any) => <option key={o.id || o.Id} value={o.id || o.Id}>{o.orgName || o.OrgName}</option>)}
                            </select>
                        </div>
                    </>
                ) : (
                    <input type="hidden" name="TenantId" value={user?.tenantId ?? (user as any).TenantId ?? 0} />
                )}

                <label className="lms-label-premium required">Role</label>
                <div className="lms-modal-panel-premium">
                    <select name="RoleId" className="lms-select-premium" required>
                        <option value="">-- Choose Role --</option>
                        {db.roles.filter((r: any) => {
                            const target = formTenantId ?? ((user as any).tenantId ?? (user as any).TenantId ?? null);
                            const isActive = (r.isActive ?? r.IsActive) !== false;
                            if (target === null) return isActive && (r.tenantId === 0 || r.tenantId === null);
                            return isActive && Number(r.tenantId) === Number(target);
                        }).map((r: any) => <option key={r.id} value={r.id}>{r.name} ({r.code})</option>)}
                    </select>
                </div>

                <label className="lms-label-premium required">TARGET MODULE *</label>
                <div className="lms-modal-panel-premium">
                    <select name="ModuleId" className="lms-select-premium" required>
                        <option value="">-- Choose Module --</option>
                        {(() => {
                            if (isSuperAdmin) {
                                return (db.modules || []).filter((m: any) => (m.isActive ?? m.IsActive) !== false).map((m: any) => <option key={m.id} value={m.id}>{m.name} ({m.code})</option>);
                            }
                            return (db.modules || []).filter((m: any) =>
                                (m.isActive ?? m.IsActive) !== false && myModuleCodes.includes(normalizeCode(m.code || m.Code))
                            ).map((m: any) => (
                                <option key={m.id} value={m.id}>{m.name} ({m.code})</option>
                            ));
                        })()}
                    </select>
                </div>

                <button type="submit" disabled={ui.loading} className="lms-btn-commit">
                    {ui.loading ? 'Synchronizing...' : 'Next: Configure Permissions'}
                </button>
            </form>
        );
    }

    // Module Permission Assign (Map Perms to Module)
    if (ui.modal === 'mod_perm_assign') {
        return (
            <form className="lms-fade-in lms-col-gap-md" onSubmit={(e) => {
                e.preventDefault();
                const fd = new FormData(e.currentTarget);
                const mId = Number(fd.get('ModuleId'));
                const module = db.modules.find((m: any) => (m.id || m.Id) === mId);
                if (module) {
                    openModPM(module);
                }
            }}>
                <label className="lms-label-premium required">TARGET MODULE *</label>
                <div className="lms-modal-panel-premium">
                    <select name="ModuleId" className="lms-select-premium" required defaultValue={ui.target?.id || ui.target?.Id || ""}>
                        <option value="">-- Select Module --</option>
                        {(() => {
                            if (isSuperAdmin) {
                                return (db.modules || []).filter((m: any) => (m.isActive ?? m.IsActive) !== false).map((m: any) => <option key={m.id} value={m.id}>{m.name} ({m.code})</option>);
                            }
                            return (db.modules || []).filter((m: any) =>
                                (m.isActive ?? m.IsActive) !== false && myModuleCodes.includes(normalizeCode(m.code || m.Code))
                            ).map((m: any) => (
                                <option key={m.id} value={m.id}>{m.name} ({m.code})</option>
                            ));
                        })()}
                    </select>
                </div>

                <button type="submit" className="lms-btn-commit">
                    Next
                </button>
            </form>
        );
    }

    // Role Module Permission Assign
    if (ui.modal === 'role_mod_perm_assign' || ui.modal === 'role_mod_assign') {
        return (
            <form className="lms-fade-in lms-col-gap-md" onSubmit={async (e) => {
                e.preventDefault();
                const fd = new FormData(e.currentTarget);
                const rId = Number(fd.get('RoleId'));
                const mId = Number(fd.get('ModuleId'));
                const tstr = fd.get('TenantId');
                const tId = tstr ? Number(tstr) : null;
                const role = db.roles.find((r: any) => (r.id || r.Id) === rId);
                const module = db.modules.find((m: any) => (m.id || m.Id) === mId);

                if (role && module) {
                    openModPM(module, role, tId);
                }
            }}>
                {isSuperAdmin ? (
                    <>
                        <label className="lms-label-premium">Organization</label>
                        <div className="lms-modal-panel-dashed lms-pm-modal-org-panel">
                            <select
                                name="TenantId"
                                value={formTenantId ?? ""}
                                onChange={(e) => setFormTenantId(e.target.value === "" ? null : Number(e.target.value))}
                                className="lms-select-premium"
                            >
                                <option value="">Super Admin (Global)</option>
                                {db.orgs.filter((o: any) => (o.isActive ?? o.IsActive) !== false).map((o: any) => <option key={o.id || o.Id} value={o.id || o.Id}>{o.orgName || o.OrgName}</option>)}
                            </select>
                        </div>
                    </>
                ) : (
                    <input type="hidden" name="TenantId" value={user?.tenantId ?? (user as any).TenantId ?? 0} />
                )}

                <div className="lms-form-grid lms-pm-modal-grid">
                    <div>
                        <label className="lms-label-premium required">Role *</label>
                        <div className="lms-modal-panel-premium">
                            <select name="RoleId" className="lms-select-premium" required defaultValue={ui.target?.roleId || ui.target?.RoleId || ui.target?.id || ui.target?.Id || ""}>
                                <option value="">-- Select Role --</option>
                                {db.roles.filter((r: any) => {
                                    const rTid = r.TenantId ?? r.tenantId ?? 0;
                                    const isActive = (r.isActive ?? r.IsActive) !== false;
                                    if (!isActive) return false;
                                    const target = formTenantId ?? ((user as any).tenantId ?? (user as any).TenantId ?? 0);
                                    if (target === 0 || target === null || target === undefined) return rTid === 0 || rTid === null || rTid === undefined;
                                    return Number(rTid) === Number(target);
                                }).map((r: any) => (
                                    <option key={r.id || r.Id} value={r.id || r.Id}>
                                        {r.name || r.Name} ({r.code || r.Code})
                                    </option>
                                ))}
                            </select>
                        </div>
                    </div>

                    <div>
                        <label className="lms-label-premium required">Module *</label>
                        <div className="lms-modal-panel-premium">
                            <select name="ModuleId" className="lms-select-premium" required>
                                <option value="">-- Select Module --</option>
                                {(() => {
                                    if (isSuperAdmin) {
                                        return (db.modules || []).filter((m: any) => (m.isActive ?? m.IsActive) !== false).map((m: any) => (
                                            <option key={m.id || m.Id} value={m.id || m.Id}>{m.name || m.Name} ({m.code || m.Code})</option>
                                        ));
                                    }
                                    const allottedModules = (db.modules || []).filter((m: any) =>
                                        (m.isActive ?? m.IsActive) !== false && myModuleCodes.includes(normalizeCode(m.code || m.Code))
                                    );
                                    if (allottedModules.length === 0) return <option disabled>No modules available</option>;
                                    return allottedModules.map((m: any) => (
                                        <option key={m.id || m.Id} value={m.id || m.Id}>{m.name || m.Name} ({m.code || m.Code})</option>
                                    ));
                                })()}
                            </select>
                        </div>
                    </div>
                </div>

                <button type="submit" className="lms-btn-commit">
                    Next
                </button>
            </form>
        );
    }

    // Role Module Permission View (Inspector)
    if (ui.modal === 'roleModPerm_view') {
        return (
            <div className="lms-col-gap-md lms-fade-in lms-pm-modal-container">
                {ui.loading ? (
                    <div className="lms-modal-panel-premium lms-pm-modal-center-panel">
                        <div className="lms-loader-spinner lms-pm-modal-spinner"></div>
                        <span className="lms-pm-modal-status-text">Analyzing Security context...</span>
                    </div>
                ) : ui.target ? (
                    <div className="lms-col-gap-md">
                        <label className="lms-label-premium">Role</label>
                        <div className="lms-modal-panel-premium lms-pm-modal-info-panel">
                            <h3 className="lms-pm-modal-info-heading">{ui.target.roleName}</h3>
                            <div className="lms-pm-modal-info-sub">CODE: {ui.target.roleCode}</div>
                        </div>
                        <label className="lms-label-premium">Module</label>
                        <div className="lms-modal-panel-premium lms-pm-modal-info-panel">
                            <h3 className="lms-pm-modal-info-heading">{ui.target.moduleName}</h3>
                            <div className="lms-pm-modal-info-sub">CODE: {ui.target.moduleCode}</div>
                        </div>
                        <label className="lms-label-premium">Permission</label>
                        <div className="lms-modal-panel-premium lms-pm-modal-info-panel">
                            <h3 className="lms-pm-modal-info-heading">{ui.target.permissionName}</h3>
                            <div className="lms-pm-modal-info-sub">CODE: {ui.target.permissionCode}</div>
                        </div>

                        <button onClick={() => setUi({ ...ui, modal: null })} className="lms-btn-commit">
                            Close
                        </button>
                    </div>
                ) : (
                    <div className="lms-modal-panel-dashed lms-pm-modal-center-panel">
                        <Icons.Alert s={40} />
                        <h3 className="lms-pm-modal-error-heading">Context Missing</h3>
                        <p className="lms-pm-modal-error-text">Failed to retrieve the requested security mapping details.</p>
                    </div>
                )}
            </div>
        );
    }

    return null;
};
