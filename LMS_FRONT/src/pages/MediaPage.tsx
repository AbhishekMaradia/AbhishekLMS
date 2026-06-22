import React from 'react';
import { CourseMediaStudio } from '../features/course/components/CourseMediaStudio';
import { CourseList } from '../features/course/components/CourseList';
import { Pagination, PerspectiveSwitcher, SearchInput } from '../shared/components/lms/LmsComponents';
import '../features/course/components/CourseMedia.css';

interface MediaPageProps {
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
}

export const MediaPage: React.FC<MediaPageProps> = (props) => {
    const { db, ui, setUi, handleMediaUpload, handleMediaEdit, handleMediaDelete, courseMedia, editTarget, setEditTarget } = props;
    const p = props.pagination?.['courses'] || { page: 1, size: 50, total: 0 };

    // If a course is selected, show the Studio
    if (ui.target && ui.modal === 'cm_studio') {
        return (
            <div className="lms-media-page lms-fade-in">
                <CourseMediaStudio
                    {...props}
                    course={ui.target}
                    cmStudioTab={ui.cmStudioTab || 'videos'}
                    setCmStudioTab={(t: string) => setUi({ ...ui, cmStudioTab: t })}
                    cmMedia={courseMedia || { vids: [], docs: [], loading: false }}
                    setPreviewMedia={props.setPreviewMedia}
                    setCmTarget={(c: any) => setUi({ ...ui, target: c })}
                    handleCmDelete={handleMediaDelete}
                    handleCmUpload={(e: React.FormEvent, t: 'vid' | 'doc') => {
                        const courseId = ui.target?.id || ui.target?.Id || ui.target?.courseId || ui.target?.CourseId;
                        console.log("[LMS DEBUG] handleCmUpload proxy executed", t, courseId);
                        return handleMediaUpload(e as React.FormEvent<HTMLFormElement>, t, courseId);
                    }}
                    handleCmEdit={handleMediaEdit}
                    cmEditTarget={editTarget}
                    setCmEditTarget={setEditTarget}
                    mediaViewMode={props.mediaViewMode}
                    setMediaViewMode={props.setMediaViewMode}
                    user={props.user}
                    isSuperAdmin={props.isSuperAdmin}
                    orgs={db.orgs}
                />
            </div>
        );
    }

    return (
        <div className="lms-media-page lms-fade-in">
            <div className="lms-premium-card lms-media-header-card">
                <div className="lms-entity-header">
                    <div className="lms-section-heading">
                        <h1 className="lms-card-title lms-media-title">Course Material</h1>
                        <span className="lms-section-count">{p.total} Courses</span>
                    </div>
                    <div className="lms-card-actions">
                        <PerspectiveSwitcher viewMode={props.viewMode} setViewMode={props.setViewMode} />
                    </div>
                </div>

                <div className="lms-entity-filters">
                    <div className="lms-entity-search">
                        <SearchInput
                            value={props.searchTerm}
                            onChange={props.setSearchTerm}
                            placeholder="Search courses..."
                        />
                    </div>
                </div>
            </div>

            <div className="lms-container">
                <CourseList
                    {...props}
                    courses={db.courses}
                    cats={(db.cats && db.cats.length > 0) ? db.cats : (db.cat || [])}
                    orgs={db.orgs}
                    tab="cm"
                    loading={ui.loading}
                    openCmStudio={(c: any) => setUi({ ...ui, modal: 'cm_studio', target: c })}
                />

                <Pagination
                    current={p.page}
                    total={Math.ceil(p.total / p.size) || 1}
                    totalItems={p.total}
                    itemsPerPage={p.size}
                    onPageChange={(page: number) => props.changePage('courses', page)}
                    onPageSizeChange={(size: number) => props.changePageSize('courses', size)}
                />
            </div>
        </div>
    );
};
