import React from 'react';
import { Sidebar } from '../shared/components/layout/Sidebar';
import { StudentSidebar } from '../shared/components/layout/StudentSidebar';
import { Header } from '../shared/components/layout/Header';
import { Icons } from '../shared/components/lms/Icons';
import '../shared/components/layout/Layout.css';
import '../features/student/Student.css';

import StudentCoursePlayer from '../features/student/components/StudentCoursePlayer';

interface StudentLayoutProps {
    children: React.ReactNode;
    user: any;
    ui: any;
    tab: string;
    setTab: (t: string) => void;
    hasPermission: (m: string, a: string) => boolean;
    isSuperAdmin: boolean;
    setUi: (u: any | ((prev: any) => any)) => void;
    activeOrg?: any;
    activeCourse?: any;
    setActiveCourse?: (c: any) => void;
    courseMedia?: any;
    setPreviewMedia?: (m: any) => void;
}


export const StudentLayout: React.FC<StudentLayoutProps> = ({
    children, user, ui, setUi, tab, setTab, hasPermission, isSuperAdmin, activeOrg, activeCourse, setActiveCourse, courseMedia, setPreviewMedia
}) => {

    const [sidebarOpen, setSidebarOpen] = React.useState(false);

    if (!user) {
        return (
            <div className="lms-studio-frame lms-studio-auth-sync">
                <div className="lms-studio-auth-sync-text">
                    <div className="lms-loader-spinner lms-studio-auth-sync-loader" />
                    <h2 className="lms-studio-auth-sync-title">Initializing Academic Portal...</h2>
                    <p className="lms-studio-auth-sync-desc">Syncing your learning journey...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="lms-studio-frame lms-student-portal">
            <div 
                className={`lms-sidebar-overlay ${sidebarOpen ? 'active' : ''}`} 
                onClick={() => setSidebarOpen(false)} 
            />
            
            <div className={`lms-sidebar-stack ${sidebarOpen ? 'open' : ''}`}>
                <StudentSidebar
                    activeTab={tab}
                    setTab={(t: string) => { setTab(t); setSidebarOpen(false); }}
                    user={user}
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
                    />


                    <main className="lms-content-scroll">
                        <div className="lms-content-inner">
                            {children}
                        </div>
                    </main>
                </div>
            </div>

            {activeCourse && (
                <StudentCoursePlayer 
                    course={activeCourse} 
                    media={courseMedia}
                    onClose={() => setActiveCourse?.(null)} 
                    setPreviewMedia={setPreviewMedia}
                />
            )}
        </div>
    );
};
