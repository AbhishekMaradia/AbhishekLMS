import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { DashboardPage } from '../../pages/DashboardPage';
import { UsersPage } from '../../pages/UsersPage';
import { OrganizationsPage } from '../../pages/OrganizationsPage';
import { CategoriesPage } from '../../pages/CategoriesPage';
import { CoursesPage } from '../../pages/CoursesPage';
import { GroupsPage } from '../../pages/GroupsPage';
import { SecurityPage } from '../../pages/SecurityPage';
import { MediaPage } from '../../pages/MediaPage';
import { StudentDashboard } from '../../features/student/components/StudentDashboard';
import { StudentCourseGrid } from '../../features/student/components/StudentCourseGrid';
import { StudentPeerList } from '../../features/student/components/StudentPeerList';
import StudentReports from '../../features/student/components/StudentReports';
import { EnrollmentsPage } from '../../pages/EnrollmentsPage';
import { ReportsPage } from '../../pages/ReportsPage';

interface AppRoutesProps {
    tab: string;
    setTab: (t: string) => void;
    db: any;
    ui: any;
    setUi: (u: any) => void;
    user: any;
    isSuperAdmin: boolean;
    searchTerm: string;
    setSearchTerm: (s: string) => void;
    viewMode: 'table' | 'grid';
    setViewMode: (v: 'table' | 'grid') => void;
    groupTab: 'groups' | 'gc' | 'gu';
    setGroupTab: (t: 'groups' | 'gc' | 'gu') => void;
    pagination: any;
    changePage: (e: string, p: number) => void;
    changePageSize: (e: string, s: number) => void;
    hasPermission: (m: string, a?: string) => boolean;
    fetchData: () => void;
    handleCrud: (a: string, t: string, d: any) => Promise<void>;
    onAuthComplete: (u: any, p: any) => void;
    counts: any;
    openGroupUsers: (g: any) => void;
    openGroupCourses: (g: any) => void;
    guModal: any;
    setGuModal: (v: any) => void;
    gcModal: any;
    setGcModal: (v: any) => void;
    toggleCourse: (id: number) => void;
    saveGroupCourses: () => Promise<void>;
    toggleUserInGroup: (id: number) => void;
    saveGroupUsers: () => Promise<void>;
    handleMediaUpload: (e: React.FormEvent<HTMLFormElement>, type: 'vid' | 'doc', tid?: number) => Promise<void>;
    handleMediaEdit: (e: React.FormEvent<HTMLFormElement>) => Promise<void>;
    handleMediaDelete: (id: number, type: 'vid' | 'doc') => Promise<void>;
    courseMedia: any;
    editTarget: any;
    setEditTarget: (t: any) => void;
    mediaViewMode: 'table' | 'grid';
    setMediaViewMode: (v: 'table' | 'grid') => void;
    previewMedia: any;
    setPreviewMedia: (m: any) => void;
    // Security Specific
    pm: any;
    setPm: React.Dispatch<React.SetStateAction<any>>;
    pmSearch: string;
    setPmSearch: React.Dispatch<React.SetStateAction<string>>;
    togglePermission: (id: number) => void;
    openModPM: (m: any, r?: any, tId?: number | null) => Promise<void>;
    savePermissions: () => Promise<void>;
    // Student Specific
    isStudent: boolean;
    subscriptions: number[];
    toggleSubscription: (id: number) => void;
    playCourse: (course: any) => void;
    revokeEnrollment: (userId: number, courseId: number) => Promise<void>;
}

export const AppRoutes: React.FC<AppRoutesProps> = (props) => {
    const { isStudent, tab, db, subscriptions, toggleSubscription, playCourse, user, counts, revokeEnrollment } = props;

    if (isStudent) {
        return (
            <Routes>
                <Route 
                    path="/student/dashboard" 
                    element={
                        <StudentDashboard 
                            user={user} counts={counts} db={db} subscriptions={subscriptions} 
                            onPlay={playCourse} 
                            onShowPreview={playCourse} 
                            loading={props.ui.loading} 
                        />
                    } 
                />
                <Route
                    path="/student/discover"
                    element={
                        <StudentCourseGrid
                            courses={db.courses}
                            subscriptions={subscriptions}
                            onSubscribe={toggleSubscription}
                            onPlay={playCourse}
                            onShowPreview={(course: any) => {
                                const isSubscribed = subscriptions.includes(Number(course.courseId || course.CourseId || course.id || course.Id));
                                if (isSubscribed) playCourse(course);
                                else props.setUi({ modal: 'student_course_preview', target: course });
                            }}
                            title="Discover Courses"
                            description="Explore courses assigned specifically to your group."
                            loading={props.ui.loading}
                        />
                    }
                />
                <Route
                    path="/student/my-courses"
                    element={
                        <StudentCourseGrid
                            courses={db.courses.filter((c: any) => subscriptions.includes(Number(c.courseId || c.CourseId || c.id || c.Id)))}
                            subscriptions={subscriptions}
                            onPlay={playCourse}
                            onShowPreview={playCourse}
                            title="My Courses"
                            description="Access your enrolled courses and continue your progress."
                            loading={props.ui.loading}
                        />
                    }
                />
                <Route path="/student/peers" element={<StudentPeerList peers={db.users} currentUser={user} loading={props.ui.loading} />} />
                <Route path="/student/reports" element={<StudentReports user={user} subscriptions={subscriptions} courses={db.courses} />} />

                <Route path="/" element={<Navigate to="/student/dashboard" replace />} />
                <Route path="*" element={<Navigate to="/student/dashboard" replace />} />
            </Routes>
        );
    }

    return (
        <Routes>
            <Route path="/dashboard" element={<DashboardPage {...props} />} />
            <Route path="/users" element={<UsersPage {...props} />} />
            <Route path="/organizations" element={<OrganizationsPage {...props} />} />
            <Route path="/categories" element={<CategoriesPage {...props} />} />
            <Route path="/courses" element={<CoursesPage {...props} />} />
            <Route path="/groups" element={<GroupsPage {...props} />} />
            <Route path="/media" element={<MediaPage {...props} />} />
            <Route path="/security" element={<SecurityPage {...props} />} />
            <Route path="/enrollments" element={<EnrollmentsPage {...props} fetchData={props.fetchData} revokeEnrollment={revokeEnrollment} />} />
            <Route path="/reports" element={<ReportsPage {...props} />} />

            <Route path="/" element={<Navigate to="/dashboard" replace />} />
            <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
    );
};

export default AppRoutes;
