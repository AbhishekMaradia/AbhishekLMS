import React from 'react';
import {
    Icons,
    Card,
    Pagination,
    SearchInput,
    PerspectiveSwitcher,
    Button,
    CommonTable,
    CommonGrid,
    PermissionButton,
    type Column
} from '../shared/components/lms/LmsComponents';

export const EnrollmentsPage: React.FC<any> = ({
    db, searchTerm, setSearchTerm, pagination, changePage, changePageSize, isSuperAdmin,
    revokeEnrollment, viewMode, setViewMode, ui, hasPermission
}) => {
    const list = db.enrollments || [];
    const p = pagination['enroll'] || { page: 1, size: 50, total: 0 };
    const totalPages = Math.ceil((p.total || 0) / (p.size || 50)) || 1;

    const canRevoke = isSuperAdmin || hasPermission('SUBSCRIPTION', 'SUBSCRIPTION_DELETE');

    const headers: Column[] = [
        { header: 'Student Profile', key: 'student' },
        { header: 'Curriculum Item', key: 'course' },
        { header: 'Organization', key: 'org', hideOnMobile: true },
        { header: 'Subscribed At', key: 'date', hideOnMobile: true },
        { header: 'Status', key: 'status', hideOnMobile: true },
        { header: 'Actions', key: 'actions', className: 'lms-text-right' }
    ];

    return (
        <div className="lms-page lms-fade-in">
            <div className="lms-premium-card">
                <div className="lms-entity-header">
                    <div className="lms-section-heading">
                        <h1 className="lms-card-title">Course Enrollments</h1>
                        <span className="lms-section-count">{p.total} records</span>
                    </div>
                    <div className="lms-card-actions">
                        <PerspectiveSwitcher viewMode={viewMode || 'table'} setViewMode={setViewMode} />
                    </div>
                </div>

                <div className="lms-entity-filters">
                    <div className="lms-entity-search">
                        <SearchInput
                            value={searchTerm}
                            onChange={setSearchTerm}
                            placeholder="Filter by student email or course title..."
                        />
                    </div>
                </div>
            </div>

            <div className="lms-container">
                {viewMode === 'table' ? (
                    <CommonTable
                        headers={headers}
                        loading={ui.loading}
                        empty={list.length === 0}
                    >
                        {list.map((item: any) => (
                            <tr key={`${item.userId}-${item.courseId}`}>
                                <td>
                                    <div className="lms-flex-row lms-gap-md">
                                        <div className="lms-status-icon-bg" style={{ width: '32px', height: '32px', fontSize: '12px', background: 'var(--color-primary-soft)', color: 'var(--color-primary)' }}>
                                            <Icons.User s={14} />
                                        </div>
                                        <div>
                                            <div className="lms-cell-bold">{item.userName || item.userEmail}</div>
                                            <div className="lms-text-xs lms-text-dim">{item.userEmail}</div>
                                        </div>
                                    </div>
                                </td>
                                <td>
                                    <div>
                                        <div className="lms-cell-bold" style={{ color: 'var(--color-primary)' }}>{item.courseTitle}</div>
                                        <div className="lms-text-xs lms-text-dim">{item.categoryName}</div>
                                    </div>
                                </td>
                                <td className="lms-hide-mobile">
                                    <span className="lms-tag info">
                                        {(item.tenantName || 'Super Admin').toUpperCase()}
                                    </span>
                                </td>
                                <td className="lms-hide-mobile">
                                    <div className="lms-text-sm lms-text-muted">
                                        {new Date(item.subscribedAt).toLocaleDateString()}
                                    </div>
                                </td>
                                <td className="lms-hide-mobile">
                                    <div className="lms-status-dot active">Active</div>
                                </td>
                                <td>
                                    <div className="lms-cell-actions lms-cl-actions-left">
                                        <PermissionButton
                                            hasPermission={canRevoke}
                                            onClick={() => revokeEnrollment(item.userId, item.courseId)}
                                            className="lms-icon-btn-sm danger"
                                            title="Revoke Access"
                                        >
                                            <Icons.Trash s={16} />
                                        </PermissionButton>
                                    </div>
                                </td>
                            </tr>
                        ))}
                    </CommonTable>
                ) : (
                    <CommonGrid
                        loading={ui.loading}
                        empty={list.length === 0}
                    >
                        {list.map((item: any) => (
                            <div key={`${item.userId}-${item.courseId}`} className="lms-grid-card lms-fade-in">
                                <div className="lms-grid-banner">
                                    <div className="lms-grid-overlay" />
                                    <div className="lms-status-icon-bg">
                                        <Icons.Users s={28} />
                                    </div>
                                    <div className="lms-grid-badge">
                                        <span className="lms-tag success">ACTIVE</span>
                                    </div>
                                </div>

                                <div className="lms-grid-body">
                                    <h3 className="lms-grid-title" style={{ fontSize: '15px', lineHeight: '1.4' }}>{item.courseTitle}</h3>

                                    <div className="lms-grid-meta">
                                        <Icons.User s={12} />
                                        <span style={{ fontWeight: 700 }}>{item.userName || 'Student'}</span>
                                    </div>
                                    <div className="lms-grid-meta" style={{ marginTop: '4px' }}>
                                        <Icons.Org s={12} />
                                        <span>{item.tenantName || 'Main Org'}</span>
                                    </div>

                                    <div className="lms-grid-footer" style={{ marginTop: '16px', borderTop: '1px solid var(--color-border-bright)', paddingTop: '16px' }}>
                                        <div style={{ flex: 1 }}>
                                            <div style={{ fontSize: '12px', fontWeight: 700 }}>{new Date(item.subscribedAt).toLocaleDateString()}</div>
                                            <div style={{ fontSize: '10px', opacity: 0.5, textTransform: 'uppercase' }}>ENROLLED DATE</div>
                                        </div>
                                        <PermissionButton
                                            hasPermission={canRevoke}
                                            onClick={() => revokeEnrollment(item.userId, item.courseId)}
                                            className="lms-icon-btn-sm danger"
                                            title="Revoke Access"
                                        >
                                            <Icons.Trash s={16} />
                                        </PermissionButton>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </CommonGrid>
                )}

                <Pagination
                    current={p.page}
                    total={totalPages}
                    totalItems={p.total}
                    itemsPerPage={p.size}
                    onPageChange={(page: number) => changePage('enroll', page)}
                    onPageSizeChange={(size: number) => changePageSize('enroll', size)}
                />
            </div>

        </div>
    );
};
