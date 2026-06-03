import React from 'react';
import { Icons } from '../shared/components/lms/Icons';
import { OrganizationList } from '../features/organization/components/OrganizationList';
import { Pagination, Button, PerspectiveSwitcher, SearchInput, StatusFilter } from '../shared/components/lms/LmsComponents';
import '../features/organization/components/Organization.css';

interface OrganizationsPageProps {
    db: any;
    ui: any;
    setUi: (u: any) => void;
    searchTerm: string;
    setSearchTerm: (s: string) => void;
    viewMode: 'table' | 'grid';
    setViewMode: (v: 'table' | 'grid') => void;
    pagination: any;
    changePage: (e: string, p: number) => void;
    changePageSize: (e: string, s: number) => void;
    hasPermission: (m: string, a?: string) => boolean;
    handleCrud: (a: string, t: string, d: any) => void;
    user: any;
    isSuperAdmin: boolean;
}

export const OrganizationsPage: React.FC<OrganizationsPageProps> = ({
    db, ui, setUi, searchTerm, setSearchTerm, viewMode, setViewMode,
    pagination, changePage, changePageSize, hasPermission, handleCrud, user, isSuperAdmin
}) => {
    const p = pagination['orgs'] || { page: 1, size: 50, total: 0 };
    
    // Filter organizations client-side for correct counts/pagination when filtered by status
    const filteredOrgs = (db.orgs || []).filter((o: any) => {
        const activeVal = o.isActive ?? o.IsActive;
        const matchesStatus =
            (ui.statusFilter || 'all') === 'all' ? true :
                (ui.statusFilter || 'all') === 'active' ? (activeVal !== false) :
                    (activeVal === false);
        return matchesStatus;
    });

    const adjustedTotal = p.total <= (p.size || 50) ? filteredOrgs.length : p.total;
    const totalPages = Math.ceil(adjustedTotal / (p.size || 50)) || 1;

    const canCreate = isSuperAdmin || hasPermission('ORGANIZATION', 'ORGANIZATION_ADD');

    return (
        <div className="lms-orgs-page lms-fade-in">
            <div className="lms-premium-card">
                <div className="lms-entity-header">
                    <div className="lms-section-heading">
                        <h1 className="lms-card-title">Organizations</h1>
                        <span className="lms-section-count">{adjustedTotal} organizations</span>
                    </div>

                    <div className="lms-card-actions">
                        <PerspectiveSwitcher viewMode={viewMode} setViewMode={setViewMode} />
                        {canCreate && (
                            <Button
                                variant="primary"
                                onClick={() => setUi({ ...ui, modal: 'org_create', target: null })}
                                className="lms-btn-primary lms-orgs-add-btn"
                            >
                                <Icons.Plus s={18} /> ADD ORG
                            </Button>
                        )}
                    </div>
                </div>

                <div className="lms-entity-filters">
                    <div className="lms-entity-search">
                        <SearchInput value={searchTerm} onChange={setSearchTerm} placeholder="Filter by organization name or code..." />
                    </div>
                    <StatusFilter
                        value={ui.statusFilter}
                        onChange={(v) => setUi({ ...ui, statusFilter: v })}
                    />
                </div>
            </div>

            <div className="lms-container">
                <OrganizationList
                    orgs={db.orgs}
                    orgStatusFilter={ui.statusFilter || 'all'}
                    viewMode={viewMode}
                    hasPermission={hasPermission}
                    setUi={setUi}
                    ui={ui}
                    handleCrud={handleCrud}
                    loading={ui.loading}
                />

                <Pagination
                    current={p.page}
                    total={totalPages}
                    totalItems={adjustedTotal}
                    itemsPerPage={p.size}
                    onPageChange={(page: number) => changePage('orgs', page)}
                    onPageSizeChange={(size: number) => changePageSize('orgs', size)}
                />
            </div>
        </div>
    );
};
