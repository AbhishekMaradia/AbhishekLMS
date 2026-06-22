import React, { useState } from 'react';
import { Icons } from '../lms/Icons';
import { useTheme } from '../../../app/providers/ThemeProvider';
import './Layout.css';

export const Header = ({
    tab,
    setUi,
    user,
    sidebarOpen,
    setSidebarOpen,
    activeOrg,
    setActiveOrg,
    activeCategory,
    setActiveCategory
}: any) => {

    const { toggleTheme } = useTheme();
    const [userDropdownOpen, setUserDropdownOpen] = useState(false);

    const getTabMeta = () => {
        if (tab === 'dash') return { title: 'Dashboard', Icon: Icons.Dash };
        if (tab === 'profile') return { title: 'My Profile', Icon: Icons.User };
        if (tab === 'orgs') return { title: 'Organizations', Icon: Icons.Org };
        if (tab === 'cat') return { title: 'Categories', Icon: Icons.Cat };
        if (tab === 'users') return { title: 'User Directory', Icon: Icons.Users };
        if (tab === 'curr') return { title: 'Curriculum', Icon: Icons.Book };
        if (tab === 'group') return { title: 'Groups', Icon: Icons.Groups };
        if (tab === 'cm' || tab === 'media') return { title: 'Course Material', Icon: Icons.Video };
        if (tab === 'enroll') return { title: 'Course Enrollments', Icon: Icons.Shield };
        if (tab === 'reports') return { title: 'Analytical Reports', Icon: Icons.BarChart };
        if (['sec', 'mods', 'perms', 'mod_perms', 'role_modules', 'role_mod_perms', 'user_roles'].includes(tab)) {
            return { title: 'Security', Icon: Icons.Shield };
        }
        if (tab === 'student-dash') return { title: 'My Journey', Icon: Icons.Dash };
        if (tab === 'student-discover') return { title: 'Explore Courses', Icon: Icons.Book };
        if (tab === 'student-my-courses') return { title: 'My Library', Icon: Icons.Video };
        if (tab === 'student-peers') return { title: 'Peer Community', Icon: Icons.Users };
        return { title: 'Dashboard', Icon: Icons.Dash };
    };

    const { title, Icon } = getTabMeta();

    return (
        <header className="lms-main-header">
            <div className="lms-flex-row">
                <button className="hamburger" onClick={() => setSidebarOpen((s: any) => !s)}>
                    {sidebarOpen ? <Icons.Close s={24} /> : <Icons.Menu s={24} />}
                </button>

                <div className="lms-header-title-container">
                    <div className="lms-header-icon-box lms-hide-mobile">
                        <Icon s={20} />
                    </div>
                    <div>
                        <h1 className="lms-header-title-text">{title}</h1>
                        <div className="lms-flex-row" style={{ gap: '8px', alignItems: 'center' }}>
                            <div className="lms-header-unit-label lms-hide-mobile">
                                {(activeCategory?.categoryName || activeOrg?.orgName || activeOrg?.OrgName || 'SYSTEM').toUpperCase()}
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div className="lms-flex-row lms-header-actions">
                <button className="lms-icon-btn" onClick={toggleTheme} title="Toggle Appearance">
                    <Icons.Moon s={20} />
                </button>

                <div
                    className="lms-user-pill"
                    onClick={() => setUserDropdownOpen(!userDropdownOpen)}
                >
                    <div className="lms-hide-mobile lms-user-info">
                        <div className="lms-user-name">{user.firstName}</div>
                        <div className="lms-user-role">{user.roleName || 'Member'}</div>
                    </div>
                    <div className="lms-user-avatar">
                        {user.firstName?.[0]}
                    </div>
                    <div className="lms-header-dropdown-icon">
                        <Icons.ChevronDown s={14} />
                    </div>

                    {userDropdownOpen && (
                        <div className="lms-dropdown-menu lms-fade-in">
                            <button className="lms-btn lms-btn-ghost lms-header-dropdown-btn" onClick={(e) => { e.stopPropagation(); if (setUi) setUi({ modal: 'user_update', target: user }); setUserDropdownOpen(false); }}>
                                <Icons.User s={16} /> <span className="lms-header-dropdown-text">My Account</span>
                            </button>
                            {activeOrg && (
                                <button className="lms-btn lms-btn-ghost lms-header-dropdown-btn" onClick={(e) => { e.stopPropagation(); if (setUi) setUi({ modal: 'org_edit', target: activeOrg }); setUserDropdownOpen(false); }}>
                                    <Icons.Org s={16} /> <span className="lms-header-dropdown-text">My Organization</span>
                                </button>
                            )}
                            <div className="lms-dropdown-divider" />
                            <button
                                className="lms-btn lms-btn-ghost lms-header-dropdown-btn lms-header-dropdown-danger"
                                onClick={(e) => { e.stopPropagation(); localStorage.clear(); window.location.reload(); }}
                            >
                                <Icons.Logout s={16} /> <span className="lms-header-dropdown-text">Logout</span>
                            </button>
                        </div>
                    )}
                </div>
            </div>
        </header>
    );
};

