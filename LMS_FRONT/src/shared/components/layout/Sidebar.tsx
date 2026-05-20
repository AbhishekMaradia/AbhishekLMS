import {
    BarChart3,
    BookOpenText,
    Building2,
    Clapperboard,
    FolderTree,
    LayoutDashboard,
    ShieldCheck,
    UsersRound,
    CheckSquare,
    type LucideIcon
} from 'lucide-react';
import { Icons, SecureImage } from '../lms/LmsComponents';
import { NavLink } from 'react-router-dom';
import './Layout.css';


export const Sidebar = ({ sidebarOpen, activeTab: tab, setTab, hasPermission, isSuperAdmin, user, activeOrg }: any) => {

    // Fallback logic for Dynamic Branding
    const brandName = activeOrg?.orgName || activeOrg?.OrgName || user?.firstName || user?.FirstName;
    const brandLogo = activeOrg?.logoUrl || activeOrg?.LogoUrl;


    const NavItem = ({ id, icon: Icon, label, disabled }: { id: string; icon: LucideIcon; label: string; disabled?: boolean }) => {

        const tabToRoute: Record<string, string> = {
            'dash': '/dashboard', 'users': '/users', 'orgs': '/organizations',
            'cat': '/categories', 'curr': '/courses', 'cm': '/media',
            'group': '/groups', 'sec': '/security', 'enroll': '/enrollments',
            'reports': '/reports', 'att': '/groups'
        };

        if (disabled) return null;

        return (
            <div
                onClick={() => {
                    setTab(id);
                }}
                className={`lms-nav-item${tab === id ? ' active' : ''}`}
                style={{ cursor: 'pointer' }}
            >
                <div>
                    <Icon size={20} strokeWidth={2.2} />
                </div>
                <span>{label}</span>
            </div>
        );
    };

    const canSeeOrg = isSuperAdmin || hasPermission('ORGANIZATION', 'ORGANIZATION_VIEW');
    const canSeeCat = hasPermission('CATEGORY', 'CATEGORY_VIEW');
    const canSeeUser = hasPermission('USER', 'USER_VIEW');
    const canSeeCourse = hasPermission('COURSE', 'COURSE_VIEW');
    const canSeeMedia = hasPermission('VIDEO', 'VIDEO_VIEW');
    const canSeeGroup = hasPermission('GROUP', 'GROUP_VIEW');
    const canSeeSecurity = (hasPermission('ROLE', 'ROLE_VIEW') || hasPermission('PERMISSION', 'PERMISSION_VIEW') || hasPermission('MODULE', 'MODULE_VIEW') || hasPermission('ROLE_MODULE', 'ROLE_MODULE_PERMISSION_VIEW') || hasPermission('USER_ROLE', 'USER_ROLE_VIEW'));
    const canSeeEnrollment = isSuperAdmin || hasPermission('SUBSCRIPTION', 'SUBSCRIPTION_VIEW');
    const canSeeReport = isSuperAdmin || hasPermission('REPORT', 'REPORT_VIEW');
    const canSeeAttendance = isSuperAdmin || hasPermission('ATTENDANCE', 'ATTENDANCE_VIEW');

    return (
        <aside className={`lms-sidebar-modern ${sidebarOpen ? 'open' : ''}`}>
            <div className="lms-sidebar-brand-wrapper">
                <div className={`lms-sidebar-brand-card ${brandLogo ? 'has-logo' : ''}`}>
                    <div className="lms-sidebar-logo-box">
                        {brandLogo ? (
                            <SecureImage src={brandLogo} className="lms-sidebar-brand-img" />
                        ) : (
                            <Icons.Check s={22} strokeWidth={3} />
                        )}
                    </div>

                    <div className="lms-sidebar-brand-text">
                        <div className="lms-sidebar-brand-name">{brandName}</div>
                        <div className="lms-sidebar-brand-sub">Workspace</div>
                    </div>
                </div>
            </div>


            <nav className="lms-sidebar-nav lms-custom-scrollbar">
                <div className="lms-sidebar-section-label first">WORKSPACE</div>
                <NavItem id="dash" icon={LayoutDashboard} label="Dashboard" />
                <NavItem id="orgs" icon={Building2} label={isSuperAdmin ? "Organizations" : "My Organization"} disabled={!canSeeOrg} />
                <NavItem id="cat" icon={FolderTree} label="Categories" disabled={!canSeeCat} />
                <NavItem id="users" icon={UsersRound} label="Users" disabled={!canSeeUser} />
                <NavItem id="curr" icon={BookOpenText} label="Courses" disabled={!canSeeCourse} />
                <NavItem id="cm" icon={Clapperboard} label="Media Center" disabled={!canSeeMedia} />
                <NavItem id="group" icon={UsersRound} label="Groups" disabled={!canSeeGroup} />
                <NavItem id="enroll" icon={ShieldCheck} label="Course Enrollments" disabled={!canSeeEnrollment} />
                <NavItem id="reports" icon={BarChart3} label="Analytical Reports" disabled={!canSeeReport} />
                <NavItem id="att" icon={CheckSquare} label="Group Attendance" disabled={!canSeeAttendance} />


                <div className="lms-sidebar-section-label">CORE</div>
                <NavItem id="sec" icon={ShieldCheck} label="Access Control" disabled={!canSeeSecurity} />
            </nav>

            <div className="lms-sidebar-footer">
                <div className="lms-flex-row lms-sidebar-footer-row">
                    <div className="lms-sidebar-user-avatar">
                        {user?.firstName?.charAt(0)}
                    </div>
                    <div className="lms-sidebar-user-info">
                        <div className="lms-sidebar-user-name">{user?.firstName}</div>
                        <div className="lms-sidebar-user-role">{isSuperAdmin ? 'Super Admin' : 'Admin'}</div>
                    </div>
                </div>
            </div>
        </aside>
    );
};

