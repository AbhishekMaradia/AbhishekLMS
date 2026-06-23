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
    const stats = [
        { label: 'Organizations', value: counts.orgs, color: 'var(--color-primary)', gradient: 'var(--accent-gradient)', Icon: StatIcons.Orgs, module: 'ORGANIZATION', tabId: 'orgs' },
        { label: 'Categories', value: counts.cats, color: '#10b981', gradient: 'linear-gradient(135deg, #10b981 0%, #059669 100%)', Icon: StatIcons.Cats, module: 'CATEGORY', tabId: 'cat' },
        { label: 'Active Users', value: Math.max(0, counts.users - 1), color: '#f59e0b', gradient: 'linear-gradient(135deg, #f59e0b 0%, #d97706 100%)', Icon: StatIcons.Users, module: 'USER', tabId: 'users' },
        { label: 'Courses', value: counts.courses, color: '#ec4899', gradient: 'linear-gradient(135deg, #ec4899 0%, #db2777 100%)', Icon: StatIcons.Courses, module: 'COURSE', tabId: 'curr' },
        { label: 'Groups', value: counts.groups, color: '#06b6d4', gradient: 'linear-gradient(135deg, #06b6d4 0%, #0891b2 100%)', Icon: StatIcons.Groups, module: 'GROUP', tabId: 'group' },
        { label: 'Course Material', value: counts.courses || 0, color: '#8b5cf6', gradient: 'linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%)', Icon: StatIcons.Media, module: 'VIDEO', tabId: 'cm' },
        { label: 'Enrollments', value: counts.enrollments || 0, color: '#6366f1', gradient: 'linear-gradient(135deg, #6366f1 0%, #4f46e5 100%)', Icon: Icons.Shield, module: 'SUBSCRIPTION', tabId: 'enroll' },
        { label: 'Analytical Reports', value: counts.reports || 0, color: '#f43f5e', gradient: 'linear-gradient(135deg, #f43f5e 0%, #e11d48 100%)', Icon: Icons.BarChart, module: 'REPORT', tabId: 'reports' },
        { label: 'Access Control', value: counts.roles || 0, color: '#ef4444', gradient: 'linear-gradient(135deg, #ef4444 0%, #dc2626 100%)', Icon: StatIcons.Security, module: 'ROLE', tabId: 'sec' },
    ].filter(s => {
        if (s.module === 'ORGANIZATION') return isSuperAdmin;
        if (s.module === 'ROLE') return isSuperAdmin || hasPermission('ROLE', 'ROLE_VIEW');
        if (s.module === 'SUBSCRIPTION') return isSuperAdmin || hasPermission('SUBSCRIPTION', 'SUBSCRIPTION_VIEW');
        if (s.module === 'REPORT') return isSuperAdmin || hasPermission('REPORT', 'REPORT_VIEW');
        return hasPermission(s.module, s.module + '_VIEW');
    });

    if (ui.loading && counts.orgs === 0) {
        return (
            <div className="lms-stat-grid">
                {[1, 2, 3, 4, 5].slice(0, stats.length).map(i => (
                    <div key={i} className="lms-stat-card skeleton">
                    </div>
                ))}
            </div>
        );
    }

    return (
        <div className="lms-fade-in lms-dashboard-wrapper">
            <div className="lms-stat-grid">
                {stats.map(({ label, value, Icon, tabId }) => {
                    const isActive = tab === tabId;
                    return (
                        <div
                            key={label}
                            className={`lms-stat-card lms-stat-theme-${tabId} ${isActive ? 'active' : ''}`}
                            onClick={() => setTab(tabId)}
                        >
                            <div className="lms-stat-header">
                                <div className="lms-stat-icon-box">
                                    <Icon s={24} />
                                </div>
                                <div className="lms-stat-value">{value}</div>
                            </div>

                            <div className="lms-stat-label">{label}</div>
                        </div>
                    );
                })}

            </div>
        </div>
    );
};
