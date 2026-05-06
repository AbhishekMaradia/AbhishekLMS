import React, { useState, useEffect, useMemo } from 'react';
import { Icons, Card, CommonTable } from '../../../shared/components/lms/LmsComponents';
import { apiClient as api } from '../../../core/api/apiClient';
import { TableSkeleton } from '../../../shared/components/lms/LmsSkeleton';

interface StudentReportsProps {
    user: any;
    subscriptions: number[];
    courses: any[];
}

/**
 * Enterprise Academic Transcript - Detailed Edition
 * Now features granular module breakdown to see exactly which video is watched fully or half.
 */
const StudentReports: React.FC<StudentReportsProps> = ({ user, subscriptions, courses }) => {
    const [reports, setReports] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);
    const [expandedRows, setExpandedRows] = useState<number[]>([]);

    const enrolled = useMemo(() => {
        return courses.filter(c => {
            const id = Number(c.courseId || c.CourseId || c.id || c.Id);
            return subscriptions.includes(id);
        });
    }, [courses, subscriptions]);

    useEffect(() => {
        if (!user?.id || enrolled.length === 0) {
            setLoading(false);
            setReports([]);
            return;
        }

        let isMounted = true;
        const fetchReports = async () => {
            setLoading(true);
            try {
                const results = await Promise.all(
                    enrolled.map(async (c) => {
                        const cid = Number(c.courseId || c.CourseId || c.id || c.Id);
                        try {
                            const res = await api.get(`reports/course/${user.id}/${cid}`);
                            const reportData = res.data?.data;
                            const data = Array.isArray(reportData) ? reportData[0] : reportData;
                            // Data now includes 'videoDetails' from our backend fix
                            return { ...c, ...(data || { totalVideos: 0, completedVideos: 0, completionPercentage: 0, videoDetails: [] }) };
                        } catch (e) {
                            return { ...c, totalVideos: 0, completedVideos: 0, completionPercentage: 0, videoDetails: [] };
                        }
                    })
                );
                if (isMounted) setReports(results);
            } catch (err) {
                console.error("[LMS] Performance Sync Failure:", err);
            } finally {
                if (isMounted) setLoading(false);
            }
        };

        fetchReports();
        return () => { isMounted = false; };
    }, [user?.id, enrolled]);

    const totalProgress = useMemo(() => {
        if (reports.length === 0) return 0;
        const sum = reports.reduce((acc, r) => acc + (r.completionPercentage || 0), 0);
        return Math.round(sum / reports.length);
    }, [reports]);

    const toggleRow = (id: number) => {
        setExpandedRows(prev => prev.includes(id) ? prev.filter(r => r !== id) : [...prev, id]);
    };

    if (loading) return <div className="lms-reports-pro lms-fade-in"><TableSkeleton rows={5} /></div>;

    return (
        <div className="lms-reports-pro lms-fade-in theme-sync">
            <header className="reports-hero">
                <div className="hero-content">
                    <div className="pro-tag">DETAILED PERFORMANCE AUDIT</div>
                    <h1 className="hero-title">Academic Transcript</h1>
                </div>
                <div className="hero-stats">
                    <div className="overall-progress-card">
                        <div className="percentage">{totalProgress}%</div>
                        <div className="card-label">TOTAL IMMERSION</div>
                    </div>
                </div>
            </header>

            <div className="transcript-section">
                <div className="transcript-card">
                    <table className="pro-transcript-table">
                        <thead>
                            <tr>
                                <th style={{ width: '40px' }}></th>
                                <th>LEARNING DOMAIN</th>
                                <th>MODULES</th>
                                <th>ENGAGEMENT</th>
                                <th>STATUS</th>
                            </tr>
                        </thead>
                        <tbody>
                            {reports.map((r, idx) => {
                                const cid = r.courseId || r.Id || idx;
                                const isExp = expandedRows.includes(cid);
                                const pct = Number(r.completionPercentage || 0).toFixed(1);
                                
                                return (
                                    <React.Fragment key={cid}>
                                        <tr className={`main-row ${isExp ? 'exp' : ''}`} onClick={() => toggleRow(cid)}>
                                            <td><Icons.ChevronRight s={16} className={`chevron ${isExp ? 'rotate' : ''}`} /></td>
                                            <td>
                                                <div className="domain-cell">
                                                    <div className="domain-info">
                                                        <div className="domain-title">{r.title || r.courseName}</div>
                                                        <div className="domain-cat">{r.categoryName || 'Core Curriculum'}</div>
                                                    </div>
                                                </div>
                                            </td>
                                            <td><span className="audit-val">{r.completedVideos} / {r.totalVideos}</span></td>
                                            <td>
                                                <div className="engagement-wrap">
                                                    <div className="engagement-bar"><div className="fill" style={{ width: `${pct}%` }} /></div>
                                                    <span className="pct-text">{pct}%</span>
                                                </div>
                                            </td>
                                            <td><div className={`status-badge ${Number(pct) >= 100 ? 'cert' : 'active'}`}>{Number(pct) >= 100 ? 'CERTIFIED' : 'ACTIVE'}</div></td>
                                        </tr>
                                        {isExp && (
                                            <tr className="details-row">
                                                <td colSpan={5}>
                                                    <div className="nested-details lms-fade-in">
                                                        <header className="details-header">SUB-MODULE BREAKDOWN</header>
                                                        <div className="video-details-grid">
                                                            {r.videoDetails?.map((v: any) => (
                                                                <div key={v.videoId} className="video-progress-item">
                                                                    <div className="v-info">
                                                                        <Icons.Play s={12} />
                                                                        <span className="v-title">{v.title}</span>
                                                                    </div>
                                                                    <div className="v-stats">
                                                                        <span className="v-pct">{Number(v.watchedPercentage).toFixed(1)}% Viewed</span>
                                                                        <div className="v-status-dot" style={{ background: v.isCompleted ? '#10b981' : '#fbbf24' }} />
                                                                    </div>
                                                                </div>
                                                            ))}
                                                            {(!r.videoDetails || r.videoDetails.length === 0) && <p className="empty-sub">No video breakdown available for this domain.</p>}
                                                        </div>
                                                    </div>
                                                </td>
                                            </tr>
                                        )}
                                    </React.Fragment>
                                );
                            })}
                        </tbody>
                    </table>
                </div>
            </div>

            <style>{`
                .lms-reports-pro { padding: 40px; max-width: 1400px; margin: 0 auto; font-family: 'Outfit', sans-serif; }
                .reports-hero { display: flex; justify-content: space-between; align-items: center; margin-bottom: 40px; }
                .pro-tag { font-size: 10px; font-weight: 900; color: var(--color-primary); letter-spacing: 2px; }
                .hero-title { font-size: 32px; font-weight: 900; margin: 8px 0; }
                .overall-progress-card { text-align: center; background: #fff; padding: 20px 30px; border-radius: 20px; border: 1px solid #eee; }
                .overall-progress-card .percentage { font-size: 24px; font-weight: 950; color: var(--color-primary); }
                .card-label { font-size: 9px; font-weight: 900; color: #bbb; margin-top: 4px; }

                .transcript-card { background: #fff; border-radius: 24px; border: 1px solid #eee; overflow: hidden; box-shadow: 0 20px 40px rgba(0,0,0,0.02); }
                .pro-transcript-table { width: 100%; border-collapse: collapse; }
                .pro-transcript-table th { padding: 20px 24px; text-align: left; font-size: 10px; font-weight: 900; color: #999; border-bottom: 1px solid #f5f5f5; text-transform: uppercase; }
                
                .main-row { cursor: pointer; transition: 0.2s; }
                .main-row:hover { background: #fafafa; }
                .main-row td { padding: 20px 24px; border-bottom: 1px solid #f9f9f9; }
                .chevron { transition: 0.3s; opacity: 0.3; }
                .chevron.rotate { transform: rotate(90deg); opacity: 1; color: var(--color-primary); }

                .domain-title { font-size: 15px; font-weight: 850; color: #111; }
                .domain-cat { font-size: 11px; color: #aaa; margin-top: 2px; }
                .audit-val { font-size: 13px; font-weight: 900; color: #444; }

                .engagement-wrap { display: flex; align-items: center; gap: 12px; }
                .engagement-bar { flex: 1; min-width: 100px; height: 6px; background: #eee; border-radius: 10px; overflow: hidden; }
                .engagement-bar .fill { height: 100%; background: var(--color-primary); border-radius: 10px; }
                .pct-text { font-size: 12px; font-weight: 900; color: var(--color-primary); min-width: 35px; }

                .status-badge { font-size: 10px; font-weight: 950; padding: 6px 12px; border-radius: 100px; display: inline-block; }
                .status-badge.cert { background: #ecfdf5; color: #10b981; }
                .status-badge.active { background: #eff6ff; color: #3b82f6; }

                .details-row { background: #fcfcfc; }
                .nested-details { padding: 24px 40px 32px 64px; }
                .details-header { font-size: 10px; font-weight: 950; color: #bbb; border-bottom: 1px solid #eee; padding-bottom: 10px; margin-bottom: 16px; }
                
                .video-details-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
                .video-progress-item { background: #fff; border: 1px solid #eee; padding: 12px 16px; border-radius: 12px; display: flex; justify-content: space-between; align-items: center; }
                .v-info { display: flex; align-items: center; gap: 10px; }
                .v-title { font-size: 12px; font-weight: 850; color: #444; }
                .v-stats { display: flex; align-items: center; gap: 12px; }
                .v-pct { font-size: 11px; font-weight: 900; color: #888; }
                .v-status-dot { width: 6px; height: 6px; border-radius: 50%; }

                .empty-sub { color: #ccc; font-size: 12px; font-style: italic; }

                /* Mobile Optimization */
                @media (max-width: 900px) {
                    .lms-reports-pro { padding: 20px; }
                    .reports-hero { flex-direction: column; align-items: flex-start; gap: 24px; }
                    .hero-title { font-size: 28px; }
                    .transcript-card { border-radius: 16px; overflow-x: auto; }
                    .pro-transcript-table { min-width: 600px; }
                    .nested-details { padding: 20px; }
                    .video-details-grid { grid-template-columns: 1fr; }
                }
            `}</style>
        </div>
    );
};

export default StudentReports;
