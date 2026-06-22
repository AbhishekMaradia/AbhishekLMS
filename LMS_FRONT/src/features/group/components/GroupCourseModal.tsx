import React, { useState } from 'react';
import { Icons } from '../../../shared/components/lms/Icons';
import '../Group.css';

interface GroupCourseModalProps {
    gcModal: any;
    setGcModal: (val: any) => void;
    toggleAllVisibleCourses: (select: boolean, category?: string) => void;
    toggleCourse: (courseId: number) => void;
    saveGroupCourses: () => Promise<void>;
}

export const GroupCourseModal: React.FC<GroupCourseModalProps> = ({
    gcModal, setGcModal,
    toggleAllVisibleCourses, toggleCourse, saveGroupCourses
}) => {
    const [selectedCat, setSelectedCat] = useState<string>('all');

    if (!gcModal) return null;

    const searchQ = (gcModal.search || "").toLowerCase();
    const categories = Array.from(new Set((gcModal.courses || []).map((c: any) => c.categoryName || 'Curriculum'))).filter(Boolean);
    const masterList = (gcModal.courses || []).filter((c: any) => {
        if (!c) return false;
        const isActive = (c.isActive ?? c.IsActive) !== false;
        if (!isActive) return false;
        const nameMatch = String(c.courseName || c.title || "").toLowerCase().includes(searchQ);
        const catMatch = selectedCat === 'all' || (c.categoryName || 'Curriculum') === selectedCat;
        return nameMatch && catMatch;
    });

    const selectedCount = (gcModal.courses || []).filter((c: any) => c.isEnable).length;

    return (
        <div className="lms-modal-overlay lms-modal-overlay-smooth lms-gc-overlay" onClick={() => setGcModal(null)}>
            <div
                className="lms-modal-content lms-slide-up lms-gc-content"
                onClick={e => e.stopPropagation()}
            >
                {/* Header */}
                <div className="lms-gc-header">
                    <div className="lms-gc-header-left">
                        <div className="lms-gc-icon-box">
                            <Icons.Book s={22} />
                        </div>
                        <div>
                            <h2 className="lms-gc-title">Course Inventory</h2>
                            <div className="lms-gc-subtitle">{gcModal.groupName}</div>
                        </div>
                    </div>
                    <button onClick={() => setGcModal(null)} className="lms-gc-close-btn">
                        <Icons.Close s={24} />
                    </button>
                </div>

                {/* Body */}
                <div className="lms-gc-body">
                    <div className="lms-gc-sidebar">
                        <div className="lms-gc-stats-box">
                            <div className="lms-gc-stats-label">SELECTED</div>
                            <div className="lms-gc-stats-val">{selectedCount}</div>
                            <div className="lms-gc-stats-sub">COURSES</div>
                        </div>
                        <div className="lms-gc-sidebar-actions">
                            <button className="lms-btn lms-btn-ghost lms-gc-sidebar-btn" onClick={() => toggleAllVisibleCourses(true, selectedCat)}>Select All</button>
                            <button className="lms-btn lms-btn-ghost lms-gc-sidebar-btn" onClick={() => toggleAllVisibleCourses(false, selectedCat)}>Clear All</button>
                        </div>
                    </div>
                    <div className="lms-gc-main">
                        <div className="lms-gc-search-wrap">
                            <div className="lms-gc-search-inner">
                                <Icons.Search className="lms-gc-search-icon" s={18} />
                                <input placeholder="Filter courses..." value={gcModal.search} onChange={e => setGcModal({ ...gcModal, search: e.target.value })}
                                    className="lms-gc-search-input"
                                />
                            </div>
                            <div className="lms-gc-filter-inner">
                                <select className="lms-gc-filter-select"
                                    value={selectedCat} onChange={e => setSelectedCat(e.target.value)}
                                >
                                    <option value="all">Categories</option>
                                    {categories.map((cat: any) => <option key={cat} value={cat}>{cat}</option>)}
                                </select>
                                <Icons.ChevronDown s={14} className="lms-gc-filter-icon" />
                            </div>
                        </div>
                        <div className="lms-custom-scrollbar lms-gc-grid-wrap">
                            <div className="lms-gc-grid">
                                {gcModal.loading ? (
                                    Array.from({ length: 6 }).map((_, i) => (
                                        <div key={i} className="lms-skeleton-pulse lms-gc-skeleton" />
                                    ))
                                ) : masterList.length === 0 ? (
                                    <div className="lms-gc-empty">
                                        <Icons.Alert s={32} className="lms-gc-empty-icon" />
                                        <div className="lms-gc-empty-text">No matches in this category</div>
                                    </div>
                                ) : (
                                    masterList.map((c: any) => {
                                        const cId = c.courseId || c.id || c.Id;
                                        return (
                                            <div key={cId} onClick={() => toggleCourse(cId)} className={`lms-gc-card ${c.isEnable ? 'active' : ''}`}>
                                                <div className={`lms-gc-checkbox ${c.isEnable ? 'active' : ''}`}>
                                                    {c.isEnable && <Icons.Check s={14} />}
                                                </div>
                                                <div className="lms-gc-card-info">
                                                    <div className="lms-gc-card-name">{c.courseName || c.title}</div>
                                                    <div className="lms-gc-card-cat">{c.categoryName || 'Curriculum'}</div>
                                                </div>
                                            </div>
                                        );
                                    })
                                )}
                            </div>
                        </div>
                    </div>
                </div>

                {/* Footer */}
                <div className="lms-gc-footer">
                    <button onClick={() => setGcModal(null)} className="lms-gc-cancel">Cancel</button>
                    <button
                        className="lms-btn-commit lms-gc-commit"
                        style={{ opacity: gcModal.saving ? 0.7 : 1 }}
                        onClick={saveGroupCourses}
                        disabled={gcModal.saving}
                    >
                        {gcModal.saving ? 'Saving...' : 'Save'}
                    </button>
                </div>
            </div>
        </div>
    );
};
