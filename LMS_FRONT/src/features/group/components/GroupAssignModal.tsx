import React, { useState } from 'react';
import { Icons } from '../../../shared/components/lms/Icons';
import '../Group.css';

interface GroupAssignModalProps {
    gcModal: any;
    setGcModal: (val: any) => void;
    guModal: any;
    setGuModal: (val: any) => void;
    toggleAllVisibleCourses: (select: boolean) => void;
    toggleCourse: (courseId: number) => void;
    saveGroupCourses: () => Promise<void>;
    toggleAllVisibleUsers: (select: boolean) => void;
    toggleUserInGroup: (userId: number) => void;
    saveGroupUsers: () => Promise<void>;
}

export const GroupAssignModal: React.FC<GroupAssignModalProps> = ({
    gcModal, setGcModal,
    guModal, setGuModal,
    toggleAllVisibleCourses, toggleCourse, saveGroupCourses,
    toggleAllVisibleUsers, toggleUserInGroup, saveGroupUsers
}) => {
    const Layout = ({ sidebar, content, footer, title, icon: Icon, onClose }: { sidebar: any, content: any, footer: any, title: string, icon: any, onClose: () => void }) => (
        <div className="lms-modal-overlay lms-modal-overlay-smooth lms-ga-overlay" onClick={onClose}>
            <div
                className="lms-modal-content lms-slide-up lms-ga-content"
                onClick={e => e.stopPropagation()}
            >
                {/* Modal Header */}
                <div className="lms-ga-header">
                    <div className="lms-ga-header-left">
                        <div className="lms-ga-icon-box">
                            <Icon s={22} />
                        </div>
                        <div>
                            <h2 className="lms-ga-title">{title}</h2>
                            <div className="lms-ga-subtitle">Resource Manager</div>
                        </div>
                    </div>
                    <button onClick={onClose} className="lms-ga-close-btn">
                        <Icons.Close s={24} />
                    </button>
                </div>

                {/* Modal Body */}
                <div className="lms-ga-body">
                    <div className="lms-ga-sidebar">{sidebar}</div>
                    <div className="lms-ga-main">
                        {content}
                    </div>
                </div>

                {/* Modal Footer */}
                <div className="lms-ga-footer">
                    {footer}
                </div>
            </div>
        </div>
    );

    if (gcModal) {
        const searchQ = (gcModal.search || "").toLowerCase();
        const masterList = (gcModal.courses || []).filter((c: any) => {
            if (!c) return false;
            const isActive = (c.isActive ?? c.IsActive) !== false;
            if (!isActive) return false;
            return String(c.courseName || c.title || "").toLowerCase().includes(searchQ);
        });

        const selectedCount = (gcModal.courses || []).filter((c: any) => c.isEnable).length;

        return (
            <Layout
                title="Manage Courses"
                icon={Icons.Book}
                onClose={() => setGcModal(null)}
                sidebar={
                    <>
                        <div className="lms-ga-sidebar-box">
                            <div className="lms-ga-stats-label">SELECTED</div>
                            <div className="lms-ga-stats-val">{selectedCount}</div>
                            <div className="lms-ga-stats-sub">COURSES</div>
                        </div>
                        <div className="lms-ga-sidebar-actions">
                            <button className="lms-ga-pill-btn" onClick={() => toggleAllVisibleCourses(true)}>Select All</button>
                            <button className="lms-ga-pill-btn" onClick={() => toggleAllVisibleCourses(false)}>Clear All</button>
                        </div>
                    </>
                }
                content={
                    <>
                        <div className="lms-ga-search-wrap">
                            <div className="lms-ga-search-inner">
                                <Icons.Search className="lms-ga-search-icon" s={18} />
                                <input placeholder="Filter courses..." value={gcModal.search} onChange={e => setGcModal({ ...gcModal, search: e.target.value })}
                                    className="lms-ga-search-input"
                                />
                            </div>
                        </div>
                        <div className="lms-custom-scrollbar lms-ga-grid-wrap">
                            <div className="lms-ga-grid">
                                {gcModal.loading ? (
                                    Array.from({ length: 6 }).map((_, i) => (
                                        <div key={i} className="lms-skeleton-pulse lms-ga-skeleton" />
                                    ))
                                ) : masterList.length === 0 ? (
                                    <div className="lms-ga-empty">
                                        <Icons.Alert s={32} className="lms-ga-empty-icon" />
                                        <div className="lms-ga-empty-text">No matches in this category</div>
                                    </div>
                                ) : (
                                    masterList.map((c: any) => {
                                        const cId = c.courseId || c.id || c.Id;
                                        return (
                                            <div key={cId} onClick={() => toggleCourse(cId)} className={`lms-ga-card ${c.isEnable ? 'active' : ''}`}>
                                                <div className={`lms-ga-checkbox lms-ga-checkbox-course ${c.isEnable ? 'active' : ''}`}>
                                                    {c.isEnable && <Icons.Check s={14} />}
                                                </div>
                                                <div className="lms-ga-card-info">
                                                    <div className="lms-ga-card-name">{c.courseName || c.title}</div>
                                                    <div className="lms-ga-card-sub lms-ga-card-cat">{c.categoryName || 'Curriculum'}</div>
                                                </div>
                                            </div>
                                        );
                                    })
                                )}
                            </div>
                        </div>
                    </>
                }
                footer={
                    <>
                        <button onClick={() => setGcModal(null)} className="lms-ga-cancel">Cancel</button>
                        <button
                            className="lms-ga-commit"
                            style={{ opacity: gcModal.saving ? 0.7 : 1 }}
                            onClick={saveGroupCourses}
                            disabled={gcModal.saving}
                        >
                            {gcModal.saving ? 'Saving...' : 'Save'}
                        </button>
                    </>
                }
            />
        );
    }

    if (guModal) {
        const searchQ = (guModal.search || "").toLowerCase();
        const masterList = (guModal.users || []).filter((u: any) => {
            const isActive = (u.isActive ?? u.IsActive) !== false;
            if (!isActive) return false;
            const name = String(u.firstName || "") + " " + String(u.lastName || "");
            const email = String(u.email || "");
            return name.toLowerCase().includes(searchQ) || email.toLowerCase().includes(searchQ);
        });
        const assignedCount = (guModal.users || []).filter((u: any) => u.isAssigned).length;

        return (
            <Layout
                title="Manage Enrollment"
                icon={Icons.User}
                onClose={() => setGuModal(null)}
                sidebar={
                    <>
                        <div className="lms-ga-sidebar-box">
                            <div className="lms-ga-stats-label">ASSIGNED</div>
                            <div className="lms-ga-stats-val">{assignedCount}</div>
                            <div className="lms-ga-stats-sub">MEMBERS</div>
                        </div>
                        <div className="lms-ga-sidebar-actions">
                            <button className="lms-ga-pill-btn" onClick={() => toggleAllVisibleUsers(true)}>Select All</button>
                            <button className="lms-ga-pill-btn" onClick={() => toggleAllVisibleUsers(false)}>Clear All</button>
                        </div>
                    </>
                }
                content={
                    <>
                        <div className="lms-ga-search-wrap">
                            <div className="lms-ga-search-inner">
                                <Icons.Search className="lms-ga-search-icon" s={18} />
                                <input placeholder="Search members..." value={guModal.search} onChange={e => setGuModal({ ...guModal, search: e.target.value })}
                                    className="lms-ga-search-input"
                                />
                            </div>
                        </div>
                        <div className="lms-custom-scrollbar lms-ga-grid-wrap">
                            <div className="lms-ga-grid">
                                {guModal.loading ? (
                                    Array.from({ length: 8 }).map((_, i) => (
                                        <div key={i} className="lms-skeleton-pulse lms-ga-skeleton" />
                                    ))
                                ) : masterList.length === 0 ? (
                                    <div className="lms-ga-empty">
                                        <Icons.Alert s={32} className="lms-ga-empty-icon" />
                                        <div className="lms-ga-empty-text">No matching members</div>
                                    </div>
                                ) : (
                                    masterList.map((u: any) => {
                                        const uId = u.userId || u.id || u.Id;
                                        return (
                                            <div key={uId} onClick={() => toggleUserInGroup(uId)} className={`lms-ga-card ${u.isAssigned ? 'active' : ''}`}>
                                                <div className={`lms-ga-checkbox ${u.isAssigned ? 'active' : ''}`}>
                                                    {u.isAssigned && <Icons.Check s={14} />}
                                                </div>
                                                <div className="lms-ga-card-info">
                                                    <div className="lms-ga-card-name">{u.firstName} {u.lastName}</div>
                                                    <div className="lms-ga-card-sub">{u.email}</div>
                                                </div>
                                            </div>
                                        );
                                    })
                                )}
                            </div>
                        </div>
                    </>
                }
                footer={
                    <>
                        <button onClick={() => setGuModal(null)} className="lms-ga-cancel">Cancel</button>
                        <button
                            className="lms-ga-commit"
                            style={{ opacity: guModal.saving ? 0.7 : 1 }}
                            onClick={saveGroupUsers}
                            disabled={guModal.saving}
                        >
                            {guModal.saving ? 'Saving...' : 'Save'}
                        </button>
                    </>
                }
            />
        );
    }

    return null;
};
