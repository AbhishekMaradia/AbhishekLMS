import React from 'react';
import { Icons } from '../../../shared/components/lms/Icons';
import '../Security.css';

interface PermissionMatrixProps {
    pm: any;
    setPm: (val: any) => void;
    pmSearch: string;
    setPmSearch: (val: string) => void;
    isSuperAdmin: boolean;
    togglePermission: (id: number) => void;
    savePermissions: () => Promise<void>;
    permissions: any;
    db?: any;
    onModuleChange?: (m: any) => void;
}

export const PermissionMatrix: React.FC<PermissionMatrixProps> = ({
    pm,
    setPm,
    pmSearch,
    setPmSearch,
    isSuperAdmin,
    togglePermission,
    savePermissions,
    permissions,
    db = { modules: [] },
    onModuleChange
}) => {
    if (!pm.open) return null;

    // Determine what actions the user is actually allowed to assign
    const getMyActionsForModule = () => {
        if (isSuperAdmin) return null; // Unrestricted
        if (!permissions) return [];

        const normalize = (code: string) => String(code || '').toUpperCase().replace(/^MOD_/, '');
        const targetMCode = normalize(pm.module?.code || pm.module?.Code);

        // Find permissions for this module in our session
        let sessionActions: string[] = [];

        if (Array.isArray(permissions)) {
            const modPerms = permissions.find(p => {
                const pc = normalize(p.moduleCode || p.ModuleCode);
                return pc === targetMCode;
            });
            sessionActions = (modPerms?.permissions || modPerms?.Permissions || []);
        } else {
            // Check both standard and MOD_ prefixed keys in the record
            sessionActions = permissions[targetMCode] || permissions[`MOD_${targetMCode}`] || [];
        }

        return sessionActions.map((a: string) => String(a).toUpperCase());
    };

    const myActions = getMyActionsForModule();

    return (
        <div className="lms-modal-overlay lms-pm-overlay" onClick={() => setPm((prev: any) => ({ ...prev, open: false }))}>
            <div
                className="lms-modal-content lms-pm-content"
                onClick={(e) => e.stopPropagation()}
                role="dialog"
                aria-modal="true"
            >

                <div className="lms-modal-header lms-pm-header">
                    <div className="lms-flex-row lms-pm-header-flex">
                        <div className="lms-status-icon accent lms-pm-lock-icon">
                            <Icons.Lock s={24} />
                        </div>
                        <div className="lms-modal-heading-stack lms-pm-heading-container">
                            <h3 className="lms-modal-title lms-pm-title">
                                {pm.role ? (pm.role.name || pm.role.Name || 'Role') : (pm.module?.name || pm.module?.Name || 'Module')}
                            </h3>
                            <div className="lms-flex-row lms-pm-subtitle-flex">
                                <span className="lms-status-sub lms-pm-badge">
                                    PERMISSIONS
                                </span>
                                {pm.role && (
                                    <span className="lms-pm-module-name-hint">
                                        — {pm.module?.name || pm.module?.Name}
                                    </span>
                                )}
                            </div>
                        </div>
                    </div>
                    <button type="button" onClick={() => setPm((prev: any) => ({ ...prev, open: false }))} className="lms-modal-close lms-pm-close-btn">
                        ✕
                    </button>
                </div>

                <div className="lms-pm-toolbar">
                    <div className="lms-flex-row lms-pm-toolbar-grid">
                        <div className="lms-pm-input-wrapper">
                            <input
                                className="lms-input-premium lms-pm-search-input"
                                placeholder="Filter security nodes..."
                                value={pmSearch}
                                onChange={(e) => setPmSearch(e.target.value)}
                            />
                            <div className="lms-pm-search-icon">
                                <Icons.Search s={18} />
                            </div>
                        </div>

                        {pm.role && (
                            <div className="lms-pm-select-wrapper">
                                <select 
                                    className="lms-select-premium lms-pm-select"
                                    value={pm.module?.id || pm.module?.Id || ""}
                                    onChange={(e) => {
                                        const m = db.modules.find((mod: any) => (mod.id || mod.Id) === Number(e.target.value));
                                        if (m) onModuleChange?.(m);
                                    }}
                                >
                                    {(db.modules || []).filter((m: any) => (m.isActive ?? m.IsActive) !== false).map((m: any) => (
                                        <option key={m.id || m.Id} value={m.id || m.Id}>{m.name || m.Name}</option>
                                    ))}
                                </select>
                            </div>
                        )}

                        {(() => {
                            const filtered = pm.mPerms.filter((p: any) => {
                                const searchMatch = (p.name || p.Name || "").toLowerCase().includes(pmSearch.toLowerCase()) || 
                                                   (p.code || p.Code || p.permissionCode || "").toLowerCase().includes(pmSearch.toLowerCase());
                                if (!searchMatch) return false;
                                if (isSuperAdmin) return true;
                                return myActions && myActions.length > 0;
                            });

                            const allVisibleIds = filtered.map((p: any) => Number(p.id || p.Id || p.permissionId || p.PermissionId));
                            const isAllSelected = allVisibleIds.length > 0 && allVisibleIds.every(id => pm.rPerms.includes(id));

                            const handleToggleAll = () => {
                                if (isAllSelected) {
                                    setPm({ ...pm, rPerms: pm.rPerms.filter((id: number) => !allVisibleIds.includes(id)) });
                                } else {
                                    setPm({ ...pm, rPerms: Array.from(new Set([...pm.rPerms, ...allVisibleIds])) });
                                }
                            };

                            return (
                                <div className="lms-flex-row lms-pm-master-toggle" onClick={handleToggleAll}>
                                    <div className={`lms-pm-checkbox master ${isAllSelected ? 'active' : ''}`}>
                                        {isAllSelected ? <Icons.Check s={12} /> : null}
                                    </div>
                                    <span className="lms-pm-master-label">SELECT ALL</span>
                                </div>
                            );
                        })()}
                    </div>
                </div>

                <div className="lms-pm-body">
                    {pm.loading ? (
                        <div className="lms-pm-loading-container">
                            <div className="lms-loader-spinner lms-pm-spinner"></div>
                            <div className="lms-pm-loading-text">Synchronizing Security Nodes...</div>
                        </div>
                    ) : pm.mPerms.length === 0 ? (
                        <div className="lms-empty-state lms-pm-empty-container">
                            <div className="lms-pm-empty-icon">🛡️</div>
                            <h4 className="lms-pm-empty-title">No Permissions Loaded</h4>
                            <p className="lms-pm-empty-subtitle">Create permissions first to populate this matrix.</p>
                        </div>
                    ) : (
                        <div className="lms-pm-grid">
                            {(() => {
                                const filtered = pm.mPerms.filter((p: any) => {
                                    const searchMatch = (p.name || p.Name || "").toLowerCase().includes(pmSearch.toLowerCase()) || 
                                                       (p.code || p.Code || p.permissionCode || "").toLowerCase().includes(pmSearch.toLowerCase());
                                    if (!searchMatch) return false;
                                    
                                    // If superadmin, show everything that passed search
                                    if (isSuperAdmin) return true;
                                    
                                    // For Org Admins, if they have permissions for this module, 
                                    // let them see the module's defined nodes.
                                    return myActions && myActions.length > 0;
                                });

                                return filtered.map((p: any) => {
                                    const id = Number(p.id || p.Id || p.permissionId || p.PermissionId);
                                    const isActive = pm.rPerms.includes(id);
                                    return (
                                        <div
                                            key={id}
                                            onClick={() => togglePermission(id)}
                                            className={`lms-pm-card ${isActive ? 'active' : ''}`}
                                        >
                                            <div className={`lms-pm-checkbox ${isActive ? 'active' : ''}`}>
                                                {isActive ? <Icons.Check s={12} /> : null}
                                            </div>
                                            <div className="lms-pm-card-text-col">
                                                <div className="lms-pm-card-title">
                                                    {p.name || p.Name || p.permissionName}
                                                </div>
                                                <div className="lms-pm-card-code">
                                                    {p.code || p.Code || p.permissionCode}
                                                </div>
                                            </div>
                                        </div>
                                    );
                                });
                            })()}
                        </div>
                    )}
                </div>

                <div className="lms-modal-footer lms-pm-footer">
                    <button type="button" onClick={() => setPm({ ...pm, open: false })} className="lms-btn lms-pm-cancel-btn">Cancel</button>
                    <button
                        type="button"
                        onClick={savePermissions}
                        disabled={pm.loading}
                        className="lms-btn-primary lms-pm-commit-btn"
                    >
                        {pm.loading ? 'Saving...' : 'Commit Changes'}
                    </button>
                </div>
            </div>
        </div>
    );
};
