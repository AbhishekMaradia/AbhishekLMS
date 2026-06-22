import React from 'react';
import { Icons } from '../shared/components/lms/Icons';
import { CourseList } from '../features/course/components/CourseList';
import { Pagination, Button, PerspectiveSwitcher, SearchInput, StatusFilter } from '../shared/components/lms/LmsComponents';
import '../features/course/components/Course.css';

interface CoursesPageProps {
    db: any;
    ui: any;
    setUi: (u: any) => void;
    user: any;
    isSuperAdmin: boolean;
    searchTerm: string;
    setSearchTerm: (s: string) => void;
    viewMode: 'table' | 'grid';
    setViewMode: (v: 'table' | 'grid') => void;
    pagination: any;
    changePage: (e: string, p: number) => void;
    changePageSize: (e: string, s: number) => void;
    hasPermission: (m: string, a?: string) => boolean;
    handleCrud: (a: string, t: string, d: any) => void;
}

export const CoursesPage: React.FC<CoursesPageProps> = ({
    db, ui, setUi, user, isSuperAdmin, searchTerm, setSearchTerm,
    viewMode, setViewMode, pagination, changePage, changePageSize, hasPermission, handleCrud
}) => {
    const p = pagination['curr'] || { page: 1, size: 50, total: 0 };
    
    // Filter courses client-side for correct counts/pagination when filtered by status
    const filteredCourses = (db.courses || []).filter((c: any) => {
        const activeVal = c.isEnable ?? c.IsEnable;
        const matchesStatus =
            (ui.statusFilter || 'all') === 'all' ? true :
                (ui.statusFilter || 'all') === 'active' ? (activeVal !== false) :
                    (activeVal === false);
        return matchesStatus;
    });

    const adjustedTotal = p.total <= (p.size || 50) ? filteredCourses.length : p.total;
    const totalPages = Math.ceil(adjustedTotal / (p.size || 50)) || 1;

    const canCreate = isSuperAdmin || hasPermission('COURSE', 'COURSE_ADD');

    return (
        <div className="lms-courses-page lms-fade-in">
            <div className="lms-premium-card">
                <div className="lms-entity-header">
                    <div className="lms-section-heading">
                        <h1 className="lms-card-title">Courses</h1>
                        <span className="lms-section-count">{adjustedTotal} courses</span>
                    </div>
                    <div className="lms-card-actions">
                        <PerspectiveSwitcher viewMode={viewMode} setViewMode={setViewMode} />
                        {canCreate && (
                            <Button
                                variant="primary"
                                onClick={() => setUi({ ...ui, modal: 'course_create' })}
                                className="lms-btn-primary lms-courses-add-btn"
                            >
                                <Icons.Plus s={18} /> ADD COURSE
                            </Button>
                        )}
                    </div>
                </div>

                <div className="lms-entity-filters">
                    <div className="lms-entity-search">
                        <SearchInput value={searchTerm} onChange={setSearchTerm} placeholder="Filter by course name or ID..." />
                    </div>
                    <StatusFilter
                        value={ui.statusFilter}
                        onChange={(v) => setUi({ ...ui, statusFilter: v })}
                    />
                </div>
            </div>

            <div className="lms-container">
                <CourseList
                    courses={db.courses}
                    cats={(db.cats && db.cats.length > 0) ? db.cats : (db.cat || [])}
                    orgs={db.orgs}
                    viewMode={viewMode}
                    hasPermission={hasPermission}
                    setUi={setUi} ui={ui}
                    handleCrud={handleCrud}
                    user={user}
                    isSuperAdmin={isSuperAdmin}
                    courseStatusFilter={ui.statusFilter}
                    openCourseModal={(c: any) => setUi({ ...ui, modal: 'course_edit', target: c })}
                    openCmStudio={(c: any) => setUi({ ...ui, modal: 'cm_studio', target: c })}
                    loading={ui.loading}
                />

                <Pagination
                    current={p.page}
                    total={totalPages}
                    totalItems={adjustedTotal}
                    itemsPerPage={p.size}
                    onPageChange={(page: number) => changePage('curr', page)}
                    onPageSizeChange={(size: number) => changePageSize('curr', size)}
                />
            </div>
        </div>
    );
};
