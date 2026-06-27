import React, { useState, useEffect } from 'react';
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
    setActiveCategory,
    fetchData,
    lastSyncedAt
}: any) => {

    const { toggleTheme } = useTheme();
    const [userDropdownOpen, setUserDropdownOpen] = useState(false);
    const [isOnline, setIsOnline] = useState(navigator.onLine);
    const [isSyncing, setIsSyncing] = useState(false);

    useEffect(() => {
        const handleOnline = () => setIsOnline(true);
        const handleOffline = () => setIsOnline(false);
        window.addEventListener('online', handleOnline);
        window.addEventListener('offline', handleOffline);
        return () => {
            window.removeEventListener('online', handleOnline);
            window.removeEventListener('offline', handleOffline);
        };
    }, []);

    const handleSync = async () => {
        if (isSyncing) return;
        setIsSyncing(true);
        if (fetchData) {
            try {
                await fetchData();
            } catch (e) {}
        }
        setTimeout(() => setIsSyncing(false), 800);
    };

    const getSyncTimeStr = () => {
        if (!lastSyncedAt) return 'Sync: Just now';
        const d = new Date(lastSyncedAt);
        return `Sync: ${d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' })}`;
    };

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
                        <h1 className="lms-header-title-text">{tab === 'dash' ? 'System Dashboard' : title}</h1>
                        <div className="lms-flex-row" style={{ gap: '8px', alignItems: 'center' }}>
                            <div className="lms-header-unit-label lms-hide-mobile">
                                {(activeCategory?.categoryName || activeOrg?.orgName || activeOrg?.OrgName || 'SYSTEM').toUpperCase()}
                            </div>
                        </div>
                    </div>
                </div>

                {tab === 'dash' && (
                    <div className="lms-header-status-container lms-hide-mobile" style={{ display: 'flex', alignItems: 'center', gap: '12px', marginLeft: '24px' }}>
                        <div 
                            className="lms-header-status-pill" 
                            style={{ 
                                display: 'flex', 
                                alignItems: 'center', 
                                gap: '6px', 
                                background: isOnline ? 'var(--color-success-bg)' : 'var(--color-danger-bg)', 
                                color: isOnline ? 'var(--color-success)' : 'var(--color-danger)', 
                                padding: '6px 14px', 
                                borderRadius: '100px', 
                                fontSize: '11px', 
                                fontWeight: 800,
                                transition: 'all 0.3s ease'
                            }}
                        >
                            <span 
                                className="lms-status-dot-green" 
                                style={{ 
                                    width: '8px', 
                                    height: '8px', 
                                    borderRadius: '50%', 
                                    background: isOnline ? '#10b981' : '#ef4444', 
                                    display: 'inline-block', 
                                    boxShadow: isOnline ? '0 0 8px #10b981' : '0 0 8px #ef4444'
                                }}
                            />
                            {isOnline ? 'Connected' : 'Offline'}
                        </div>
                        <div 
                            className="lms-header-status-pill" 
                            style={{ 
                                display: 'flex', 
                                alignItems: 'center', 
                                gap: '8px', 
                                background: 'var(--color-border)', 
                                color: 'var(--color-text)', 
                                padding: '6px 14px', 
                                borderRadius: '100px', 
                                fontSize: '11px', 
                                fontWeight: 600 
                            }}
                        >
                            <span>{getSyncTimeStr()}</span>
                            <button 
                                onClick={handleSync}
                                title="Sync Database Now"
                                style={{
                                    background: 'transparent',
                                    border: 'none',
                                    padding: 0,
                                    margin: 0,
                                    cursor: 'pointer',
                                    display: 'flex',
                                    alignItems: 'center',
                                    color: 'var(--color-primary)',
                                    animation: isSyncing ? 'spin 0.8s linear infinite' : 'none'
                                }}
                            >
                                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.8" strokeLinecap="round" strokeLinejoin="round" style={{ pointerEvents: 'none' }}>
                                    <path d="M21.5 2v6h-6M21.34 15.57a10 10 0 1 1-.57-8.38l5.67-5.67" />
                                </svg>
                            </button>
                        </div>
                    </div>
                )}
            </div>

            <div className="lms-flex-row lms-header-actions">
                <button className="lms-icon-btn" title="Notifications" style={{ position: 'relative' }}>
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.1" strokeLinecap="round" strokeLinejoin="round" style={{ pointerEvents: 'none' }}>
                        <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
                        <path d="M13.73 21a2 2 0 0 1-3.46 0" />
                    </svg>
                    <span className="lms-notification-badge" style={{ position: 'absolute', top: '10px', right: '10px', width: '8px', height: '8px', borderRadius: '50%', background: '#ef4444', border: '1.5px solid var(--color-bg-alt)' }} />
                </button>

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

