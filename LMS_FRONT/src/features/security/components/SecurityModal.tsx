import React from 'react';
import { Icons } from '../../../shared/components/lms/Icons';
import { securityApi } from '../../auth/api/securityApi';
import '../Security.css';

interface SecurityModalProps {
    ui: any;
    user: any;
    isSuperAdmin: boolean;
    db: any;
    setUi: (val: any) => void;
    sync: () => Promise<void>;
    handleCrud: (action: string, entity: string, data: any) => void;
    openModPM: (mod: any) => void;
    extractData: (res: any) => any;
}

export const SecurityModal: React.FC<SecurityModalProps> = ({
    ui,
    user,
    isSuperAdmin,
    db,
    setUi,
    sync,
    handleCrud,
    openModPM,
    extractData
}) => {
    if (ui.modal === 'role_create' || ui.modal === 'role_edit') {
        const op = ui.modal.split('_')[1];
        const action = (op === 'edit' || op === 'update') ? 'update' : 'create';
        return (
            <form className="lms-fade-in lms-col-gap-md" onSubmit={(e) => {
                e.preventDefault();
                const fd = new FormData(e.currentTarget);
                let data: any = Object.fromEntries(fd.entries());

                const body = {
                    name: data.Name || data.name,
                    code: data.Code || data.code,
                    isActive: (e.currentTarget.elements.namedItem('IsActive') as HTMLInputElement).checked,
                    tenantId: (isSuperAdmin && 'TenantId' in data)
                        ? ((Number(data.TenantId) === 0 || isNaN(Number(data.TenantId))) ? null : Number(data.TenantId))
                        : Number((user as any).tenantId ?? (user as any).TenantId ?? 0)
                };

                handleCrud(action, 'role', body);
            }}>
                {ui.modal === 'role_edit' && <input type="hidden" name="Id" value={ui.target?.id || ui.target?.Id || 0} />}

                <label className="lms-label-premium required">Name</label>
                <div className="lms-modal-panel-premium">
                    <input name="Name" defaultValue={ui.target?.name} placeholder="Name" className="lms-input-premium" required />
                </div>

                <label className="lms-label-premium required">Code</label>
                <div className="lms-modal-panel-premium">
                    <input name="Code" defaultValue={ui.target?.code} placeholder="Code" className="lms-input-premium" required readOnly={ui.modal === 'role_edit'} />
                </div>

                {isSuperAdmin && (
                    <>
                        <label className="lms-label-premium">Organization</label>
                        <div className="lms-modal-panel-premium">
                            <select
                                name="TenantId"
                                className="lms-select-premium lms-sec-modal-select-flat"
                                defaultValue={ui.target?.tenantId || ui.target?.TenantId || ""}
                            >
                                <option value="">Super Admin (Global)</option>
                                {db.orgs.filter((o: any) => (o.isActive ?? o.IsActive) !== false).map((o: any) => <option key={o.id || o.Id} value={o.id || o.Id}>{o.orgName || o.OrgName}</option>)}
                            </select>
                        </div>
                    </>
                )}

                <div className="lms-switch-premium lms-sec-modal-switch-wrapper">
                    <span className="lms-sec-modal-switch-label">Status</span>
                    <input
                        type="checkbox"
                        name="IsActive"
                        defaultChecked={ui.target ? ui.target.isActive : true}
                        className="lms-sec-modal-checkbox"
                    />
                </div>

                <button type="submit" disabled={ui.loading} className="lms-btn-commit">
                    {ui.loading ? 'Saving...' : 'Save'}
                </button>
            </form>
        );
    }

    // Permission Form
    if (ui.modal === 'perm_create' || ui.modal === 'perm_edit') {
        const op = ui.modal.split('_')[1];
        const action = (op === 'edit' || op === 'update') ? 'update' : 'create';
        return (
            <form className="lms-fade-in lms-col-gap-md" onSubmit={(e) => {
                e.preventDefault();
                const fd = new FormData(e.currentTarget);
                let data: any = Object.fromEntries(fd.entries());

                const body = {
                    name: data.Name || data.name,
                    code: data.Code || data.code,
                    isActive: (e.currentTarget.elements.namedItem('IsActive') as HTMLInputElement).checked
                };

                handleCrud(action, 'perm', body);
            }}>
                {ui.modal === 'perm_edit' && <input type="hidden" name="Id" value={ui.target?.id || ui.target?.Id || ui.target?.permissionId || 0} />}

                <label className="lms-label-premium required">Name</label>
                <div className="lms-modal-panel-premium">
                    <input name="Name" defaultValue={ui.target?.permissionName || ui.target?.name} placeholder="Name" className="lms-input-premium" required />
                </div>

                <label className="lms-label-premium required">Code</label>
                <div className="lms-modal-panel-premium">
                    <input name="Code" defaultValue={ui.target?.permissionCode || ui.target?.code} placeholder="Code" className="lms-input-premium" required readOnly={ui.modal === 'perm_edit'} />
                </div>

                <div className="lms-switch-premium lms-sec-modal-switch-wrapper">
                    <span className="lms-sec-modal-switch-label">Status</span>
                    <input
                        type="checkbox"
                        name="IsActive"
                        defaultChecked={ui.target ? ui.target.isActive : true}
                        className="lms-sec-modal-checkbox"
                    />
                </div>

                <button type="submit" disabled={ui.loading} className="lms-btn-commit">
                    {ui.loading ? 'Saving...' : 'Save'}
                </button>
            </form>
        );
    }

    // Module Form
    if (ui.modal === 'module_create' || ui.modal === 'module_edit') {
        return (
            <form className="lms-fade-in lms-col-gap-md" onSubmit={async (e) => {
                e.preventDefault();
                const fd = new FormData(e.currentTarget);
                let data: any = Object.fromEntries(fd.entries());
                setUi((prev: any) => ({ ...prev, loading: true }));
                try {
                    let moduleId = ui.target?.id || ui.target?.Id;
                    const isActive = (e.currentTarget.elements.namedItem('IsActive') as HTMLInputElement).checked;

                    if (ui.modal === 'module_create') {
                        const res: any = await securityApi.createModule({ name: data.Name, code: data.Code });
                        const body = res.data !== undefined ? res.data : (res.Data !== undefined ? res.Data : res);
                        moduleId = body?.id || body?.Id || res.id;
                    } else {
                        await securityApi.updateModule(moduleId, { name: data.Name, isActive: isActive } as any);
                    }

                    await sync();
                    setUi((prev: any) => ({ ...prev, modal: null, target: null, loading: false }));
                    if (moduleId) {
                        const modulesRes = (await securityApi.getModules('', 1, 1000));
                        const items = extractData(modulesRes);
                        const newMod = items.find((m: any) => (m.id || m.Id) === moduleId);
                        if (newMod) setTimeout(() => setUi({ ...ui, modal: 'mod_perm_assign', target: newMod, loading: false }), 300);
                    }
                } catch (err: any) {
                    alert(err.message || 'Module Sync Failed');
                    setUi((prev: any) => ({ ...prev, loading: false }));
                }
            }}>
                <label className="lms-label-premium required">Name</label>
                <div className="lms-modal-panel-premium">
                    <input name="Name" defaultValue={ui.target?.name} placeholder="Name" className="lms-input-premium" required />
                </div>

                <label className="lms-label-premium required">Code</label>
                <div className="lms-modal-panel-premium">
                    <input name="Code" defaultValue={ui.target?.code} placeholder="Code" className="lms-input-premium" required readOnly={ui.modal === 'module_edit'} />
                </div>

                <div className="lms-switch-premium lms-sec-modal-switch-wrapper">
                    <span className="lms-sec-modal-switch-label">Status</span>
                    <input
                        type="checkbox"
                        name="IsActive"
                        defaultChecked={ui.target ? (ui.target.isActive !== false) : true}
                        className="lms-sec-modal-checkbox"
                    />
                </div>

                <button type="submit" disabled={ui.loading} className="lms-btn-commit">
                    {ui.loading ? 'Saving...' : (ui.modal === 'module_create' ? 'Create Module' : 'Save Changes')}
                </button>
            </form>
        );
    }

    return null;
};
