import React from 'react';
import { Sidebar } from '../shared/components/layout/Sidebar';
import { Header } from '../shared/components/layout/Header';
import { Icons } from '../shared/components/lms/Icons';
import '../shared/components/layout/Layout.css';

interface StudioLayoutProps {
    children: React.ReactNode;
    user: any;
    ui: any;
    tab: string;
    setTab: (t: string) => void;
    hasPermission: (m: string, a: string) => boolean;
    isSuperAdmin: boolean;
    setUi: (u: any | ((prev: any) => any)) => void;
    activeOrg?: any;
    fetchData?: () => void;
    lastSyncedAt?: Date | null;
}


export const StudioLayout: React.FC<StudioLayoutProps> = ({
    children, user, ui, setUi, tab, setTab, hasPermission, isSuperAdmin, activeOrg, fetchData, lastSyncedAt
}) => {

    const [sidebarOpen, setSidebarOpen] = React.useState(false);
    // Architectural Safety Guard:
    // If the session is authenticated but the user profile is null (synchronizing),
    // we block the entire sub-tree to prevent undefined/null pointer crashes.
    if (!user) {
        return (
            <div className="lms-studio-frame lms-studio-auth-sync">
                <div className="lms-studio-auth-sync-text">
                    <div className="lms-loader-spinner lms-studio-auth-sync-loader" />
                    <h2 className="lms-studio-auth-sync-title">Synchronizing Identity Hub...</h2>
                    <p className="lms-studio-auth-sync-desc">Establishing secure context and permissions...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="lms-studio-frame">
            <div
                className={`lms-sidebar-overlay ${sidebarOpen ? 'active' : ''}`}
                onClick={() => setSidebarOpen(false)}
            />

            <div className={`lms-sidebar-stack ${sidebarOpen ? 'open' : ''}`}>
                <Sidebar
                    activeTab={tab}
                    setTab={(t: string) => { setTab(t); setSidebarOpen(false); }}
                    user={user}
                    hasPermission={hasPermission}
                    isSuperAdmin={isSuperAdmin}
                    sidebarOpen={sidebarOpen}
                    setSidebarOpen={setSidebarOpen}
                    activeOrg={activeOrg}
                />

            </div>

            <div className="lms-main-stack">
                <div className="lms-main-content">
                    <Header
                        user={user}
                        tab={tab}
                        setTab={setTab}
                        setUi={setUi}
                        sidebarOpen={sidebarOpen}
                        setSidebarOpen={setSidebarOpen}
                        activeOrg={activeOrg}
                        fetchData={fetchData}
                        lastSyncedAt={lastSyncedAt}
                    />


                    <main className="lms-content-scroll">
                        <div className="lms-content-inner">
                            {children}
                        </div>
                    </main>
                </div>
            </div>
        </div>
    );
};
