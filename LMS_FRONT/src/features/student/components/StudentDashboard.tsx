import React, { useMemo, useState, useEffect } from 'react';
import { Icons, SecureImage } from '../../../shared/components/lms/LmsComponents';
import { DashboardStatsSkeleton } from '../../../shared/components/lms/LmsSkeleton';
import api from '../../../core/api/axios';

/**
 * World-Class Student Command Center
 * Designed with 20 years of UI/UX expertise.
 * Features: High-fidelity glassmorphism, adaptive context, and luminous interactions.
 */
export const StudentDashboard = ({ user, db, subscriptions, onShowPreview, loading }: any) => {
    
    // Core Data Context
    const enrolledCourses = useMemo(() => (db.courses || []).filter((c: any) =>
        subscriptions.includes(Number(c.courseId || c.CourseId || c.id || c.Id))
    ), [db.courses, subscriptions]);

    const activeDomain = enrolledCourses[0] || null;

    const [progressMap, setProgressMap] = useState<Record<number, number>>({});
    const [modulesCountMap, setModulesCountMap] = useState<Record<number, number>>({});
    const [peersCount, setPeersCount] = useState<number>(0);
    const [groupPeers, setGroupPeers] = useState<any[]>([]);
    const [fetchingDetails, setFetchingDetails] = useState(false);

    useEffect(() => {
        if (!user?.id) return;
        
        let isMounted = true;
        const loadDashboardDetails = async () => {
            setFetchingDetails(true);
            try {
                // 1. Fetch progress & total videos for each enrolled course
                const progressData: Record<number, number> = {};
                const modulesData: Record<number, number> = {};
                await Promise.all(
                    enrolledCourses.map(async (c: any) => {
                        const cid = Number(c.courseId || c.CourseId || c.id || c.Id);
                        try {
                            const res = await api.get(`reports/course/${user.id}/${cid}`);
                            const report = res.data?.data?.[0] || res.data?.data || res.data?.[0] || null;
                            const pct = report?.completionPercentage ?? report?.CompletionPercentage ?? 0;
                            const totalVids = report?.totalVideos ?? report?.TotalVideos ?? 0;
                            progressData[cid] = Math.round(pct);
                            modulesData[cid] = totalVids;
                        } catch (e) {
                            progressData[cid] = 0;
                            modulesData[cid] = 0;
                        }
                    })
                );
                
                // 2. Fetch peers list and count if user is in a group
                let pCount = 0;
                let peersList: any[] = [];
                if (user.groupId) {
                    try {
                        const res = await api.get(`Group/users/${user.groupId}`);
                        const list = res.data?.data || res.data || [];
                        if (Array.isArray(list)) {
                            // Filter out current user from peers list
                            peersList = list.filter((u: any) => u.id !== user.id);
                            pCount = peersList.length;
                        }
                    } catch (e) {
                        pCount = 0;
                    }
                }
                
                if (isMounted) {
                    setProgressMap(progressData);
                    setModulesCountMap(modulesData);
                    setPeersCount(pCount);
                    setGroupPeers(peersList);
                }
            } catch (err) {
                console.error("Failed to load Student Dashboard details", err);
            } finally {
                if (isMounted) setFetchingDetails(false);
            }
        };
        
        loadDashboardDetails();
        return () => { isMounted = false; };
    }, [user?.id, user?.groupId, enrolledCourses]);

    const overallPace = useMemo(() => {
        if (enrolledCourses.length === 0) return 0;
        const total = enrolledCourses.reduce((acc: number, c: any) => {
            const cid = Number(c.courseId || c.CourseId || c.id || c.Id);
            return acc + (progressMap[cid] || 0);
        }, 0);
        return Math.round(total / enrolledCourses.length);
    }, [enrolledCourses, progressMap]);

    const totalLearningModules = useMemo(() => {
        return Object.values(modulesCountMap).reduce((a, b) => a + b, 0);
    }, [modulesCountMap]);

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
                    <h1 className="elite-greeting">
                        Welcome back, <span className="highlight">{user?.firstName}</span>.
                    </h1>
                    <p className="elite-subtitle">
                        Your professional development velocity is currently at optimal levels. 
                        You have {enrolledCourses.length} domain{enrolledCourses.length === 1 ? '' : 's'} in active specialization.
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
                    </div>
                </div>

                <div className="header-visual-metrics">
                    <div className="metric-circle-box">
                        <svg viewBox="0 0 36 36" className="circular-loader">
                            <path className="circle-bg" d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" />
                            <path className="circle" strokeDasharray={`${overallPace}, 100`} d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" />
                        </svg>
                        <div className="metric-val">{overallPace}%</div>
                        <div className="metric-label">OVERALL PACE</div>
                    </div>
                </div>
            </header>

            {/* 2. New Journey Timeline & Goal Achievement Path Section */}
            <section className="lms-journey-timeline-section">
                {/* Column 1: My Journey Timeline */}
                <div className="lms-journey-timeline-card">
                    <div className="card-header-elite">
                        <span className="card-title-elite">My Journey Timeline</span>
                        <Icons.Next s={16} />
                    </div>
                    <div className="timeline-horizontal-path">
                        <div className="timeline-node completed">
                            <div className="node-icon-wrapper"><Icons.Check s={12} /></div>
                            <div className="node-info">
                                <div className="node-title">Started</div>
                                <div className="node-status">Oct 2023</div>
                            </div>
                        </div>
                        <div className="timeline-connector active" />

                        {enrolledCourses.length > 0 ? (
                            enrolledCourses.slice(0, 2).map((course: any, idx: number) => {
                                const cid = Number(course.courseId || course.CourseId || course.id || course.Id);
                                const pct = progressMap[cid] || 0;
                                const isComplete = pct === 100;
                                return (
                                    <React.Fragment key={cid}>
                                        <div className={`timeline-node ${isComplete ? 'completed' : 'active'}`}>
                                            <div className="node-icon-wrapper">
                                                {isComplete ? <Icons.Check s={12} /> : <Icons.Play s={10} />}
                                            </div>
                                            <div className="node-info">
                                                <div className="node-title">{course.title || course.courseName}</div>
                                                <div className="node-status">{isComplete ? 'Complete' : `${pct}% Active`}</div>
                                            </div>
                                        </div>
                                        {idx < enrolledCourses.slice(0, 2).length - 1 && (
                                            <div className="timeline-connector active" />
                                        )}
                                    </React.Fragment>
                                );
                            })
                        ) : (
                            <div className="timeline-node planned">
                                <div className="node-icon-wrapper"><Icons.Book s={12} /></div>
                                <div className="node-info">
                                    <div className="node-title">First Domain</div>
                                    <div className="node-status">Planned</div>
                                </div>
                            </div>
                        )}
                    </div>
                </div>

                {/* Column 2: Goal Achievement Path */}
                <div className="lms-journey-timeline-card">
                    <div className="card-header-elite">
                        <span className="card-title-elite">Goal Achievement Path</span>
                        <Icons.Next s={16} />
                    </div>
                    <div className="timeline-horizontal-path">
                        <div className="timeline-node completed">
                            <div className="node-icon-wrapper"><Icons.Check s={12} /></div>
                            <div className="node-info">
                                <div className="node-title">Junior Cert.</div>
                                <div className="node-status">Complete</div>
                            </div>
                        </div>
                        <div className="timeline-connector active" />
                        <div className="timeline-node active">
                            <div className="node-icon-wrapper"><div className="node-pulse-point" /></div>
                            <div className="node-info">
                                <div className="node-title">Mid-Level</div>
                                <div className="node-status">Active</div>
                            </div>
                        </div>
                        <div className="timeline-connector planned" />
                        <div className="timeline-node planned">
                            <div className="node-icon-wrapper"><Icons.Shield s={12} /></div>
                            <div className="node-info">
                                <div className="node-title">Senior Cert.</div>
                                <div className="node-status">Planned</div>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            {/* 3. Pathways & Community Connections Row */}
            <div className="grid-section">
                <div className="section-head-elite">
                    <div className="sh-left">
                        <div className="sh-tag">CONTINUE PROGRESSION</div>
                        <h2 className="sh-title">Resume Learning Pathways</h2>
                    </div>
                </div>

                <div className="lms-pathways-community-row">
                    
                    {/* Left Side: Resume Learning Pathways */}
                    <div className="lms-pathways-left-col">
                        {enrolledCourses.length === 0 ? (
                            <div className="lms-glass-card empty-dashboard">
                                <Icons.Globe s={48} opacity={0.3} />
                                <h3>Your learning path is ready to be charted.</h3>
                                <p>Explore our library and subscribe to your first professional domain.</p>
                            </div>
                        ) : (
                            <div className="curated-grid">
                                {enrolledCourses.slice(0, 3).map((course: any) => {
                                    const id = Number(course.courseId || course.CourseId || course.id || course.Id);
                                    const pct = progressMap[id] || 0;
                                    
                                    return (
                                        <div key={id} className="lms-luminous-card curated-card" onClick={() => onShowPreview?.(course)}>
                                            <div className="c-banner">
                                                <SecureImage src={course.imageUrl || course.courseMainImageUrl} className="c-img" />
                                                <div className="c-overlay" />
                                                <div className="c-badge">{pct === 100 ? 'COMPLETED' : pct === 0 ? 'not started' : 'IN PROGRESS'}</div>
                                            </div>
                                            <div className="c-content">
                                                <span className="c-tag">{course.categoryName || 'CORE CATEGORY'}</span>
                                                <h3 className="c-title">{course.title || course.courseName}</h3>
                                                <div className="c-footer">
                                                    <div className="c-prog-wrap">
                                                        <div className="c-prog-bar"><div className="fill" style={{ width: `${pct}%` }} /></div>
                                                        <span className="c-pct">{pct}%</span>
                                                    </div>
                                                    <div className="c-action"><Icons.Play s={14} /></div>
                                                </div>
                                            </div>
                                        </div>
                                    );
                                })}

                                {/* Standard placeholders for un-enrolled catalog courses */}
                                {db.courses && db.courses.filter((c: any) => !subscriptions.includes(Number(c.courseId || c.CourseId || c.id || c.Id))).slice(0, 2).map((course: any) => {
                                    const id = Number(course.courseId || course.CourseId || course.id || course.Id);
                                    return (
                                        <div key={id} className="lms-luminous-card curated-card disabled-card" onClick={() => onShowPreview?.(course)}>
                                            <div className="c-banner">
                                                <SecureImage src={course.imageUrl || course.courseMainImageUrl} className="c-img" />
                                                <div className="c-overlay" />
                                                <div className="c-badge">not started</div>
                                            </div>
                                            <div className="c-content">
                                                <span className="c-tag">{course.categoryName || 'CATALOG'}</span>
                                                <h3 className="c-title">{course.title || course.courseName}</h3>
                                                <div className="c-footer">
                                                    <span className="catalog-action-text" style={{ fontSize: '11px', fontWeight: 850, color: 'var(--color-primary)' }}>Enrol</span>
                                                    <div className="c-action secondary" style={{ background: 'var(--color-border)', color: 'var(--color-text)' }}><Icons.Next s={14} /></div>
                                                </div>
                                            </div>
                                        </div>
                                    );
                                })}
                            </div>
                        )}
                    </div>

                    {/* Right Side: Community Connections */}
                    <div className="lms-community-right-col">
                        <div className="card-header-elite">
                            <span className="card-title-elite">Community Connections</span>
                            <Icons.Next s={16} />
                        </div>
                        <div className="lms-community-list">
                            {groupPeers.length > 0 ? (
                                groupPeers.slice(0, 4).map((peer: any) => (
                                    <div key={peer.id} className="lms-community-item">
                                        <div className="peer-avatar-wrapper">
                                            <SecureImage src={peer.imageUrl} className="peer-avatar" />
                                            <span className="peer-active-dot" />
                                        </div>
                                        <div className="peer-info">
                                            <div className="peer-name">{peer.firstName} {peer.lastName}</div>
                                            <div className="peer-activity">Active activities...</div>
                                        </div>
                                    </div>
                                ))
                            ) : (
                                <div className="empty-community">
                                    <Icons.Users s={24} opacity={0.4} />
                                    <div style={{ fontSize: '11px', color: '#999', marginTop: '6px' }}>No active peers in group</div>
                                </div>
                            )}
                        </div>
                    </div>

                </div>
            </div>

            <style>{`
                .lms-elite-dashboard { padding: 20px 24px; max-width: 1450px; margin: 0 auto; font-family: var(--font-elite); }
                
                /* Elite Greeting & Command Header */
                .lms-command-header { padding: 30px 48px; background: linear-gradient(135deg, #745147 0%, #3e2823 100%); border-radius: 28px; position: relative; overflow: hidden; box-shadow: var(--shadow-md); margin-bottom: 24px; color: #fff; }
                .elite-greeting { font-size: 36px; font-weight: 900; letter-spacing: -1.5px; margin: 10px 0; color: #fff; }
                .elite-greeting .highlight { opacity: 0.8; }
                .elite-subtitle { font-size: 15px; color: rgba(255,255,255,0.7); max-width: 600px; line-height: 1.5; margin-bottom: 24px; }
                
                .header-content-v2 { position: relative; z-index: 5; }
                .header-actions-elite { display: flex; gap: 12px; align-items: center; }
                
                .elite-action-btn { 
                    padding: 10px 20px; border-radius: 12px; border: none; font-size: 12px; font-weight: 900; 
                    cursor: pointer; display: flex; align-items: center; gap: 8px; transition: 0.3s;
                }
                .elite-action-btn.primary { background: #fff; color: var(--color-primary); box-shadow: 0 5px 15px rgba(0,0,0,0.05); }
                .elite-action-btn.primary:hover { transform: scale(1.03); }

                .header-visual-metrics { display: flex; align-items: center; gap: 20px; position: absolute; right: 48px; top: 50%; transform: translateY(-50%); }
                .metric-circle-box { text-align: center; position: relative; }
                .circular-loader { width: 100px; height: 100px; filter: drop-shadow(0 0 10px rgba(255,255,255,0.15)); }
                .circle-bg { stroke: rgba(255,255,255,0.1); stroke-width: 2.5; fill: none; }
                .circle { stroke: #fff; stroke-width: 2.5; stroke-linecap: round; fill: none; transition: 0.5s; }
                .metric-val { position: absolute; inset: 0; display: flex; align-items: center; justify-content: center; font-size: 20px; font-weight: 950; color: #fff; transform: translateY(-3px); }
                .metric-label { font-size: 8px; font-weight: 950; color: rgba(255,255,255,0.6); position: absolute; bottom: 24px; width: 100%; text-align: center; }

                /* Journey Timeline Section */
                .lms-journey-timeline-section { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; margin-bottom: 24px; }
                .lms-journey-timeline-card { background: #fff; border: 1px solid #f0f0f0; border-radius: 20px; padding: 16px 20px; box-shadow: 0 5px 15px rgba(0,0,0,0.01); display: flex; flex-direction: column; gap: 12px; }
                
                .card-header-elite { display: flex; justify-content: space-between; align-items: center; border-bottom: 1.5px solid #fafafa; padding-bottom: 8px; color: #444; }
                .card-title-elite { font-size: 13px; font-weight: 950; letter-spacing: -0.2px; color: #111; text-transform: uppercase; }
                
                .timeline-horizontal-path { display: flex; align-items: center; justify-content: space-between; padding: 4px 6px; overflow-x: auto; gap: 4px; }
                .timeline-node { display: flex; flex-direction: column; align-items: center; text-align: center; gap: 6px; min-width: 80px; }
                .node-icon-wrapper { width: 28px; height: 28px; border-radius: 50%; display: flex; align-items: center; justify-content: center; transition: all 0.3s ease; }
                
                .timeline-node.completed .node-icon-wrapper { background: var(--color-primary-soft); color: var(--color-primary); border: 2px solid var(--color-primary); }
                .timeline-node.active .node-icon-wrapper { background: #fff; color: var(--color-primary); border: 2px solid var(--color-primary); box-shadow: 0 0 6px var(--color-primary-soft); }
                .timeline-node.planned .node-icon-wrapper { background: #f5f5f5; color: #bbb; border: 1.5px dashed #ddd; }
                
                .node-pulse-point { width: 6px; height: 6px; background: var(--color-primary); border-radius: 50%; animation: pulse-primary-node-glow-final 2.5s infinite; }
                @keyframes pulse-primary-node-glow-final { 
                    0% { box-shadow: 0 0 0 0 var(--color-primary-soft); } 
                    70% { box-shadow: 0 0 0 6px rgba(0, 0, 0, 0); } 
                    100% { box-shadow: 0 0 0 0 rgba(0, 0, 0, 0); } 
                }

                .node-info { display: flex; flex-direction: column; gap: 1px; }
                .node-title { font-size: 10px; font-weight: 850; color: #222; max-width: 90px; text-overflow: ellipsis; overflow: hidden; white-space: nowrap; }
                .node-status { font-size: 9px; color: #999; font-weight: 700; }
                
                .timeline-connector { flex: 1; height: 2px; min-width: 20px; margin-bottom: 22px; }
                .timeline-connector.active { background: var(--color-primary); }
                .timeline-connector.planned { background: #eee; border-top: 2px dashed #ddd; height: 0; }

                /* Pathways & Community Layout */
                .lms-pathways-community-row { display: grid; grid-template-columns: 3.2fr 1.2fr; gap: 24px; margin-bottom: 24px; }
                .lms-pathways-left-col { display: flex; flex-direction: column; }
                .lms-community-right-col { background: #fff; border: 1px solid #f0f0f0; border-radius: 20px; padding: 16px 20px; box-shadow: 0 5px 15px rgba(0,0,0,0.01); display: flex; flex-direction: column; gap: 12px; align-self: flex-start; min-width: 250px; }

                /* Section Branding */
                .grid-section { margin-top: 16px; }
                .section-head-elite { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 16px; padding: 0 4px; }
                .sh-tag { font-size: 9px; font-weight: 950; color: var(--color-primary); letter-spacing: 1.5px; margin-bottom: 4px; }
                .sh-title { font-size: 20px; font-weight: 900; letter-spacing: -0.5px; margin: 0; }

                /* Curated Cards */
                .curated-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px; }
                .curated-card { cursor: pointer; background: #fff; border: 1px solid #f2f2f2; border-radius: 20px; overflow: hidden; box-shadow: 0 8px 24px rgba(0,0,0,0.01); display: flex; flex-direction: column; }
                .c-banner { height: 130px; position: relative; overflow: hidden; }
                .c-img { width: 100%; height: 100%; object-fit: cover; transition: 0.5s; }
                .curated-card:hover .c-img { transform: scale(1.03); }
                .c-overlay { position: absolute; inset: 0; background: linear-gradient(to bottom, transparent, rgba(0,0,0,0.3)); }
                .c-badge { position: absolute; top: 12px; right: 12px; background: rgba(0,0,0,0.5); backdrop-filter: blur(8px); padding: 4px 10px; border-radius: 6px; font-size: 8px; font-weight: 900; color: #fff; text-transform: uppercase; }
                
                .c-content { padding: 16px; display: flex; flex-direction: column; flex: 1; }
                .c-tag { font-size: 8px; font-weight: 950; color: var(--color-primary); letter-spacing: 1px; opacity: 0.6; }
                .c-title { font-size: 15px; font-weight: 900; color: #222; margin: 6px 0 14px; min-height: 40px; line-height: 1.35; }
                
                .c-footer { display: flex; align-items: center; justify-content: space-between; border-top: 1px solid #f9f9f9; padding-top: 12px; margin-top: auto; }
                .c-prog-wrap { display: flex; align-items: center; gap: 8px; flex: 1; }
                .c-prog-bar { flex: 1; height: 4px; background: #eee; border-radius: 10px; overflow: hidden; }
                .c-prog-bar .fill { height: 100%; background: var(--color-primary); border-radius: 10px; }
                .c-pct { font-size: 11px; font-weight: 900; color: #444; }
                .c-action { width: 28px; height: 28px; border-radius: 8px; background: var(--color-primary-soft); color: var(--color-primary); display: flex; align-items: center; justify-content: center; margin-left: 12px; }

                /* Catalog card style */
                .disabled-card { opacity: 0.88; }
                
                /* Community Connections styles */
                .lms-community-list { display: flex; flex-direction: column; gap: 12px; }
                .lms-community-item { display: flex; align-items: center; gap: 10px; padding: 2px 0; }
                .peer-avatar-wrapper { position: relative; width: 32px; height: 32px; border-radius: 50%; display: flex; align-items: center; justify-content: center; background: #fafafa; border: 1px solid #eee; overflow: visible; }
                .peer-avatar { width: 100%; height: 100%; border-radius: 50%; object-fit: cover; }
                .peer-active-dot { position: absolute; bottom: 0; right: 0; width: 8px; height: 8px; background: #10b981; border: 1.5px solid #fff; border-radius: 50%; }
                
                .peer-info { display: flex; flex-direction: column; gap: 1px; }
                .peer-name { font-size: 12px; font-weight: 900; color: #222; }
                .peer-activity { font-size: 9px; color: #999; font-weight: 650; }
                .empty-community { display: flex; flex-direction: column; align-items: center; padding: 16px 0; text-align: center; }

                .empty-dashboard { padding: 80px; text-align: center; border-radius: 40px; }
                
                .pulse-dot { width: 8px; height: 8px; background: #10b981; border-radius: 50%; animation: pulse-green-glow 2s infinite; }
                @keyframes pulse-green-glow { 0% { box-shadow: 0 0 0 0 rgba(16, 185, 129, 0.4); } 70% { box-shadow: 0 0 0 10px rgba(16, 185, 129, 0); } 100% { box-shadow: 0 0 0 0 rgba(16, 185, 129, 0); } }

                @media (max-width: 1200px) {
                    .header-visual-metrics { display: none; }
                    .lms-journey-timeline-section { grid-template-columns: 1fr; }
                    .lms-pathways-community-row { grid-template-columns: 1fr; }
                    .lms-community-right-col { margin-top: 24px; width: 100%; }
                    .curated-grid { grid-template-columns: repeat(2, 1fr); }
                }

                @media (max-width: 768px) {
                    .lms-elite-dashboard { padding: 20px; }
                    .lms-command-header { padding: 40px 30px; border-radius: 24px; }
                    .elite-greeting { font-size: 32px; }
                    .curated-grid { grid-template-columns: 1fr; }
                }
            `}</style>
        </div>
    );
};
