import React from 'react';
import { Icons } from '../../../shared/components/lms/Icons';
import '../Group.css';

interface GroupModalProps {
    ui: any;
    user: any;
    isSuperAdmin: boolean;
    db: any;
    handleCrud: (action: string, entity: string, data: any) => void;
}

export const GroupModal: React.FC<GroupModalProps> = ({
    ui,
    user,
    isSuperAdmin,
    db,
    handleCrud
}) => {
    return (
        <form
            key={`group-form-${ui.target?.id ?? 'new'}`}
            onSubmit={(e) => {
                e.preventDefault();
                const fd = new FormData(e.currentTarget);
                let data: any = Object.fromEntries(fd.entries());

                if ('TenantId' in data) {
                    const t = Number(data.TenantId);
                    data.TenantId = (!data.TenantId || data.TenantId === '' || isNaN(t) || t === 0) ? null : t;
                }
                const op = ui.modal.split('_')[1];
                const action = (op === 'edit' || op === 'update') ? 'update' : 'create';

                handleCrud(action, 'group', data);
            }}
            className="lms-fade-in lms-col-gap-md"
        >
            {ui.modal === 'group_edit' && <input type="hidden" name="Id" value={ui.target?.id || 0} />}

            <label className="lms-label-premium required">Name</label>
            <div className="lms-modal-panel-premium">
                <input name="GroupName" defaultValue={ui.target?.groupName} placeholder="Name" className="lms-input-premium" required />
            </div>

            {isSuperAdmin ? (
                <>
                    <label className="lms-label-premium required">Organization</label>
                    <div className="lms-modal-panel-premium">
                        <select
                            name="TenantId"
                            key={ui.modal + (ui.target?.id || 'new')}
                            defaultValue={ui.target?.tenantId ?? ui.target?.TenantId ?? 0}
                            className="lms-select-premium"
                            required
                        >
                            <option value={0}>Super Admin (Global)</option>
                            {(db.orgs || []).filter((o: any) => (o.isActive ?? o.IsActive) !== false).map((o: any) => <option key={o.id || o.Id} value={o.id || o.Id}>{o.orgName || o.OrgName}</option>)}
                        </select>
                    </div>
                </>
            ) : (
                <input type="hidden" name="TenantId" value={user?.tenantId ?? (user as any)?.TenantId ?? 0} />
            )}

            <button type="submit" disabled={ui.loading} className="lms-btn-commit lms-group-modal-btn">
                {ui.loading ? 'Saving...' : 'Save'}
            </button>
        </form>
    );
};
