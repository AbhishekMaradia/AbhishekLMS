import React from 'react';
import { Icons } from '../../../shared/components/lms/Icons';
import { CommonTable, CommonGrid, SecureImage, type Column } from '../../../shared/components/lms/LmsComponents';
import './Course.css';

export const CourseList = ({
    courses,
    cats,
    orgs,
    courseStatusFilter,
    viewMode,
    hasPermission,
    openCourseModal,
    handleCrud,
    setPreviewImage,
    tab,
    openCmStudio,
    user,
    isSuperAdmin,
    loading = false
}: any) => {

    const filtered = (courses || []).filter((c: any) => {
        const active = c.isActive ?? c.IsActive;
        const matchesStatus = (!courseStatusFilter || courseStatusFilter === 'all') ? true : courseStatusFilter === 'active' ? (active !== false) : (active === false);
        return matchesStatus;
    });

    const getOrgName = (c: any) => {
        if (c.orgName || c.OrgName) return c.orgName || c.OrgName;

        // Use the robust extraction logic again as it handles inconsistent API property names
        const rawTid = c.tenantId ?? c.TenantId ?? (c as any).organizationId ?? (c as any).OrganizationId ?? (c as any).tid;

        // Super Admin fallback for 0 or null
        if (rawTid === undefined || rawTid === null || Number(rawTid) === 0 || String(rawTid).toLowerCase() === 'null') {
            return 'Super Admin';
        }

        const tidNum = Number(rawTid);
        const org = (orgs || []).find((o: any) => {
            const oid = o.id ?? o.Id ?? o.tenantId ?? o.TenantId ?? o.tid ?? o.Tid ?? o.organizationId ?? o.OrganizationId;
            return Number(oid) === tidNum;
        });

        if (org) return org.orgName || org.OrgName || org.name || org.Name;
        if (!isSuperAdmin && (user?.orgName || user?.OrgName)) {
            return user.orgName || user.OrgName;
        }
        return 'Super Admin';
    };

    const headers: Column[] = [
        { header: 'Course Name', key: 'title' },
        { header: 'Category', key: 'category' },
        { header: 'Organization', key: 'org', hideOnMobile: true },
        { header: 'Price', key: 'price', hideOnMobile: true },
        { header: 'Status', key: 'status', hideOnMobile: true },
        { header: 'Actions', key: 'actions', className: 'lms-text-right' }
    ].filter(col => {
        if (tab === 'cm' && (col.key === 'org' || col.key === 'price' || col.key === 'status')) return false;
        return true;
    });

    if (viewMode === 'table') {
        return (
            <CommonTable
                headers={headers}
                loading={loading}
                empty={filtered.length === 0}
            >
                {filtered.map((c: any) => (
                    <tr key={c.courseId}>
                        <td>
                            <div className="lms-flex-row lms-courselist-flex-row">
                                {c.thumbnailUrl ? (
                                    <SecureImage src={c.thumbnailUrl} className="lms-media-thumb lms-courselist-thumb" />
                                ) : (
                                    <div className="lms-status-icon grey lms-courselist-icon-placeholder"><Icons.Book s={18} /></div>
                                )}
                                <div>
                                    <div className="lms-cell-bold">{c.title}</div>
                                </div>
                            </div>
                        </td>
                        <td>
                            <span className="lms-tag info">
                                {tab === 'cm'
                                    ? (getOrgName(c) || 'SYSTEM').toUpperCase()
                                    : ((cats || []).find((cat: any) => cat.categoryId === c.categoryId)?.categoryName || 'GENERAL').toUpperCase()
                                }
                            </span>
                        </td>
                        {tab !== 'cm' && <td className="lms-hide-mobile">
                            <span className="lms-tag info">
                                {(getOrgName(c) || 'Super Admin').toUpperCase()}
                            </span>
                        </td>}
                        {tab !== 'cm' && <td className="lms-hide-mobile">
                            <div className="lms-cell-bold lms-courselist-price">₹{c.price || 0}</div>
                        </td>}
                        {tab !== 'cm' && <td className="lms-hide-mobile">
                            <div className={`lms-status-dot ${c.isActive !== false ? 'active' : 'inactive'}`}>
                                {c.isActive !== false ? 'Active' : 'Inactive'}
                            </div>
                        </td>}
                        <td>
                            <div className="lms-cell-actions lms-cl-actions-left">
                                {tab === 'cm' ? (
                                    hasPermission('COURSE', 'COURSE_EDIT') && (
                                        <button onClick={() => openCmStudio(c)} className="lms-btn-pill-sm accent solid lms-courselist-btn-studio">
                                            <Icons.Play s={14} /> Studio
                                        </button>
                                    )
                                ) : (
                                    <>
                                        {hasPermission('COURSE', 'COURSE_EDIT') && <button onClick={() => openCourseModal(c)} className="lms-icon-btn-sm info" title="Edit"><Icons.Edit s={16} /></button>}
                                        {hasPermission('COURSE', 'COURSE_DELETE') && <button onClick={() => handleCrud('delete', 'course', c.courseId)} className="lms-icon-btn-sm danger" title="Delete"><Icons.Trash s={16} /></button>}
                                    </>
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
            empty={filtered.length === 0}
        >
            {filtered.map((c: any) => (
                <div key={c.courseId} className="lms-grid-card lms-fade-in">
                    <div className="lms-grid-banner">
                        <div className="lms-grid-overlay" />
                        {c.thumbnailUrl ? (
                            <SecureImage src={c.thumbnailUrl} className="lms-grid-banner-img" />
                        ) : (
                            <div className="lms-status-icon-bg lms-courselist-icon-bg">
                                <Icons.Book s={28} />
                            </div>
                        )}
                        <div className="lms-grid-badge">
                            <span className={`lms-tag ${c.isActive !== false ? 'success' : 'danger'}`}>
                                {c.isActive !== false ? 'ACTIVE' : 'INACTIVE'}
                            </span>
                        </div>
                    </div>

                    <div className="lms-grid-body">
                        <h3 className="lms-grid-title">{c.title}</h3>

                        <div className="lms-grid-meta">
                            <Icons.Org s={12} />
                            <span>{getOrgName(c)}</span>
                            <span className="lms-courselist-meta-dot">•</span>
                            <span>{(cats || []).find((cat: any) => cat.categoryId === c.categoryId)?.categoryName || 'GENERAL'}</span>
                        </div>

                        <div className="lms-grid-description">
                            General course providing core educational content and resources.
                        </div>

                        <div className="lms-grid-footer lms-courselist-grid-footer">
                            {tab === 'cm' ? (
                                <button onClick={() => openCmStudio(c)} className="lms-btn accent solid lms-flex-1 lms-courselist-studio-btn">
                                    <Icons.Play s={18} /> STUDIO
                                </button>
                            ) : (
                                <>
                                    <div className="lms-flex-1">
                                        <div className="lms-cell-bold lms-courselist-price-grid">₹{c.price || 0}</div>
                                        <div className="lms-status-sub lms-courselist-price-sub">PRICE</div>
                                    </div>
                                    <div className="lms-grid-actions">
                                        {hasPermission('COURSE', 'COURSE_EDIT') && (
                                            <button onClick={() => openCourseModal(c)} className="lms-icon-btn-sm info" title="Edit">
                                                <Icons.Edit s={16} />
                                            </button>
                                        )}
                                        {hasPermission('COURSE', 'COURSE_DELETE') && (
                                            <button onClick={() => handleCrud('delete', 'course', c.courseId)} className="lms-icon-btn-sm danger" title="Delete">
                                                <Icons.Trash s={16} />
                                            </button>
                                        )}
                                    </div>
                                </>
                            )}
                        </div>
                    </div>
                </div>
            ))}
        </CommonGrid>
    );
};
