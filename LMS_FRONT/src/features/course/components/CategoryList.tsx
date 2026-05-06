import React from 'react';
import { Icons } from '../../../shared/components/lms/Icons';
import { CommonTable, CommonGrid, type Column } from '../../../shared/components/lms/LmsComponents';
import type { CategoryDto } from '../api/categoryApi';
import './Course.css';

interface CategoryListProps {
    cats: CategoryDto[];
    orgs: any[];
    viewMode: 'table' | 'grid';
    hasPermission: (module: string, permission?: string) => boolean;
    ui: any;
    setUi: (ui: any) => void;
    handleCrud: (action: string, type: string, data: any) => void;
    user: any;
    isSuperAdmin: boolean;
}

export const CategoryList: React.FC<CategoryListProps> = ({
    cats,
    orgs,
    viewMode,
    hasPermission,
    setUi,
    ui,
    handleCrud,
    user,
    isSuperAdmin
}) => {
    const data = cats || [];

    const getOrgName = (c: any) => {
        if (c.orgName || c.OrgName) return c.orgName || c.OrgName;
        const tid = c.tenantId || c.TenantId;
        if (!tid || tid === 0) return 'Super Admin';
        const org = orgs?.find(o => Number(o.id || o.Id) === Number(tid));
        if (org) return org.orgName || org.Name || org.OrgName;
        if (!isSuperAdmin && (user?.orgName || user?.OrgName)) {
            return user.orgName || user.OrgName;
        }
        return 'Local Node';
    };

    const headers: Column[] = [
        { header: 'Category Name', key: 'name' },
        { header: 'Organization Scope', key: 'org', hideOnMobile: true },
        { header: 'Actions', key: 'actions', className: 'lms-text-center' }
    ];

    if (viewMode === 'table') {
        return (
            <CommonTable
                headers={headers}
                loading={ui.loading}
                empty={data.length === 0}
            >
                {data.map(c => (
                    <tr key={c.categoryId}>
                        <td className="lms-cl-td-middle">
                            <div className="lms-cell-bold lms-cl-cell-bold-text">{c.categoryName}</div>
                        </td>
                        <td className="lms-cl-td-middle">
                            <div>
                                <span className="lms-tag info lms-cl-tag-bold">{getOrgName(c)}</span>
                            </div>
                        </td>
                        <td className="lms-cl-td-middle">
                            <div className="lms-cell-actions lms-cl-actions-left">
                                {hasPermission('CATEGORY', 'CATEGORY_EDIT') && (
                                    <button
                                        onClick={() => setUi({ ...ui, modal: 'cat_edit', target: c })}
                                        className="lms-icon-btn-sm info"
                                        title="Edit"
                                    >
                                        <Icons.Edit s={16} />
                                    </button>
                                )}
                                {hasPermission('CATEGORY', 'CATEGORY_DELETE') && (
                                    <button
                                        onClick={() => handleCrud('delete', 'cat', c.categoryId)}
                                        className="lms-icon-btn-sm danger"
                                        title="Delete"
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
            loading={ui.loading}
            empty={data.length === 0}
        >
            {data.map(c => (
                <div key={c.categoryId} className="lms-grid-card lms-fade-in">
                    <div className="lms-grid-banner accent">
                        <div className="lms-grid-overlay" />
                        <div className="lms-status-icon-bg lms-cl-icon-scaled">
                            <Icons.Grid s={28} />
                        </div>
                        <div className="lms-grid-badge">
                            <span className="lms-tag success">ACTIVE</span>
                        </div>
                    </div>

                    <div className="lms-grid-body">
                        <h3 className="lms-grid-title">{c.categoryName}</h3>

                        <div className="lms-grid-meta">
                            <Icons.Org s={12} />
                            <span>{getOrgName(c)}</span>
                        </div>

                        <div className="lms-grid-description">
                            Department category for organizing courses and content.
                        </div>

                        <div className="lms-grid-footer lms-cl-grid-footer">
                            <div className="lms-flex-1">
                                <div className="lms-cell-bold lms-cl-footer-title">CATEGORY</div>
                                <div className="lms-status-sub lms-cl-footer-sub">SETTINGS</div>
                            </div>
                            <div className="lms-grid-actions">
                                {hasPermission('CATEGORY', 'CATEGORY_EDIT') && (
                                    <button
                                        onClick={() => setUi({ ...ui, modal: 'cat_edit', target: c })}
                                        className="lms-icon-btn-sm info"
                                        title="Edit"
                                    >
                                        <Icons.Edit s={16} />
                                    </button>
                                )}
                                {hasPermission('CATEGORY', 'CATEGORY_DELETE') && (
                                    <button
                                        onClick={() => handleCrud('delete', 'cat', c.categoryId)}
                                        className="lms-icon-btn-sm danger"
                                        title="Delete"
                                    >
                                        <Icons.Trash s={16} />
                                    </button>
                                )}

                            </div>
                        </div>
                    </div>
                </div>
            ))}
        </CommonGrid>
    );
};
