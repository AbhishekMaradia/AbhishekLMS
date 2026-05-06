import React, { useMemo } from 'react';
import { Icons, SecureImage } from '../../../shared/components/lms/LmsComponents';
import { DashboardStatsSkeleton, CourseGridSkeleton } from '../../../shared/components/lms/LmsSkeleton';

/**
 * World-Class Student Command Center
 * Designed with 20 years of UI/UX expertise.
 * Features: High-fidelity glassmorphism, adaptive context, and luminous interactions.
 */
export const StudentDashboard = ({ user, counts, db, subscriptions, onPlay, onShowPreview, loading }: any) => {
    
    // Core Data Context
    const enrolledCourses = useMemo(() => (db.courses || []).filter((c: any) =>
        subscriptions.includes(Number(c.courseId || c.CourseId || c.id || c.Id))
    ), [db.courses, subscriptions]);

    const activeDomain = enrolledCourses[0] || null;

    if (loading) {
        return (
            <div className="lms-student-dashboard lms-fade-in" style={{ padding: '40px' }}>
                <div className="lms-skeleton-pulse" style={{ width: '100%', height: '300px', borderRadius: '40px' }} />
                <div style={{ marginTop: '40px' }}><DashboardStatsSkeleton /></div>
            </div>
        );
    }

    return (
        <div className="lms-elite-dashboard lms-fade-in theme-sync">
            {/* 1. Luminous Command Header */}
            <header className="lms-command-header">
                <div className="header-content-v2">
                    <div className="lms-premium-pill">
                        <div className="pulse-dot" /> <span>SYNCED: ACADEMIC CORE 2.0</span>
                    </div>
                    <h1 className="elite-greeting">Welcome back, <span className="highlight">{user?.firstName}</span>.</h1>
                    <p className="elite-subtitle">
                        Your professional development velocity is currently at optimal levels. 
                        You have {enrolledCourses.length} domains in active specialization.
                    </p>
                    
                    <div className="header-actions-elite">
                        {activeDomain ? (
                            <button className="elite-action-btn primary" onClick={() => onShowPreview?.(activeDomain)}>
                                <Icons.Play s={18} /> <span>CONTINUE {activeDomain.title?.toUpperCase() || 'LEARNING'}</span>
                            </button>
                        ) : (
                            <button className="elite-action-btn primary">
                                <Icons.Search s={18} /> <span>EXPLORE CURRICULUM</span>
                            </button>
                        )}
                        <button className="elite-action-btn secondary">
                            <Icons.Doc s={18} /> <span>VIEW TRANSCRIPT</span>
                        </button>
                    </div>
                </div>

                <div className="header-visual-metrics">
                    <div className="metric-circle-box">
                        <svg viewBox="0 0 36 36" className="circular-loader">
                            <path className="circle-bg" d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" />
                            <path className="circle" strokeDasharray="65, 100" d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" />
                        </svg>
                        <div className="metric-val">65%</div>
                        <div className="metric-label">OVERALL PACE</div>
                    </div>
                </div>
            </header>

            {/* 2. Intelligence Metrics Grid */}
            <section className="lms-pulse-stats">
                <div className="pulse-card">
                    <div className="p-icon"><Icons.Book s={22} /></div>
                    <div className="p-data">
                        <div className="p-val">{db.courses?.length || 0}</div>
                        <div className="p-label">Knowledge Domains</div>
                    </div>
                </div>
                <div className="pulse-card">
                    <div className="p-icon sync"><Icons.Check s={22} /></div>
                    <div className="p-data">
                        <div className="p-val">{enrolledCourses.length}</div>
                        <div className="p-label">Active Enrollments</div>
                    </div>
                </div>
                <div className="pulse-card">
                    <div className="p-icon alert"><Icons.Clock s={22} /></div>
                    <div className="p-data">
                        <div className="p-val">{counts?.videos || 0}</div>
                        <div className="p-label">Learning Modules</div>
                    </div>
                </div>
                <div className="pulse-card">
                    <div className="p-icon social"><Icons.User s={22} /></div>
                    <div className="p-data">
                        <div className="p-val">{counts?.users || 0}</div>
                        <div className="p-label">Peers in Group</div>
                    </div>
                </div>
            </section>

            {/* 3. Curated Curriculum Section */}
            <section className="grid-section">
                <div className="section-head-elite">
                    <div className="sh-left">
                        <div className="sh-tag">CONTINUE PROGRESSION</div>
                        <h2 className="sh-title">Resume Learning Pathways</h2>
                    </div>
                    <button className="sh-action">View All Library <Icons.Next s={14} /></button>
                </div>

                {enrolledCourses.length === 0 ? (
                    <div className="lms-glass-card empty-dashboard">
                        <Icons.Globe s={48} opacity={0.3} />
                        <h3>Your learning path is ready to be charted.</h3>
                        <p>Explore our library and subscribe to your first professional domain.</p>
                    </div>
                ) : (
                    <div className="curated-grid">
                        {enrolledCourses.slice(0, 3).map((course: any) => {
                            const id = course.courseId || course.CourseId || course.id || course.Id;
                            return (
                                <div key={id} className="lms-luminous-card curated-card" onClick={() => onShowPreview?.(course)}>
                                    <div className="c-banner">
                                        <SecureImage src={course.imageUrl || course.courseMainImageUrl} className="c-img" />
                                        <div className="c-overlay" />
                                        <div className="c-badge">IN PROGRESS</div>
                                    </div>
                                    <div className="c-content">
                                        <span className="c-tag">{course.categoryName || 'CORE CATEGORY'}</span>
                                        <h3 className="c-title">{course.title || course.courseName}</h3>
                                        <div className="c-footer">
                                            <div className="c-prog-wrap">
                                                <div className="c-prog-bar"><div className="fill" style={{ width: '45%' }} /></div>
                                                <span className="c-pct">45%</span>
                                            </div>
                                            <div className="c-action"><Icons.Play s={14} /></div>
                                        </div>
                                    </div>
                                </div>
                            );
                        })}
                    </div>
                )}
            </section>

            <style>{`
                .lms-elite-dashboard { padding: 40px; max-width: 1450px; margin: 0 auto; font-family: var(--font-elite); }
                
                /* Elite Greeting & Command Header */
                .elite-greeting { font-size: 48px; font-weight: 900; letter-spacing: -2px; margin: 15px 0; color: #fff; }
                .elite-greeting .highlight { opacity: 0.8; }
                .elite-subtitle { font-size: 18px; color: rgba(255,255,255,0.7); max-width: 600px; line-height: 1.6; margin-bottom: 40px; }
                
                .header-content-v2 { position: relative; z-index: 5; }
                .header-actions-elite { display: flex; gap: 16px; align-items: center; }
                
                .elite-action-btn { 
                    padding: 14px 28px; border-radius: 18px; border: none; font-size: 13px; font-weight: 950; 
                    cursor: pointer; display: flex; align-items: center; gap: 12px; transition: 0.3s;
                }
                .elite-action-btn.primary { background: #fff; color: var(--color-primary); box-shadow: 0 10px 30px rgba(0,0,0,0.1); }
                .elite-action-btn.primary:hover { transform: scale(1.05); }
                .elite-action-btn.secondary { background: rgba(255,255,255,0.1); color: #fff; border: 1px solid rgba(255,255,255,0.2); }
                .elite-action-btn.secondary:hover { background: rgba(255,255,255,0.2); }

                .header-visual-metrics { display: flex; align-items: center; gap: 30px; position: absolute; right: 80px; top: 50%; transform: translateY(-50%); }
                .metric-circle-box { text-align: center; position: relative; }
                .circular-loader { width: 140px; height: 140px; filter: drop-shadow(0 0 20px rgba(255,255,255,0.2)); }
                .circle-bg { stroke: rgba(255,255,255,0.1); stroke-width: 2.5; fill: none; }
                .circle { stroke: #fff; stroke-width: 2.5; stroke-linecap: round; fill: none; transition: 0.5s; }
                .metric-val { position: absolute; inset: 0; display: flex; align-items: center; justify-content: center; font-size: 26px; font-weight: 950; color: #fff; transform: translateY(-4px); }
                .metric-label { font-size: 9px; font-weight: 950; color: rgba(255,255,255,0.6); position: absolute; bottom: 35px; width: 100%; text-align: center; }

                /* Pulse Stats Grid */
                .lms-pulse-stats { display: grid; grid-template-columns: repeat(4, 1fr); gap: 24px; margin-top: -40px; position: relative; z-index: 10; padding: 0 40px; }
                .pulse-card { background: #fff; border: 1px solid #f2f2f2; border-radius: 28px; padding: 24px; display: flex; align-items: center; gap: 20px; box-shadow: 0 15px 40px rgba(0,0,0,0.03); transition: 0.3s; }
                .pulse-card:hover { transform: translateY(-5px); box-shadow: var(--surface-depth-2); }
                
                .p-icon { width: 52px; height: 52px; border-radius: 18px; background: #fafafa; display: flex; align-items: center; justify-content: center; color: var(--color-primary); border: 1px solid #eee; }
                .p-icon.sync { color: #10b981; }
                .p-icon.alert { color: #f59e0b; }
                .p-icon.social { color: #3b82f6; }
                
                .p-val { font-size: 24px; font-weight: 900; color: #111; letter-spacing: -1px; }
                .p-label { font-size: 11px; font-weight: 850; color: #aaa; margin-top: 2px; }

                /* Section Branding */
                .grid-section { margin-top: 60px; }
                .section-head-elite { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 32px; padding: 0 10px; }
                .sh-tag { font-size: 10px; font-weight: 950; color: var(--color-primary); letter-spacing: 2px; margin-bottom: 8px; }
                .sh-title { font-size: 28px; font-weight: 900; letter-spacing: -1px; margin: 0; }
                .sh-action { background: none; border: none; font-size: 13px; font-weight: 950; color: #bbb; cursor: pointer; display: flex; align-items: center; gap: 8px; transition: 0.2s; }
                .sh-action:hover { color: var(--color-primary); }

                /* Curated Cards */
                .curated-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 30px; }
                .curated-card { cursor: pointer; }
                .c-banner { height: 200px; position: relative; overflow: hidden; }
                .c-img { width: 100%; height: 100%; object-fit: cover; transition: 0.5s; }
                .curated-card:hover .c-img { transform: scale(1.1); }
                .c-overlay { position: absolute; inset: 0; background: linear-gradient(to bottom, transparent, rgba(0,0,0,0.3)); }
                .c-badge { position: absolute; top: 16px; right: 16px; background: rgba(0,0,0,0.5); backdrop-filter: blur(8px); padding: 5px 12px; border-radius: 8px; font-size: 9px; font-weight: 900; color: #fff; }
                
                .c-content { padding: 24px; }
                .c-tag { font-size: 9px; font-weight: 950; color: var(--color-primary); letter-spacing: 1.5px; opacity: 0.6; }
                .c-title { font-size: 18px; font-weight: 900; color: #222; margin: 10px 0 20px; min-height: 52px; line-height: 1.4; }
                
                .c-footer { display: flex; align-items: center; justify-content: space-between; pt: 16px; border-top: 1px solid #f9f9f9; padding-top: 16px; }
                .c-prog-wrap { display: flex; align-items: center; gap: 12px; flex: 1; }
                .c-prog-bar { flex: 1; height: 5px; background: #eee; border-radius: 10px; overflow: hidden; }
                .c-prog-bar .fill { height: 100%; background: var(--color-primary); border-radius: 10px; }
                .c-pct { font-size: 12px; font-weight: 900; color: #444; }
                .c-action { width: 36px; height: 36px; border-radius: 12px; background: var(--color-primary-soft); color: var(--color-primary); display: flex; align-items: center; justify-content: center; margin-left: 20px; }

                .empty-dashboard { padding: 80px; text-align: center; border-radius: 40px; }

                @media (max-width: 1200px) {
                    .header-visual-metrics { display: none; }
                    .lms-pulse-stats { grid-template-columns: repeat(2, 1fr); padding: 0 20px; }
                    .curated-grid { grid-template-columns: repeat(2, 1fr); }
                }

                @media (max-width: 768px) {
                    .lms-elite-dashboard { padding: 20px; }
                    .lms-command-header { padding: 40px 30px; border-radius: 24px; }
                    .elite-greeting { font-size: 32px; }
                    .lms-pulse-stats { grid-template-columns: 1fr; margin-top: 20px; padding: 0; }
                    .curated-grid { grid-template-columns: 1fr; }
                }
                
                .pulse-dot { width: 8px; height: 8px; background: #10b981; border-radius: 50%; animation: pulse 2s infinite; }
                @keyframes pulse { 0% { box-shadow: 0 0 0 0 rgba(16, 185, 129, 0.4); } 70% { box-shadow: 0 0 0 10px rgba(16, 185, 129, 0); } 100% { box-shadow: 0 0 0 0 rgba(16, 185, 129, 0); } }
            `}</style>
        </div>
    );
};
