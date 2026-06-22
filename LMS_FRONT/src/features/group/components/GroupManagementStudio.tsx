import React from 'react';
import { THEME, STYLES } from '../../../shared/components/lms/theme';
import { Icons } from '../../../shared/components/lms/Icons';
import { ViewToggle, CommonTable, CommonGrid, SearchInput } from '../../../shared/components/lms/LmsComponents';
import '../Group.css';

interface GroupManagementStudioProps {
    tab: 'gc' | 'gu';
    target: any; // gcTarget or guTarget
    setTarget: (target: any) => void;
    data: any; // gcCData or guUData
    search: string;
    setSearch: (s: string) => void;
    viewMode: 'table' | 'grid';
    setViewMode: (m: 'table' | 'grid') => void;
    openAssignModal: (target: any) => void;
    removeAction: (id: any) => void;
    getOrgNameByTenant: (tenantId: any) => string;
    getTenantId: (target: any) => any;
    isGlobalTenant: (id: any) => boolean;
    db: any;
}

export const GroupManagementStudio: React.FC<GroupManagementStudioProps> = ({
    tab,
    target,
    setTarget,
    data,
    search,
    setSearch,
    viewMode,
    setViewMode,
    openAssignModal,
    removeAction,
    getOrgNameByTenant,
    getTenantId,
    isGlobalTenant,
    db
}) => {
    if (!target) return null;

    const filtered = (tab === 'gc' ? data.courses : data.users).filter((item: any) => {
        const s = search.toLowerCase();
        if (tab === 'gc') {
            const name = `${item.courseName || item.title || ''}`.toLowerCase();
            const org = `${item.orgName || ''}`.toLowerCase();
            return name.includes(s) || org.includes(s);
        } else {
            const name = `${item.firstName} ${item.lastName}`.toLowerCase();
            const email = (item.email || '').toLowerCase();
            return name.includes(s) || email.includes(s);
        }
    });

    // --- TABLE CONFIG ---
    const columns = [
        { header: '#', key: 'index' },
        { header: tab === 'gc' ? 'Course' : 'User', key: 'name' },
        { header: tab === 'gc' ? 'Organization' : 'Email', key: 'info', hideOnMobile: true },
        { header: tab === 'gc' ? 'Access Type' : 'Status', key: 'type', hideOnMobile: true },
        { header: 'Actions', key: 'actions' }
    ];

    const renderTableRow = (item: any, cols: any[], index: number) => (
        <tr key={tab === 'gc' ? item.courseId : item.userId}>
            <td className="lms-status-sub lms-gms-td-index">{index + 1}</td>
            <td>
                <div className="lms-gms-flex-row">
                    <div className={`lms-status-icon lms-gms-icon-bg ${tab === 'gc' ? 'success' : 'info'}`}>
                        {tab === 'gc' ? <Icons.Book s={16} /> : <Icons.User s={16} />}
                    </div>
                    <div>
                        <div className="lms-status-title lms-gms-title">{tab === 'gc' ? (item.courseName || item.title) : `${item.firstName} ${item.lastName}`}</div>
                        {tab === 'gc' && <div className="lms-status-sub lms-gms-sub-text">{(db?.cats || [])?.find((cat: any) => cat.categoryId === item.categoryId)?.categoryName || 'Curriculum'}</div>}
                    </div>
                </div>
            </td>
            <td className="lms-hide-mobile">
                <span className="lms-tag info">
                    {tab === 'gc' ? item.orgName : item.email}
                </span>
            </td>
            <td className="lms-hide-mobile">
                <span className="lms-status-sub">
                    {tab === 'gc' ? (isGlobalTenant(item.courseTenantId) ? 'Foundation' : 'Organization') : 'ENROLLED'}
                </span>
            </td>
            <td>
                <button onClick={() => removeAction(tab === 'gc' ? item.courseId : item.userId)} className="lms-icon-btn-sm danger" title="Remove">
                    <Icons.Trash s={16} />
                </button>
            </td>
        </tr>
    );

    // --- GRID CONFIG ---
    const renderGridCard = (item: any) => {
        const id = tab === 'gc' ? item.courseId : item.userId;
        const bannerClass = tab === 'gc' ? 'info' : 'accent';
        const bannerIcon = tab === 'gc' ? <Icons.Book s={32} /> : <Icons.User s={32} />;

        return (
            <div key={id} className="lms-grid-card">
                <div className={`lms-grid-banner ${bannerClass}`}>
                    <div className="lms-banner-icon-ring">
                        {bannerIcon}
                    </div>
                    <div className="lms-grid-overlay" />
                </div>

                <div className="lms-grid-body">
                    <div className="lms-gms-grid-header">
                        <span className="lms-cell-id lms-gms-cell-id">
                            {tab === 'gc' ? 'CURRICULUM' : 'ENROLLED'}
                        </span>
                    </div>
                    <h3 className="lms-grid-title">{tab === 'gc' ? (item.courseName || item.title) : `${item.firstName} ${item.lastName}`}</h3>
                    
                    <div className="lms-gms-grid-info-box">
                        <p className="lms-grid-description lms-gms-grid-desc">
                            <Icons.Mail s={14} className="lms-gms-icon-mail" />
                            <span className="lms-gms-info-text">{tab === 'gc' ? item.orgName : item.email}</span>
                        </p>
                    </div>

                    <div className="lms-grid-footer lms-gms-grid-footer">
                        <button onClick={() => removeAction(id)} className="lms-icon-btn-sm danger" title="Remove from Group">
                            <Icons.Trash s={18} />
                        </button>
                    </div>
                </div>
            </div>
        );
    };

    return (
        <div className="lms-studio-entry">
            <div className="lms-studio-header">
                <div className="lms-section-header">
                    <div className="lms-gms-header-left">
                        <button onClick={() => setTarget(null)} className="lms-studio-back-btn">
                            <Icons.Close s={20} />
                        </button>
                        <div>
                            <h2 className="lms-studio-title">{target.groupName}</h2>
                            <div className={`lms-status-sub lms-gms-status-sub ${tab === 'gc' ? 'success' : 'info'}`}>
                                {tab === 'gc' ? 'Course Access Matrix' : 'User Enrollment Roster'}
                            </div>
                        </div>
                    </div>
                    <div className="lms-gms-header-right">
                        <div className="lms-tag info hide-mobile lms-gms-tag-scope">
                            SCOPE: {getOrgNameByTenant(getTenantId(target))}
                        </div>
                        <ViewToggle viewMode={viewMode} setViewMode={setViewMode} />
                        <button onClick={() => openAssignModal(target)} className={`lms-btn-pill-sm lms-gms-btn-assign ${tab === 'gc' ? 'success' : 'accent'} solid`}>
                            <Icons.Plus s={16} /> {tab === 'gc' ? 'ASSIGN' : 'ENROLL'}
                        </button>
                    </div>
                </div>

                <div className="lms-gms-search-wrap">
                    <SearchInput
                        placeholder={tab === 'gc' ? "Filter courses..." : "Search users..."}
                        value={search}
                        onChange={setSearch}
                    />
                </div>
            </div>

            {data.loading ? (
                <div className="lms-studio-loader">
                    <div className="lms-loader-spinner" />
                    <p>Synchronizing Repository...</p>
                </div>
            ) : (
                filtered.length === 0 ? (
                    <div className="lms-empty-state">
                        <h3 className="lms-empty-title">{search ? 'No matches found' : 'Registry empty'}</h3>
                        <p className="lms-empty-desc">Check your filters or add new assignments.</p>
                        {!search && (
                            <button onClick={() => openAssignModal(target)} className={`lms-btn-primary lms-gms-btn-empty ${tab === 'gc' ? 'success' : ''}`}>
                                {tab === 'gc' ? 'Assign Now' : 'Enroll Now'}
                            </button>
                        )}
                    </div>
                ) : (
                    viewMode === 'table' ? (
                        <CommonTable
                            headers={columns}
                            empty={filtered.length === 0}
                        >
                            {filtered.map((item: any, idx: number) => renderTableRow(item, columns, idx))}
                        </CommonTable>
                    ) : (
                        <CommonGrid
                            empty={filtered.length === 0}
                        >
                            {filtered.map((item: any) => renderGridCard(item))}
                        </CommonGrid>
                    )
                )

            )}
        </div>
    );
};
