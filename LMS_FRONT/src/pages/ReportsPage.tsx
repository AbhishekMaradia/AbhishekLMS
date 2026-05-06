import React, { useState } from 'react';
import { Icons, PerspectiveSwitcher, SearchInput, Pagination, CommonTable, CommonGrid, CustomSelect } from '../shared/components/lms/LmsComponents';
import { apiClient as api } from '../core/api/apiClient';

export const ReportsPage: React.FC<any> = ({
    db, searchTerm, setSearchTerm, pagination, changePage, changePageSize, ui, isSuperAdmin, filters, setFilters
}) => {
    const list = db.reports || [];
    const p = pagination['reports'] || { page: 1, size: 20, total: 0 };
    const totalPages = Math.ceil((p.total || 0) / (p.size || 20)) || 1;

    const [expandedRow, setExpandedRow] = useState<string | null>(null);
    const [details, setDetails] = useState<any>(null);
    const [detailsLoading, setDetailsLoading] = useState(false);

    const headers = [
        { header: '', key: 'exp', width: '40px' },
        { header: 'Student Identity', key: 'user' },
        { header: 'Organization', key: 'org', hideOnMobile: !isSuperAdmin },
        { header: 'Cohort / Group', key: 'group', hideOnMobile: true },
        { header: 'Knowledge Domain', key: 'course' },
        { header: 'Progress', key: 'progress' },
        { header: 'Activity', key: 'date', hideOnMobile: true },
        { header: 'Actions', key: 'actions', className: 'lms-text-right' }
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

    return (
        <div className="lms-page lms-fade-in">
            <header className="lms-reports-hero-premium">
                <div className="lms-hero-main">
                    <div className="lms-pro-tag">ACADEMIC PERFORMANCE HUB</div>
                    <h1 className="lms-hero-title">{isSuperAdmin ? 'Global Performance Audit' : 'Organization Progress Audit'}</h1>
                    <p className="lms-hero-desc">Monitoring student velocity and engagement across all active nodes.</p>
                </div>
                <div className="lms-hero-stats">
                    <div className="lms-stat-card-pro">
                        <div className="lms-stat-val">{averageImmersion}%</div>
                        <div className="lms-stat-lbl">PLATFORM AVG IMMERSION</div>
                    </div>
                </div>
            </header>

            <div className="lms-premium-card" style={{ marginTop: '20px' }}>
                <div className="lms-entity-filters" style={{ border: 'none', padding: '12px 16px', gap: '16px', flexWrap: 'wrap' }}>
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
                                    <td>
                                        <Icons.ChevronRight s={16} className={`lms-report-chevron ${isExp ? 'rotate' : ''}`} />
                                    </td>
                                    <td>
                                        <div className="lms-flex-row" style={{ gap: '12px' }}>
                                            <div className="lms-status-icon-bg" style={{ width: '36px', height: '36px', fontSize: '11px', background: 'var(--color-primary-soft)', color: 'var(--color-primary)' }}>
                                                {(item.userName || item.userEmail || "U").charAt(0).toUpperCase()}
                                            </div>
                                            <div>
                                                <div className="lms-cell-bold">{item.userName || 'Anonymous Student'}</div>
                                                <div style={{ fontSize: '11px', opacity: 0.5 }}>{item.userEmail}</div>
                                            </div>
                                        </div>
                                    </td>
                                    {isSuperAdmin && (
                                        <td className="lms-hide-mobile">
                                            <span className="lms-tag info">
                                                {item.orgName || 'Platform'}
                                            </span>
                                        </td>
                                    )}
                                    <td className="lms-hide-mobile">
                                        <div style={{ fontSize: '13px', fontWeight: 600, color: 'var(--color-text-muted)' }}>
                                            <Icons.Users s={12} style={{ marginRight: '6px', opacity: 0.5 }} />
                                            {item.groupName || 'General'}
                                        </div>
                                    </td>
                                    <td>
                                        <div>
                                            <div className="lms-cell-bold" style={{ color: 'var(--color-primary)' }}>{item.courseTitle}</div>
                                            <div style={{ fontSize: '11px', opacity: 0.5 }}>by {item.courseInstructor || 'Senior Faculty'}</div>
                                        </div>
                                    </td>
                                    <td>
                                        <div style={{ minWidth: '120px' }}>
                                            <div className="lms-flex-row" style={{ justifyContent: 'space-between', marginBottom: '4px' }}>
                                                <span style={{ fontSize: '11px', fontWeight: 900 }}>{item.completionPercentage}%</span>
                                                <span style={{ fontSize: '10px', opacity: 0.5 }}>{item.completedVideos}/{item.totalVideos} MODS</span>
                                            </div>
                                            <div style={{ height: '4px', background: 'var(--color-border)', borderRadius: '10px', overflow: 'hidden' }}>
                                                <div style={{
                                                    height: '100%',
                                                    width: `${item.completionPercentage}%`,
                                                    background: item.completionPercentage >= 100 ? '#10b981' : 'var(--color-primary)',
                                                    transition: '0.8s cubic-bezier(0.4, 0, 0.2, 1)'
                                                }} />
                                            </div>
                                        </div>
                                    </td>
                                    <td className="lms-hide-mobile">
                                        <div style={{ fontSize: '13px', opacity: 0.7 }}>
                                            {item.lastUpdated ? new Date(item.lastUpdated).toLocaleDateString() : 'N/A'}
                                            <div style={{ fontSize: '10px', opacity: 0.5 }}>Activity Sync</div>
                                        </div>
                                    </td>
                                    <td className="lms-text-right">
                                        <div className="lms-cell-actions">
                                            <button className="lms-icon-btn-sm info" title="View Detailed Audit">
                                                <Icons.Eye s={16} />
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
                .lms-reports-hero-premium { 
                    display: flex; 
                    justify-content: space-between; 
                    align-items: center; 
                    padding: 32px 40px; 
                    background: var(--color-bg-alt); 
                    border-radius: 24px; 
                    border: 1px solid var(--color-border); 
                    box-shadow: var(--shadow-md); 
                }
                .lms-pro-tag { font-size: 10px; font-weight: 950; color: var(--color-primary); letter-spacing: 2px; }
                .lms-hero-title { font-size: 32px; font-weight: 900; margin: 4px 0; color: var(--color-primary); }
                .lms-hero-desc { font-size: 14px; color: var(--color-text-muted); }
                
                .lms-stat-card-pro { 
                    text-align: center; 
                    background: var(--color-bg); 
                    padding: 16px 32px; 
                    border-radius: 20px; 
                    border: 1px solid var(--color-border); 
                    box-shadow: var(--shadow-sm); 
                }
                .lms-stat-val { font-size: 32px; font-weight: 950; color: var(--color-primary); }
                .lms-stat-lbl { font-size: 10px; font-weight: 900; color: var(--color-text-dim); margin-top: 4px; }

                .lms-premium-select, .lms-premium-date { 
                    padding: 10px 16px; 
                    border-radius: 12px; 
                    border: 1px solid var(--color-border); 
                    background: var(--color-bg-alt); 
                    font-size: 13px; 
                    font-weight: 700; 
                    color: var(--color-text); 
                    outline: none; 
                    cursor: pointer; 
                    transition: 0.2s; 
                    min-width: 180px;
                }
                .lms-premium-select:hover, .lms-premium-date:hover { border-color: var(--color-primary); }

                .lms-report-main-row { cursor: pointer; transition: background 0.15s ease; border-bottom: 1px solid var(--color-border); }
                .lms-report-main-row:hover { background: var(--color-nav-hover); }
                .lms-report-main-row.active { background: var(--color-primary-soft); }
                
                .lms-report-chevron { transition: transform 0.3s ease, color 0.3s ease; opacity: 0.3; color: var(--color-text); }
                .lms-report-chevron.rotate { transform: rotate(90deg); opacity: 1; color: var(--color-primary); }
                
                .lms-report-details-row { background: var(--color-bg); }
                .lms-report-nested-content { 
                    padding: 24px 40px 32px 64px; 
                    border-left: 4px solid var(--color-primary); 
                    position: relative; 
                    background: var(--color-bg-alt);
                    margin: 8px 16px;
                    border-radius: 0 16px 16px 0;
                }
                .lms-report-details-header { 
                    font-size: 11px; 
                    font-weight: 900; 
                    color: var(--color-text-dim); 
                    letter-spacing: 1.5px; 
                    border-bottom: 1px solid var(--color-border); 
                    padding-bottom: 12px; 
                    margin-bottom: 20px; 
                    text-transform: uppercase;
                }
                
                .lms-report-video-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 12px; }
                .lms-report-video-item { 
                    background: var(--color-bg); 
                    border: 1px solid var(--color-border); 
                    padding: 14px 18px; 
                    border-radius: 12px; 
                    display: flex; 
                    justify-content: space-between; 
                    align-items: center; 
                    transition: 0.2s; 
                }
                .lms-report-video-item:hover { transform: translateY(-2px); border-color: var(--color-primary); }
                .lms-report-v-info { display: flex; align-items: center; gap: 12px; }
                .lms-report-v-title { font-size: 13px; font-weight: 700; color: var(--color-text); }
                .lms-report-v-stats { display: flex; align-items: center; gap: 16px; }
                .lms-report-v-pct { font-size: 11px; font-weight: 900; color: var(--color-text-dim); }
                .lms-report-v-dot { width: 8px; height: 8px; border-radius: 50%; }
                
                .lms-details-loading { font-size: 14px; color: var(--color-text-dim); font-style: italic; padding: 40px 0; text-align: center; }
                .lms-report-empty, .lms-report-error { font-size: 13px; color: var(--color-text-dim); font-style: italic; padding: 40px 0; text-align: center; }

                @media (max-width: 1100px) {
                    .lms-reports-hero-premium { flex-direction: column; align-items: flex-start; gap: 20px; padding: 24px; }
                    .lms-stat-card-pro { width: 100%; }
                }
                
                @media (max-width: 600px) {
                    .lms-hero-title { font-size: 24px; }
                    .lms-report-nested-content { padding: 16px 16px 20px 24px; margin: 8px 0; border-radius: 12px; }
                    .lms-report-video-grid { grid-template-columns: 1fr; }
                    .lms-premium-select, .lms-premium-date, .lms-custom-select-container { width: 100% !important; min-width: 0 !important; }
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
