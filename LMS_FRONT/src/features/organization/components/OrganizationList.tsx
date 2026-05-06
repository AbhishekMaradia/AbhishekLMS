import React from 'react';
import { Icons } from '../../../shared/components/lms/Icons';
import { CommonTable, CommonGrid, SecureImage } from '../../../shared/components/lms/LmsComponents';
import type { OrganizationDto as OrgDto } from '../types/organization.types';
import './Organization.css';

interface OrganizationListProps {
    orgs: OrgDto[];
    orgStatusFilter: 'all' | 'active' | 'inactive';
    viewMode: 'table' | 'grid';
    hasPermission: (module: string, action: string) => boolean;
    setUi: (ui: any) => void;
    ui: any;
    handleCrud: (action: string, type: string, id: any) => void;
    loading?: boolean;
}

export const OrganizationList: React.FC<OrganizationListProps> = ({
    orgs,
    orgStatusFilter,
    viewMode,
    hasPermission,
    setUi,
    ui,
    handleCrud,
    loading = false
}) => {
    const filteredOrgs = orgs.filter(o => {
        const activeVal = o.isActive;
        const matchesStatus = orgStatusFilter === 'all' ? true :
            orgStatusFilter === 'active' ? (activeVal !== false) :
                (activeVal === false);
        return matchesStatus;
    });

    const headers = [
        { header: 'Organization Name', key: 'name' },
        { header: 'Org Code', key: 'code', hideOnMobile: true },
        { header: 'Status', key: 'status' },
        { header: 'Actions', key: 'actions', className: 'lms-text-right' }
    ];

    if (viewMode === 'table') {
        return (
            <CommonTable
                headers={headers}
                loading={loading}
                empty={filteredOrgs.length === 0}
            >
                {filteredOrgs.map(o => (
                    <tr key={o.id}>
                        <td>
                            <div className="lms-flex-row lms-org-list-cell-row">
                                <div className="lms-org-list-logo-box">
                                    {o.logoUrl ? (
                                        <SecureImage src={o.logoUrl} className="lms-org-list-logo-img" />
                                    ) : (
                                        <Icons.Org s={14} className="lms-org-list-logo-icon" />
                                    )}
                                </div>
                                <div className="lms-cell-bold">{o.orgName}</div>
                            </div>
                        </td>
                        <td className="lms-hide-mobile">
                            <span className="lms-tag info">{o.orgCode}</span>
                        </td>
                        <td>
                            <div className={`lms-status-dot ${o.isActive !== false ? 'active' : 'inactive'}`}>
                                {o.isActive !== false ? 'Active' : 'Inactive'}
                            </div>
                        </td>
                        <td>
                            <div className="lms-cell-actions lms-cl-actions-left">
                                {hasPermission('ORGANIZATION', 'ORGANIZATION_EDIT') && (
                                    <button
                                        onClick={() => setUi({ ...ui, modal: 'org_edit', target: o })}
                                        className="lms-icon-btn-sm info"
                                        title="Edit Node"
                                    >
                                        <Icons.Edit s={16} />
                                    </button>
                                )}
                                {hasPermission('ORGANIZATION', 'ORGANIZATION_DELETE') && (
                                    <button
                                        onClick={() => handleCrud('delete', 'org', o)}
                                        className="lms-icon-btn-sm danger"
                                        title="Delete Node"
                                    >
                                        <Icons.Trash s={16} />
                                    </button>
                                )}

                            </div>
                        </td>
                    </tr>
                ))}
            </CommonTable>
        );
    }

    return (
        <CommonGrid
            loading={loading}
            empty={filteredOrgs.length === 0}
        >
            {filteredOrgs.map(o => {
                const isActive = o.isActive !== false;
                return (
                    <div key={o.id} className="lms-grid-card lms-fade-in">
                        {o.logoUrl && (
                            <div className="lms-grid-banner lms-org-list-banner">
                                <SecureImage src={o.logoUrl} className="lms-org-list-banner-img" />
                                <div className="lms-grid-badge">
                                    <span className={`lms-tag ${isActive ? 'success' : 'danger'}`}>
                                        {isActive ? 'ACTIVE' : 'INACTIVE'}
                                    </span>
                                </div>
                                <div className="lms-grid-overlay" />
                            </div>
                        )}
                        <div className="lms-grid-body lms-org-list-body">
                            <h3 className="lms-grid-title lms-org-list-title">{o.orgName}</h3>
                            <div className="lms-flex-row lms-org-list-code-row">
                                <span className="lms-tag info lms-org-list-code-tag">{o.orgCode}</span>
                                <div className="lms-flex-row lms-org-list-colors-row">
                                    <div className="lms-org-list-color-swatch" style={{ '--swatch-color': o.primaryColor || '#763121' } as any} />
                                    <div className="lms-org-list-color-swatch" style={{ '--swatch-color': o.secondaryColor || '#4a2118' } as any} />
                                </div>
                            </div>

                            <div className="lms-grid-footer lms-org-list-footer">
                                <div className="lms-grid-actions">
                                    {hasPermission('ORGANIZATION', 'ORGANIZATION_EDIT') && (
                                        <button
                                            onClick={() => setUi({ ...ui, modal: 'org_edit', target: o })}
                                            className="lms-icon-btn-sm info"
                                            title="Configure Entity"
                                        >
                                            <Icons.Edit s={18} />
                                        </button>
                                    )}

                                    {hasPermission('ORGANIZATION', 'ORGANIZATION_DELETE') && (
                                        <button
                                            onClick={() => handleCrud('delete', 'org', o)}
                                            className="lms-icon-btn-sm danger"
                                            title="Decommission Node"
                                        >
                                            <Icons.Trash s={18} />
                                        </button>
                                    )}

                                </div>
                            </div>
                        </div>
                    </div>
                );
            })}
        </CommonGrid>
    );
};
