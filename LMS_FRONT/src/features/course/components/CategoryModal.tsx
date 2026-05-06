import React from 'react';
import { Icons } from '../../../shared/components/lms/Icons';

interface CategoryModalProps {
    ui: any;
    user: any;
    isSuperAdmin: boolean;
    db: any;
    handleCrud: (action: string, entity: string, data?: any) => Promise<void>;
}

export const CategoryModal: React.FC<CategoryModalProps> = ({
    ui,
    user,
    isSuperAdmin,
    db,
    handleCrud
}) => {
    return (
        <form onSubmit={(e) => {
            e.preventDefault();
            const fd = new FormData(e.currentTarget);
            let data: any = Object.fromEntries(fd.entries());

            if (!isSuperAdmin) {
                data.TenantId = (user as any).tenantId ?? (user as any).TenantId;
            } else {
                if ('TenantId' in data) {
                    data.TenantId = (Number(data.TenantId) === 0 || isNaN(Number(data.TenantId))) ? null : Number(data.TenantId);
                }
            }
            const op = ui.modal.split('_')[1];
            const action = (op === 'edit' || op === 'update') ? 'update' : 'create';

            handleCrud(action, 'cat', data);
        }} className="lms-fade-in lms-col-gap-md">
            {ui.modal === 'cat_edit' && <input type="hidden" name="Id" value={ui.target?.categoryId || ui.target?.id || ui.target?.Id || 0} />}

            <label className="lms-label-premium required">Name</label>
            <div className="lms-modal-panel-premium">
                <input name="CategoryName" defaultValue={ui.target?.categoryName} placeholder="Name" className="lms-input-premium" required />
            </div>

            {isSuperAdmin ? (
                <>
                    <label className="lms-label-premium required">Organization</label>
                    <div className="lms-modal-panel-premium">
                        <select
                            name="TenantId"
                            key={ui.modal + (ui.target?.id || 'new')}
                            defaultValue={ui.target?.tenantId || 0}
                            className="lms-select-premium"
                        >
                            <option value={0}>Super Admin (Global)</option>
                            {(db.orgs || []).filter((o: any) => (o.isActive ?? o.IsActive) !== false).map((o: any) => <option key={o.id || o.Id} value={o.id || o.Id}>{o.orgName || o.OrgName}</option>)}
                        </select>
                    </div>
                </>
            ) : (
                <input type="hidden" name="TenantId" value={user?.tenantId ?? (user as any)?.TenantId ?? 0} />
            )}

            <button type="submit" disabled={ui.loading} className="lms-btn-commit">
                {ui.loading ? 'Saving...' : 'Save'}
            </button>
        </form>
    );
};
