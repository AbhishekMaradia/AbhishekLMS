import React from 'react';
import {
    LayoutDashboard,
    Compass,
    Library,
    Users2,
    Settings,
    GraduationCap,
    FileText,
    type LucideIcon
} from 'lucide-react';
import { NavLink } from 'react-router-dom';
import { Icons, SecureImage } from '../lms/LmsComponents';
import './Layout.css';

export const StudentSidebar = ({ sidebarOpen, activeTab: tab, setTab, user, activeOrg, hasPermission }: any) => {

    const brandName = activeOrg?.orgName || activeOrg?.OrgName || 'SoulCode';
    const brandLogo = activeOrg?.logoUrl || activeOrg?.LogoUrl;

    const NavItem = ({ id, icon: Icon, label, disabled }: { id: string; icon: LucideIcon; label: string; disabled?: boolean }) => {
        const tabToRoute: Record<string, string> = {
            'student-dash': '/student/dashboard',
            'student-discover': '/student/discover',
            'student-my-courses': '/student/my-courses',
            'student-peers': '/student/peers',
            'student-reports': '/student/reports',
        };

        if (disabled) return null;

        return (
            <NavLink
                to={tabToRoute[id] || '/student/dashboard'}
                onClick={() => setTab(id)}
                className={({ isActive }) => `lms-nav-item student-nav-item${isActive ? ' active' : ''}`}
            >
                <div>
                    <Icon size={20} strokeWidth={2.2} />
                </div>
                <span>{label}</span>
            </NavLink>
        );
    };

    const hasLogo = !!brandLogo;
    
    return (
        <aside className="lms-sidebar-modern lms-student-sidebar">
            <div className="lms-sidebar-brand-wrapper">
                <div className={`lms-sidebar-brand-card student-brand-card ${hasLogo ? 'has-logo' : ''}`}>
                    <div className={`lms-sidebar-logo-box student-logo-box ${hasLogo ? 'premium' : ''}`}>
                        {brandLogo ? (
                            <SecureImage src={brandLogo} className="lms-sidebar-brand-img" />
                        ) : (
                            <GraduationCap size={24} className="lms-student-brand-icon" />
                        )}
                    </div>
                    
                    <div className="lms-sidebar-brand-text">
                        <div className="lms-sidebar-brand-name">{brandName}</div>
                        <div className="lms-sidebar-brand-sub">Student Portal</div>
                    </div>
                </div>
            </div>

            <nav className="lms-sidebar-nav lms-custom-scrollbar">
                <div className="lms-sidebar-section-label first">MY JOURNEY</div>
                <NavItem id="student-dash" icon={LayoutDashboard} label="Home" />
                <NavItem id="student-discover" icon={Compass} label="Discover" />
                <NavItem id="student-my-courses" icon={Library} label="My Courses" />
                <NavItem id="student-peers" icon={Users2} label="Peers" />
                <NavItem id="student-reports" icon={FileText} label="Reports" disabled={hasPermission ? !(hasPermission('REPORT', 'REPORT_VIEW') || hasPermission('REPORT', 'REPORT_GENERATE')) : false} />
            </nav>

            <div className="lms-sidebar-footer">
                <div className="lms-flex-row lms-sidebar-footer-row">
                    <div className="lms-sidebar-user-avatar student-avatar">
                        {user?.firstName?.charAt(0)}
                    </div>
                    <div className="lms-sidebar-user-info">
                        <div className="lms-sidebar-user-name">{user?.firstName}</div>
                        <div className="lms-sidebar-user-role">Student</div>
                    </div>
                </div>
            </div>
        </aside>
    );
};
