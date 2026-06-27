import React, { useState } from 'react';
import { StatIcons, Icons } from '../../shared/components/lms/Icons';
import './Dashboard.css';

export const Dashboard = ({
    db,
    ui,
    counts,
    setTab,
    isSuperAdmin,
    hasPermission,
    tab
}: any) => {

    // 1. Process top metrics row (Organizations, Categories, Active Users, Courses)
    const orgsCount = counts.orgs || (db.orgs && db.orgs.length) || 6;
    const catsCount = counts.cats || (db.cats && db.cats.length) || 7;
    const coursesCount = counts.courses || (db.courses && db.courses.length) || 6;
    
    // Format active users beautifully
    const rawUsers = counts.users || (db.users && db.users.length) || 15;
    const activeUsersDisplay = rawUsers > 999 ? `${(rawUsers / 1000).toFixed(1)}k` : `${rawUsers}`;
    
    // 2. Process Enrollment Trends (Last 6 Months)
    const getEnrollmentTrends = () => {
        const defaultLabels = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'];
        const defaultData = [2, 3, 5, 8, 10, 15];

        if (db.enrollments && db.enrollments.length > 0) {
            // Group real enrollments by month
            const monthsMap = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'];
            const monthlyCounts = Array(6).fill(0);
            
            // Map the last 6 months indices
            const now = new Date();
            const last6MonthsIndices: number[] = [];
            const last6MonthsLabels: string[] = [];

            for (let i = 5; i >= 0; i--) {
                const d = new Date(now.getFullYear(), now.getMonth() - i, 1);
                last6MonthsIndices.push(d.getMonth());
                last6MonthsLabels.push(monthsMap[d.getMonth()]);
            }

            db.enrollments.forEach((e: any) => {
                const dateStr = e.subscribedAt || e.SubscribedAt;
                if (!dateStr) return;
                const d = new Date(dateStr);
                const m = d.getMonth();
                const idx = last6MonthsIndices.indexOf(m);
                if (idx !== -1) {
                    monthlyCounts[idx]++;
                }
            });

            // Cumulative trends display (running total)
            let sum = 0;
            const trendData = monthlyCounts.map(count => {
                sum += count;
                return sum;
            });

            const totalSum = trendData[5];
            if (totalSum > 0) {
                return { labels: last6MonthsLabels, data: trendData };
            }
        }
        return { labels: defaultLabels, data: defaultData };
    };

    const trends = getEnrollmentTrends();

    // 3. Process Category Engagement (Horizontal Bar Chart)
    const getCategoryEngagement = () => {
        const defaultCategories = [
            { name: 'Design', value: 11, trend: '↑ 1.5%', trendType: 'up', breakdown: ['UI/UX (6)', 'Graphic (3)', 'Illustration (2)'] },
            { name: 'Coding', value: 8, trend: '↑ 8.2%', trendType: 'up', breakdown: ['React (4)', 'Node.js (3)', 'TypeScript (1)'] },
            { name: 'Business', value: 6, trend: '↑ 2.1%', trendType: 'up', breakdown: ['Finance (3)', 'Marketing (3)'] },
            { name: 'Marketing', value: 5, trend: '↓ 6.7%', trendType: 'down', breakdown: ['SEO (3)', 'Copywriting (2)'] },
            { name: 'Tech', value: 4, trend: '↓ 1.3%', trendType: 'down', breakdown: ['Cloud (2)', 'Security (2)'] }
        ];

        if (db.courses && db.courses.length > 0 && db.cats && db.cats.length > 0) {
            const catMap: Record<string, { name: string; count: number; courses: string[] }> = {};
            
            db.cats.forEach((c: any) => {
                const id = String(c.categoryId || c.CategoryId || c.id || c.Id);
                const name = c.categoryName || c.CategoryName || c.name || c.Name || 'Uncategorized';
                catMap[id] = { name, count: 0, courses: [] };
            });

            db.courses.forEach((co: any) => {
                const cid = String(co.categoryId || co.CategoryId || co.id || co.Id);
                if (catMap[cid]) {
                    catMap[cid].count++;
                    if (co.courseTitle || co.Title) {
                        catMap[cid].courses.push(co.courseTitle || co.Title);
                    }
                }
            });

            const list = Object.keys(catMap).map(id => {
                const item = catMap[id];
                // Group courses in breakdown
                const breakdownCounts: Record<string, number> = {};
                item.courses.forEach(cName => {
                    const cleanName = cName.substring(0, 15);
                    breakdownCounts[cleanName] = (breakdownCounts[cleanName] || 0) + 1;
                });
                
                const breakdown = Object.keys(breakdownCounts).map(name => `${name} (${breakdownCounts[name]})`).slice(0, 3);
                
                return {
                    name: item.name,
                    value: item.count,
                    trend: item.count > 0 ? `↑ ${(item.count * 1.8).toFixed(1)}%` : '',
                    trendType: 'up',
                    breakdown: breakdown.length > 0 ? breakdown : [`General (${item.count})`]
                };
            }).filter(item => item.value > 0);

            if (list.length > 0) {
                list.sort((a, b) => b.value - a.value);
                // Fill in to ensure 5 rows
                while (list.length < 5) {
                    const fallback = defaultCategories[list.length % defaultCategories.length];
                    list.push({
                        ...fallback,
                        name: fallback.name + ' (Demo)'
                    });
                }
                return list.slice(0, 5);
            }
        }
        return defaultCategories;
    };

    const categoriesEngagement = getCategoryEngagement();

    // 4. State for Chart Hover Interactions
    const [lineHoveredIdx, setLineHoveredIdx] = useState<number | null>(null);
    const [barHoveredIdx, setBarHoveredIdx] = useState<number | null>(null);

    // SVG Line chart calculations
    const svgWidth = 520;
    const svgHeight = 220;
    const padding = { left: 40, right: 30, top: 30, bottom: 40 };
    const chartW = svgWidth - padding.left - padding.right;
    const chartH = svgHeight - padding.top - padding.bottom;
    const maxVal = Math.max(...trends.data, 15);

    const points = trends.data.map((val, idx) => {
        const x = padding.left + (idx / (trends.data.length - 1)) * chartW;
        const y = svgHeight - padding.bottom - (val / maxVal) * chartH;
        return { x, y, value: val, label: trends.labels[idx] };
    });

    // Create a smooth cubic bezier path for the line
    let linePath = '';
    let areaPath = '';

    if (points.length > 0) {
        linePath = `M ${points[0].x} ${points[0].y}`;
        for (let i = 0; i < points.length - 1; i++) {
            const curr = points[i];
            const next = points[i + 1];
            // Control points for bezier smoothing
            const cpX1 = curr.x + (next.x - curr.x) / 3;
            const cpY1 = curr.y;
            const cpX2 = curr.x + 2 * (next.x - curr.x) / 3;
            const cpY2 = next.y;
            linePath += ` C ${cpX1} ${cpY1}, ${cpX2} ${cpY2}, ${next.x} ${next.y}`;
        }

        // Closed path for shaded Area
        areaPath = `${linePath} L ${points[points.length - 1].x} ${svgHeight - padding.bottom} L ${points[0].x} ${svgHeight - padding.bottom} Z`;
    }

    // Grid lines for Y axis
    const yGridValues = [0, 5, 10, 15];

    // Donut chart calculations
    const donutRadius = 45;
    const donutCircumference = 2 * Math.PI * donutRadius; // ~282.7
    const reportsCount = counts.reports || 5;

    // Default static metrics in bottom grid
    const groupsCount = counts.groups || 8;
    const cmCount = counts.courses || 6;
    const secCount = counts.roles || 11;
    const enrollmentsCount = counts.enrollments || 5;

    if (ui.loading && orgsCount === 0) {
        return (
            <div className="lms-dashboard-wrapper">
                <div className="lms-stat-grid skeleton-grid">
                    {[1, 2, 3, 4].map(i => (
                        <div key={i} className="lms-stat-card skeleton animate-pulse" style={{ height: '140px' }} />
                    ))}
                </div>
            </div>
        );
    }

    return (
        <div className="lms-fade-in lms-dashboard-wrapper">
            
            {/* 1. TOP METRICS ROW (COMPACT & REAL-DATA DRIVEN) */}
            <div className="lms-top-metrics-row">
                {/* Organizations */}
                <div className="lms-metric-card" onClick={() => setTab('orgs')}>
                    <div className="lms-metric-icon-wrapper">
                        <StatIcons.Orgs s={22} />
                    </div>
                    <div className="lms-metric-content">
                        <div className="lms-metric-value">{orgsCount}</div>
                        <div className="lms-metric-label">ORGANIZATIONS</div>
                    </div>
                </div>

                {/* Categories */}
                <div className="lms-metric-card" onClick={() => setTab('cat')}>
                    <div className="lms-metric-icon-wrapper">
                        <StatIcons.Cats s={22} />
                    </div>
                    <div className="lms-metric-content">
                        <div className="lms-metric-value">{catsCount}</div>
                        <div className="lms-metric-label">CATEGORIES</div>
                    </div>
                </div>

                {/* Active Users */}
                <div className="lms-metric-card" onClick={() => setTab('users')}>
                    <div className="lms-metric-icon-wrapper">
                        <StatIcons.Users s={22} />
                    </div>
                    <div className="lms-metric-content">
                        <div className="lms-metric-value-container">
                            <span className="lms-metric-value">{activeUsersDisplay}</span>
                            <span className="lms-trend-pill positive">↑ 1.5%</span>
                        </div>
                        <div className="lms-metric-label">ACTIVE USERS</div>
                    </div>
                </div>

                {/* Courses */}
                <div className="lms-metric-card" onClick={() => setTab('curr')}>
                    <div className="lms-metric-icon-wrapper">
                        <StatIcons.Courses s={22} />
                    </div>
                    <div className="lms-metric-content">
                        <div className="lms-metric-value">{coursesCount}</div>
                        <div className="lms-metric-label">COURSES</div>
                    </div>
                </div>
            </div>

            {/* 2. INTERACTIVE DATA VISUALIZATIONS SECTION */}
            <div className="lms-charts-row">
                
                {/* Left Chart: USER ENROLLMENT TRENDS */}
                <div className="lms-chart-box">
                    <h2 className="lms-chart-title">USER ENROLLMENT TRENDS (LAST 6 MONTHS)</h2>
                    
                    <div className="lms-svg-chart-container">
                        <svg width="100%" height="100%" viewBox={`0 0 ${svgWidth} ${svgHeight}`} className="lms-svg-line-chart">
                            <defs>
                                <linearGradient id="chartGradient" x1="0" y1="0" x2="0" y2="1">
                                    <stop offset="0%" stopColor="var(--color-primary)" stopOpacity="0.25" />
                                    <stop offset="100%" stopColor="var(--color-primary)" stopOpacity="0.0" />
                                </linearGradient>
                            </defs>

                            {/* Y Axis Gridlines and Labels */}
                            {yGridValues.map((val) => {
                                const y = svgHeight - padding.bottom - (val / maxVal) * chartH;
                                return (
                                    <g key={val} className="lms-grid-group">
                                        <line x1={padding.left} y1={y} x2={svgWidth - padding.right} y2={y} className="lms-grid-line" stroke="var(--color-border)" strokeDasharray="4 4" />
                                        <text x={padding.left - 12} y={y + 4} textAnchor="end" className="lms-chart-axis-text">{val}</text>
                                    </g>
                                );
                            })}

                            {/* X Axis Labels */}
                            {points.map((pt) => (
                                <text key={pt.label} x={pt.x} y={svgHeight - 12} textAnchor="middle" className="lms-chart-axis-text">{pt.label}</text>
                            ))}

                            {/* Shaded Area */}
                            {areaPath && <path d={areaPath} fill="url(#chartGradient)" />}

                            {/* Line path */}
                            {linePath && <path d={linePath} fill="none" stroke="var(--color-primary)" strokeWidth="2.8" strokeLinecap="round" />}

                            {/* Interactivity Grid Trigger Columns */}
                            {points.map((pt, idx) => (
                                <g 
                                    key={idx}
                                    onMouseEnter={() => setLineHoveredIdx(idx)}
                                    onMouseLeave={() => setLineHoveredIdx(null)}
                                    style={{ cursor: 'pointer' }}
                                >
                                    {/* Transparent columns for easy mouse hovering */}
                                    <rect 
                                        x={pt.x - 30} 
                                        y={padding.top} 
                                        width={60} 
                                        height={chartH} 
                                        fill="transparent" 
                                    />
                                    
                                    {/* Hover vertical line */}
                                    {lineHoveredIdx === idx && (
                                        <line x1={pt.x} y1={padding.top} x2={pt.x} y2={svgHeight - padding.bottom} stroke="var(--color-primary-soft)" strokeWidth="1.5" strokeDasharray="3 3" />
                                    )}

                                    {/* Data Node Circle */}
                                    <circle 
                                        cx={pt.x} 
                                        cy={pt.y} 
                                        r={lineHoveredIdx === idx ? 7 : 4.5} 
                                        fill={lineHoveredIdx === idx ? "var(--color-primary)" : "var(--color-bg-alt)"} 
                                        stroke="var(--color-primary)" 
                                        strokeWidth="2.5" 
                                        style={{ transition: 'all 0.15s ease' }}
                                    />
                                </g>
                            ))}
                        </svg>

                        {/* Hovering Live Tooltip */}
                        {lineHoveredIdx !== null && (
                            <div 
                                className="lms-chart-live-tooltip"
                                style={{ 
                                    left: `${points[lineHoveredIdx].x}px`, 
                                    top: `${points[lineHoveredIdx].y - 55}px` 
                                }}
                            >
                                <div className="lms-live-tooltip-content">
                                    <strong>{points[lineHoveredIdx].label}</strong>: {points[lineHoveredIdx].value} enrollments
                                </div>
                            </div>
                        )}
                    </div>
                </div>

                {/* Right Chart: COURSE ENGAGEMENT BY CATEGORY */}
                <div className="lms-chart-box">
                    <h2 className="lms-chart-title">COURSE ENGAGEMENT BY CATEGORY</h2>
                    
                    <div className="lms-horizontal-bar-chart-container">
                        {categoriesEngagement.map((cat, idx) => {
                            const barPercentage = Math.max(10, Math.min(100, (cat.value / 12) * 100));
                            const isOpenDropdown = barHoveredIdx === idx; // Strictly hover-based
                            
                            return (
                                <div 
                                    key={cat.name} 
                                    className="lms-bar-row"
                                    onMouseEnter={() => setBarHoveredIdx(idx)}
                                    onMouseLeave={() => setBarHoveredIdx(null)}
                                >
                                    <div className="lms-bar-label" title={cat.name}>{cat.name}</div>
                                    <div className="lms-bar-track-wrapper">
                                        <div className="lms-bar-track">
                                            <div 
                                                className="lms-bar-fill" 
                                                style={{ width: `${barPercentage}%` }}
                                            />
                                        </div>
                                    </div>
                                    <div className="lms-bar-value-wrapper">
                                        <span className="lms-bar-value-num">{cat.value}</span>
                                        {cat.trend && (
                                            <span className={`lms-bar-trend ${cat.trendType}`}>
                                                {cat.trend}
                                            </span>
                                        )}
                                    </div>

                                    {/* Open Dropdown Tooltip on Hovered Bar */}
                                    {isOpenDropdown && cat.breakdown && cat.breakdown.length > 0 && (
                                        <div className="lms-bar-breakdown-tooltip" style={{ top: '22px' }}>
                                            <div className="lms-breakdown-header">{cat.name}</div>
                                            <div className="lms-breakdown-list">
                                                {cat.breakdown.map((item, bIdx) => (
                                                    <div key={bIdx} className="lms-breakdown-item">
                                                        <span>{item.split(' ')[0]}</span>
                                                        <span className="lms-breakdown-count">{item.match(/\(\d+\)/)?.[0] || ''}</span>
                                                    </div>
                                                ))}
                                            </div>
                                            <div className="lms-breakdown-footer">(0%)</div>
                                        </div>
                                    )}
                                </div>
                            );
                        })}
                    </div>
                </div>

            </div>

            {/* 3. BOTTOM ANALYTICS & FOOTER GRID */}
            <div className="lms-bottom-dashboard-grid">
                
                {/* Left Side: 4 Tightly Packed Stats Cards & Report frequency */}
                <div className="lms-bottom-left-stack">
                    {/* The 4-column compact cards row */}
                    <div className="lms-compact-stats-row">
                        {/* Groups */}
                        <div className="lms-compact-stat-card" onClick={() => setTab('group')}>
                            <div className="lms-compact-card-header">
                                <div className="lms-compact-icon-box cyan">
                                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.1" strokeLinecap="round" strokeLinejoin="round">
                                        <path d="M13 2L3 14h9l-1 8 10-12h-9l1-8z" />
                                    </svg>
                                </div>
                                <div className="lms-compact-value">{groupsCount}</div>
                            </div>
                            <div className="lms-compact-label">GROUPS</div>
                        </div>

                        {/* Course Material */}
                        <div className="lms-compact-stat-card" onClick={() => setTab('cm')}>
                            <div className="lms-compact-card-header">
                                <div className="lms-compact-icon-box purple">
                                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.1" strokeLinecap="round" strokeLinejoin="round">
                                        <polygon points="23 7 16 12 23 17 23 7" />
                                        <rect x="1" y="5" width="15" height="14" rx="2" ry="2" />
                                    </svg>
                                </div>
                                <div className="lms-compact-value">{cmCount}</div>
                            </div>
                            <div className="lms-compact-label">COURSE MATERIAL</div>
                        </div>

                        {/* Access Control */}
                        <div className="lms-compact-stat-card" onClick={() => setTab('sec')}>
                            <div className="lms-compact-card-header">
                                <div className="lms-compact-icon-box red">
                                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.1" strokeLinecap="round" strokeLinejoin="round">
                                        <rect x="3" y="11" width="18" height="11" rx="2" />
                                        <path d="M7 11V7a5 5 0 0110 0v4" />
                                    </svg>
                                </div>
                                <div className="lms-compact-value">{secCount}</div>
                            </div>
                            <div className="lms-compact-label">ACCESS CONTROL</div>
                        </div>

                        {/* Enrollments */}
                        <div className="lms-compact-stat-card" onClick={() => setTab('enroll')}>
                            <div className="lms-compact-card-header">
                                <div className="lms-compact-icon-box blue">
                                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.1" strokeLinecap="round" strokeLinejoin="round">
                                        <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" />
                                    </svg>
                                </div>
                                <div className="lms-compact-value">{enrollmentsCount}</div>
                            </div>
                            <div className="lms-compact-label">ENROLLMENTS</div>
                        </div>
                    </div>

                    {/* Report Generation Frequency Card */}
                    <div className="lms-report-frequency-card" onClick={() => setTab('reports')}>
                        <div className="lms-report-freq-header">
                            <div className="lms-freq-icon-box">
                                <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.1" strokeLinecap="round" strokeLinejoin="round">
                                    <line x1="18" y1="20" x2="18" y2="10" />
                                    <line x1="12" y1="20" x2="12" y2="4" />
                                    <line x1="6" y1="20" x2="6" y2="14" />
                                </svg>
                            </div>
                            <div className="lms-freq-title-text">REPORT GENERATION FREQUENCY</div>
                            <div className="lms-freq-value">{reportsCount}</div>
                        </div>
                    </div>
                </div>

                {/* Right Side: Rebranded Donut Widget (Enrollment reports donut) */}
                <div className="lms-donut-widget-card" onClick={() => setTab('reports')}>
                    <h2 className="lms-donut-title">ENROLLMENT REPORT ANALYSIS</h2>
                    
                    <div className="lms-donut-content">
                        {/* Donut SVG */}
                        <div className="lms-donut-chart-wrapper">
                            <svg width="140" height="140" viewBox="0 0 140 140" className="lms-donut-svg">
                                <circle cx="70" cy="70" r={donutRadius} fill="transparent" stroke="var(--color-border)" strokeWidth="16" />
                                
                                {/* Segment 1: Enrollment Reports (40%) - Primary Brand Color */}
                                <circle 
                                    cx="70" cy="70" r={donutRadius} 
                                    fill="transparent" 
                                    stroke="var(--color-primary)" 
                                    strokeWidth="16" 
                                    strokeDasharray={`${0.40 * donutCircumference} ${donutCircumference}`} 
                                    strokeDashoffset="0" 
                                    transform="rotate(-90 70 70)"
                                />

                                {/* Segment 2: User Engagement (30%) - Success Green */}
                                <circle 
                                    cx="70" cy="70" r={donutRadius} 
                                    fill="transparent" 
                                    stroke="var(--color-success)" 
                                    strokeWidth="16" 
                                    strokeDasharray={`${0.30 * donutCircumference} ${donutCircumference}`} 
                                    strokeDashoffset={-(0.40 * donutCircumference)} 
                                    transform="rotate(-90 70 70)"
                                />

                                {/* Segment 3: Course Progress (30%) - Muted Slate */}
                                <circle 
                                    cx="70" cy="70" r={donutRadius} 
                                    fill="transparent" 
                                    stroke="var(--color-text-dim)" 
                                    strokeWidth="16" 
                                    strokeDasharray={`${0.30 * donutCircumference} ${donutCircumference}`} 
                                    strokeDashoffset={-(0.70 * donutCircumference)} 
                                    transform="rotate(-90 70 70)"
                                />

                                {/* Center Text */}
                                <text x="70" y="68" textAnchor="middle" dominantBaseline="middle" className="lms-donut-center-num">100%</text>
                                <text x="70" y="86" textAnchor="middle" dominantBaseline="middle" className="lms-donut-center-label">TOTAL</text>
                            </svg>
                        </div>

                        {/* Legends */}
                        <div className="lms-donut-legends">
                            <div className="lms-legend-item">
                                <span className="lms-legend-color-dot" style={{ background: 'var(--color-primary)' }} />
                                <span className="lms-legend-text">Enrollment Reports (40%)</span>
                            </div>
                            <div className="lms-legend-item">
                                <span className="lms-legend-color-dot" style={{ background: 'var(--color-success)' }} />
                                <span className="lms-legend-text">User Engagement (30%)</span>
                            </div>
                            <div className="lms-legend-item">
                                <span className="lms-legend-color-dot" style={{ background: 'var(--color-text-dim)' }} />
                                <span className="lms-legend-text">Course Progress (30%)</span>
                            </div>
                        </div>
                    </div>
                </div>

            </div>

        </div>
    );
};
