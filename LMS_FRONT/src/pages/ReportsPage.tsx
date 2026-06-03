import React, { useState } from 'react';
import { Icons, PerspectiveSwitcher, SearchInput, Pagination, CommonTable, CommonGrid, CustomSelect } from '../shared/components/lms/LmsComponents';
import { apiClient as api } from '../core/api/apiClient';

export const ReportsPage: React.FC<any> = ({
    db, searchTerm, setSearchTerm, pagination, changePage, changePageSize, ui, isSuperAdmin, filters, setFilters
}) => {
    const list = db.reports || [];
    const p = pagination['reports'] || { page: 1, size: 5, total: 0 };
    const totalPages = Math.ceil((p.total || 0) / (p.size || 5)) || 1;

    const [expandedRow, setExpandedRow] = useState<string | null>(null);
    const [details, setDetails] = useState<any>(null);
    const [detailsLoading, setDetailsLoading] = useState(false);

    const headers = [
        { header: '', key: 'exp', className: 'lms-col-chevron' },
        { header: 'Student Identity', key: 'user', className: 'lms-col-student' },
        ...(isSuperAdmin ? [{ header: 'Organization', key: 'org', className: 'lms-col-org', hideOnMobile: false }] : []),
        { header: 'Cohort / Group', key: 'group', className: 'lms-col-group', hideOnMobile: true },
        { header: 'Knowledge Domain', key: 'course', className: 'lms-col-course' },
        { header: 'Progress', key: 'progress', className: 'lms-col-progress' },
        { header: 'Activity', key: 'date', className: 'lms-col-activity', hideOnMobile: true },
        { header: 'Actions', key: 'actions', className: 'lms-col-actions lms-text-right' }
    ];

    const toggleExpand = async (item: any) => {
        const rowId = `${item.userId}-${item.courseId}`;
        if (expandedRow === rowId) {
            setExpandedRow(null);
            return;
        }

        setExpandedRow(rowId);
        setDetailsLoading(true);
        setDetails(null);
        try {
            const res = await api.get(`reports/course/${item.userId}/${item.courseId}`);
            const reportData = res.data?.data;
            const data = Array.isArray(reportData) ? reportData[0] : reportData;
            setDetails(data);
        } catch (err) {
            console.error("Failed to fetch detailed audit", err);
        } finally {
            setDetailsLoading(false);
        }
    };

    const averageImmersion = React.useMemo(() => {
        if (list.length === 0) return 0;
        const sum = list.reduce((acc: number, r: any) => acc + (r.completionPercentage || 0), 0);
        return Math.round(sum / list.length);
    }, [list]);

    const handleFilterChange = (key: string, value: any) => {
        setFilters({ ...filters, [key]: value });
    };

    const formatDate = (dateStr: string) => {
        if (!dateStr) return 'Never';
        const d = new Date(dateStr);
        if (isNaN(d.getTime()) || d.getFullYear() <= 1970) return 'Never';
        return d.toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' });
    };

    return (
        <div className="lms-page lms-fade-in">
            <header className="lms-reports-hero-premium">
                <div className="lms-hero-main">
                    <div className="lms-pro-tag">ACADEMIC PERFORMANCE HUB</div>
                    <h1 className="lms-hero-title">{isSuperAdmin ? 'Global Performance Overview' : 'Organization Performance Overview'}</h1>
                    <p className="lms-hero-desc">Track student progress, video completion rates, and recent learning activity.</p>
                </div>
                <div className="lms-hero-stats">
                    <div className="lms-stat-card-pro">
                        <div className="lms-stat-val">{averageImmersion}%</div>
                        <div className="lms-stat-lbl">PLATFORM AVG IMMERSION</div>
                    </div>
                </div>
            </header>

            <div className="lms-premium-card" style={{ marginTop: '12px' }}>
                <div className="lms-entity-filters" style={{ border: 'none', padding: '8px 16px', gap: '12px', flexWrap: 'wrap' }}>
                    <div className="lms-entity-search" style={{ flex: '1', minWidth: '300px' }}>
                        <SearchInput
                            value={searchTerm}
                            onChange={setSearchTerm}
                            placeholder="Search by student name or domain..."
                        />
                    </div>

                    {isSuperAdmin && (
                        <CustomSelect
                            options={[
                                { value: '', label: 'All Organizations' },
                                ...(db.orgs?.map((o: any) => ({ value: o.id || o.Id, label: o.orgName || o.Name || o.name || 'Unknown' })) || [])
                            ]}
                            value={filters?.tenantId || ''}
                            onChange={(val) => handleFilterChange('tenantId', val)}
                        />
                    )}

                    <CustomSelect
                        options={[
                            { value: '', label: 'All Cohorts / Groups' },
                            ...(db.groups?.filter((g: any) => g && (!filters?.tenantId || g.tenantId === parseInt(filters.tenantId))).map((g: any) => ({ value: g.id || g.Id, label: g.name || g.Name || g.groupName || g.GroupName })) || [])
                        ]}
                        value={filters?.groupId || ''}
                        onChange={(val) => handleFilterChange('groupId', val)}
                    />

                    <input
                        type="date"
                        className="lms-premium-date"
                        value={filters?.dateFrom || ''}
                        onChange={(e) => handleFilterChange('dateFrom', e.target.value)}
                        title="Enrolled From"
                    />
                </div>
            </div>

            <div className="lms-container">
                <CommonTable
                    headers={headers}
                    loading={ui.loading}
                    empty={list.length === 0}
                >
                    {list.map((item: any) => {
                        const rowId = `${item.userId}-${item.courseId}`;
                        const isExp = expandedRow === rowId;
                        return (
                            <React.Fragment key={rowId}>
                                <tr className={`lms-report-main-row ${isExp ? 'active' : ''}`} onClick={() => toggleExpand(item)}>
                                    <td className="lms-col-chevron">
                                        <Icons.ChevronRight s={16} className={`lms-report-chevron ${isExp ? 'rotate' : ''}`} />
                                    </td>
                                    <td className="lms-col-student">
                                        <div className="lms-flex-row" style={{ gap: '12px' }}>
                                            <div className="lms-status-icon-bg" style={{ width: '36px', height: '36px', fontSize: '11px', background: 'var(--color-primary-soft)', color: 'var(--color-primary)' }}>
                                                {(item.userName || item.userEmail || "U").charAt(0).toUpperCase()}
                                            </div>
                                            <div>
                                                <div className="lms-cell-bold">{item.userName || 'Anonymous Student'}</div>
                                                <div className="lms-report-student-email">{item.userEmail}</div>
                                            </div>
                                        </div>
                                    </td>
                                    {isSuperAdmin && (
                                        <td className="lms-col-org lms-hide-mobile">
                                            <span className="lms-tag info">
                                                {item.orgName || 'Platform'}
                                            </span>
                                        </td>
                                    )}
                                    <td className="lms-col-group lms-hide-mobile">
                                        <div className="lms-report-group-name">
                                            <Icons.Users s={12} className="lms-report-group-icon" />
                                            {item.groupName || 'General'}
                                        </div>
                                    </td>
                                    <td className="lms-col-course">
                                        <div>
                                            <div className="lms-cell-bold lms-report-domain-title">{item.courseTitle}</div>
                                            <div className="lms-report-domain-instructor">by {item.courseInstructor || 'Senior Faculty'}</div>
                                        </div>
                                    </td>
                                    <td className="lms-col-progress">
                                        <div className="lms-report-progress-container">
                                            <div className="lms-flex-row" style={{ justifyContent: 'space-between', marginBottom: '4px' }}>
                                                <span className="lms-report-progress-text">{item.completionPercentage}%</span>
                                                <span className="lms-report-progress-mods">{item.completedVideos}/{item.totalVideos} MODS</span>
                                            </div>
                                            <div className="lms-progress-track">
                                                <div className="lms-progress-fill" style={{
                                                    width: `${item.completionPercentage}%`,
                                                    background: item.completionPercentage >= 100 ? '#10b981' : 'var(--color-primary)'
                                                }} />
                                            </div>
                                        </div>
                                    </td>
                                    <td className="lms-col-activity lms-hide-mobile">
                                        <div className="lms-report-activity">
                                            {formatDate(item.lastUpdated)}
                                            <div className="lms-report-activity-sub">Activity Sync</div>
                                        </div>
                                    </td>
                                    <td className="lms-col-actions lms-text-right">
                                        <div className="lms-cell-actions">
                                            <button 
                                                className={`lms-icon-btn-sm ${isExp ? 'active' : ''}`} 
                                                onClick={(e) => { e.stopPropagation(); toggleExpand(item); }}
                                                title={isExp ? "Hide Detailed Audit" : "View Detailed Audit"}
                                            >
                                                <Icons.Eye s={16} style={{ transform: isExp ? 'scale(1.1)' : 'none', color: isExp ? 'var(--color-primary)' : 'inherit' }} />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                                {isExp && (
                                    <tr className="lms-report-details-row">
                                        <td colSpan={headers.length}>
                                            <div className="lms-report-nested-content lms-fade-in">
                                                <header className="lms-report-details-header">GRANULAR SUB-MODULE PERFORMANCE AUDIT</header>
                                                {detailsLoading ? (
                                                    <div className="lms-details-loading">Synchronizing with academic node...</div>
                                                ) : details ? (
                                                    <div className="lms-report-video-grid">
                                                        {details.videoDetails?.map((v: any) => (
                                                            <div key={v.videoId} className="lms-report-video-item">
                                                                <div className="lms-report-v-info">
                                                                    <Icons.Play s={12} style={{ color: v.isCompleted ? '#10b981' : 'var(--color-primary)' }} />
                                                                    <span className="lms-report-v-title">{v.title}</span>
                                                                </div>
                                                                <div className="lms-report-v-stats">
                                                                    <span className="lms-report-v-pct">{Number(v.watchedPercentage).toFixed(1)}%</span>
                                                                    <div className="lms-report-v-dot" style={{ background: v.isCompleted ? '#10b981' : '#fbbf24' }} />
                                                                </div>
                                                            </div>
                                                        ))}
                                                        {(!details.videoDetails || details.videoDetails.length === 0) && (
                                                            <p className="lms-report-empty">No granular module-level data captured for this domain.</p>
                                                        )}
                                                    </div>
                                                ) : (
                                                    <p className="lms-report-error">Could not retrieve detailed audit from the data nexus.</p>
                                                )}
                                            </div>
                                        </td>
                                    </tr>
                                )}
                            </React.Fragment>
                        );
                    })}
                </CommonTable>

                <Pagination
                    current={p.page}
                    total={totalPages}
                    totalItems={p.total}
                    itemsPerPage={p.size}
                    onPageChange={(page: number) => changePage('reports', page)}
                    onPageSizeChange={(size: number) => changePageSize('reports', size)}
                />
            </div>

            <style>{`
                .lms-page {
                    margin-top: -12px;
                }
                .lms-reports-hero-premium { 
                    display: flex; 
                    justify-content: space-between; 
                    align-items: center; 
                    padding: 16px 28px; 
                    background: var(--color-bg-alt); 
                    border-radius: 20px; 
                    border: 1px solid var(--color-border); 
                    box-shadow: var(--shadow-sm); 
                }
                .lms-pro-tag { font-size: 10px; font-weight: 950; color: var(--color-primary); letter-spacing: 1.5px; }
                .lms-hero-title { font-size: 24px; font-weight: 900; margin: 2px 0; color: var(--color-primary); }
                .lms-hero-desc { font-size: 13px; color: var(--color-text-muted); }
                
                .lms-stat-card-pro { 
                    text-align: center; 
                    background: var(--color-bg); 
                    padding: 8px 20px; 
                    border-radius: 14px; 
                    border: 1px solid var(--color-border); 
                    box-shadow: var(--shadow-sm); 
                }
                .lms-stat-val { font-size: 24px; font-weight: 950; color: var(--color-primary); }
                .lms-stat-lbl { font-size: 9px; font-weight: 900; color: var(--color-text-dim); margin-top: 2px; }

                .lms-page .lms-table-wrapper {
                    margin-top: 12px !important;
                }
                .lms-page .lms-table-main th {
                    padding: 10px 16px !important;
                    font-size: 11px !important;
                }
                .lms-page .lms-table-main td {
                    padding: 8px 16px !important;
                    font-size: 13px !important;
                }

                .lms-premium-date { 
                    height: 42px;
                    background: var(--color-bg-alt); 
                    border: 1px solid var(--color-border); 
                    border-radius: 12px; 
                    padding: 0 16px; 
                    font-size: 13px; 
                    font-weight: 700; 
                    color: var(--color-text); 
                    outline: none; 
                    cursor: pointer; 
                    transition: all var(--transit); 
                    min-width: 180px;
                    font-family: inherit;
                    box-sizing: border-box;
                }
                .lms-premium-date:hover { border-color: var(--color-primary); }
                .lms-premium-date:focus { 
                    border-color: var(--color-primary);
                    box-shadow: 0 0 0 3px var(--color-primary-soft);
                }
                .lms-premium-date::-webkit-calendar-picker-indicator {
                    cursor: pointer;
                    opacity: 0.6;
                    transition: opacity 0.2s;
                }
                .lms-premium-date::-webkit-calendar-picker-indicator:hover {
                    opacity: 1;
                }
                :root[data-theme='dark'] .lms-premium-date::-webkit-calendar-picker-indicator {
                    filter: invert(100%);
                }

                .lms-page .lms-table-main {
                    table-layout: fixed;
                    width: 100%;
                    min-width: 100%;
                }
                .lms-col-chevron { width: 45px; text-align: center; }
                .lms-col-student { width: 25%; }
                .lms-col-org { width: 12%; }
                .lms-col-group { width: 15%; }
                .lms-col-course { width: 25%; }
                .lms-col-progress { width: 13%; }
                .lms-col-activity { width: 10%; }
                .lms-col-actions { width: 65px; text-align: right; }

                .lms-col-student div, .lms-col-course div, .lms-col-group div {
                    white-space: normal;
                    word-wrap: break-word;
                    word-break: break-word;
                }

                .lms-report-main-row { cursor: pointer; transition: all var(--transit); border-bottom: 1px solid var(--color-border-bright); }
                .lms-report-main-row:hover td { background: var(--color-nav-hover) !important; }
                .lms-report-main-row.active td { background: rgba(var(--color-primary-rgb), 0.05) !important; }
                .lms-report-main-row.active td:first-child { border-left: 4px solid var(--color-primary); }
                .lms-report-main-row.active { border-bottom-color: transparent; }
                
                .lms-report-chevron { transition: transform var(--transit), color var(--transit), opacity var(--transit); opacity: 0.4; color: var(--color-text-muted); }
                .lms-report-chevron.rotate { transform: rotate(90deg); opacity: 1; color: var(--color-primary); }
                
                .lms-status-icon-bg {
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    border-radius: 50%;
                    font-weight: 800;
                    border: 1px solid var(--color-border-bright);
                    transition: transform var(--transit);
                    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.04);
                }
                .lms-report-main-row:hover .lms-status-icon-bg {
                    transform: scale(1.05);
                }

                .lms-report-student-email {
                    font-size: 11px;
                    color: var(--color-text-dim);
                }
                
                .lms-report-group-name {
                    font-size: 13px;
                    font-weight: 600;
                    color: var(--color-text-muted);
                    display: flex;
                    align-items: center;
                }
                .lms-report-group-icon {
                    margin-right: 6px;
                    opacity: 0.5;
                    color: var(--color-primary);
                }

                .lms-report-domain-title {
                    font-weight: 700;
                    color: var(--color-primary);
                }
                .lms-report-domain-instructor {
                    font-size: 11px;
                    color: var(--color-text-dim);
                }

                .lms-report-progress-container {
                    min-width: 120px;
                }
                .lms-report-progress-text {
                    font-size: 11px;
                    font-weight: 800;
                    color: var(--color-text);
                }
                .lms-report-progress-mods {
                    font-size: 10px;
                    color: var(--color-text-dim);
                }
                .lms-progress-track {
                    height: 6px;
                    background: var(--color-border-bright);
                    border-radius: 10px;
                    overflow: hidden;
                    margin-top: 4px;
                }
                .lms-progress-fill {
                    height: 100%;
                    border-radius: 10px;
                    transition: width 0.8s cubic-bezier(0.4, 0, 0.2, 1);
                }

                .lms-report-activity {
                    font-size: 13px;
                    font-weight: 600;
                    color: var(--color-text-muted);
                }
                .lms-report-activity-sub {
                    font-size: 10px;
                    color: var(--color-text-dim);
                    margin-top: 2px;
                }

                .lms-report-details-row td { 
                    padding: 0 !important;
                    background: var(--color-bg) !important;
                }
                .lms-report-nested-content { 
                    padding: 28px 40px; 
                    border-left: 4px solid var(--color-primary); 
                    position: relative; 
                    background: var(--color-bg-alt);
                    margin: 8px 24px 16px 24px;
                    border-radius: 0 var(--radius-md) var(--radius-md) 0;
                    box-shadow: inset 0 2px 8px rgba(0, 0, 0, 0.04), var(--shadow-sm);
                    border: 1px solid var(--color-border-bright);
                    border-left: 4px solid var(--color-primary);
                }
                .lms-report-details-header { 
                    font-size: 11px; 
                    font-weight: 900; 
                    color: var(--color-text-dim); 
                    letter-spacing: 2px; 
                    border-bottom: 1px solid var(--color-border-bright); 
                    padding-bottom: 12px; 
                    margin-bottom: 20px; 
                    text-transform: uppercase;
                }
                
                .lms-report-video-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 16px; }
                .lms-report-video-item { 
                    background: var(--color-bg); 
                    border: 1px solid var(--color-border-bright); 
                    padding: 16px 20px; 
                    border-radius: 14px; 
                    display: flex; 
                    justify-content: space-between; 
                    align-items: center; 
                    transition: all var(--transit); 
                    box-shadow: var(--shadow-sm);
                }
                .lms-report-video-item:hover { 
                    transform: translateY(-2px); 
                    border-color: var(--color-primary); 
                    box-shadow: var(--shadow-md);
                    background: var(--color-bg-alt);
                }
                .lms-report-v-info { display: flex; align-items: center; gap: 12px; min-width: 0; }
                .lms-report-v-title { font-size: 13px; font-weight: 700; color: var(--color-text); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
                .lms-report-v-stats { display: flex; align-items: center; gap: 12px; flex-shrink: 0; }
                .lms-report-v-pct { font-size: 11px; font-weight: 800; color: var(--color-text-dim); }
                .lms-report-v-dot { width: 8px; height: 8px; border-radius: 50%; }
                
                .lms-details-loading { font-size: 14px; color: var(--color-text-dim); font-style: italic; padding: 40px 0; text-align: center; }
                .lms-report-empty, .lms-report-error { font-size: 13px; color: var(--color-text-dim); font-style: italic; padding: 40px 0; text-align: center; }

                @media (max-width: 1100px) {
                    .lms-reports-hero-premium { flex-direction: column; align-items: flex-start; gap: 20px; padding: 24px; }
                    .lms-stat-card-pro { width: 100%; }
                }
                
                @media (max-width: 600px) {
                    .lms-hero-title { font-size: 24px; }
                    
                    .lms-table-main tr.lms-report-details-row {
                        background: transparent !important;
                        border: none !important;
                        box-shadow: none !important;
                        padding: 0 !important;
                        margin-top: -8px !important;
                        margin-bottom: 12px !important;
                    }
                    .lms-report-details-row td {
                        background: transparent !important;
                    }
                    .lms-report-nested-content { 
                        padding: 16px 20px; 
                        margin: 0 0 10px 0; 
                        border-radius: 12px; 
                        border: 1px solid var(--color-border-bright);
                        border-left: 4px solid var(--color-primary);
                        background: var(--color-bg-alt);
                    }
                    
                    .lms-report-video-grid { grid-template-columns: 1fr; }
                    .lms-premium-date { width: 100% !important; min-width: 0 !important; }
                    .lms-entity-filters { flex-direction: column !important; align-items: stretch !important; }
                }
                
                @media (max-width: 380px) {
                    .lms-hero-title { font-size: 20px; }
                    .lms-reports-hero-premium { padding: 20px; }
                    .lms-stat-val { font-size: 28px; }
                }
            `}</style>
        </div>
    );
};
