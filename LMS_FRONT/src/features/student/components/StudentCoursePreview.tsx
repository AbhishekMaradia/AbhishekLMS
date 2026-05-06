import React from 'react';
import { Icons, SecureImage } from '../../../shared/components/lms/LmsComponents';
import { SkeletonBox } from '../../../shared/components/lms/LmsSkeleton';

interface StudentCoursePreviewProps {
    course: any;
    media: { vids: any[], docs: any[], loading: boolean };
    onStart: (course: any) => void;
    onClose: () => void;
    isEnrolled: boolean;
    onSubscribe: (id: number) => void;
}

export const StudentCoursePreview: React.FC<StudentCoursePreviewProps> = ({
    course, media, onStart, onClose, isEnrolled, onSubscribe
}) => {
    const banner = course.imageUrl || course.courseMainImageUrl;
    const id = Number(course.courseId || course.CourseId || course.id || course.Id);

    return (
        <div className="course-preview-container lms-fade-in">
            {/* Header Mirroring Studio Breadcrumbs */}
            <div className="preview-top-breadcrumb">
                <div className="lms-cms-header-breadcrumb">
                    WORKSPACE <span className="lms-cms-header-slash">/</span> ACADEMIC DISCOVERY
                </div>
                <div className="lms-modal-header-tag student-wizard-tag">HUB CONTEXT</div>
            </div>

            <div className="preview-hero">
                <div className="preview-banner-box">
                    {banner ? (
                        <SecureImage src={banner} className="preview-banner-img" />
                    ) : (
                        <div className="preview-banner-fallback">
                            <Icons.Book s={64} />
                        </div>
                    )}
                    <div className="preview-banner-overlay" />
                </div>
                
                <div className="preview-hero-content">
                    <div className="academic-category-pill">
                        {course.categoryName || 'General Curriculum'}
                    </div>
                    <h1 className="preview-title">{course.title || course.courseName}</h1>
                    <div className="preview-meta-strip">
                        <div className="meta-pill">
                            <Icons.Video s={14} />
                            <span>{media.vids.length} MODULES</span>
                        </div>
                        <div className="meta-pill">
                            <Icons.Doc s={14} />
                            <span>{media.docs.length} RESOURCES</span>
                        </div>
                        <div className="meta-pill accent">
                            <Icons.Play s={14} />
                            <span>CERTIFIED PATHWAY</span>
                        </div>
                    </div>
                </div>
            </div>

            <div className="preview-content-grid">
                <div className="preview-main">
                    <section className="preview-section-studio">
                        <div className="section-header-studio">
                             <div className="studio-bullet" />
                             <h2 className="studio-section-title">CORE CURRICULUM OVERVIEW</h2>
                        </div>
                        <p className="preview-description-studio">
                            {course.description || "Embark on a comprehensive learning journey designed to master core concepts and practical skills in this field. This curriculum follows industry best practices and provides hands-on modules."}
                        </p>
                    </section>

                    <section className="preview-section-studio">
                        <div className="section-header-studio">
                             <div className="studio-bullet accent" />
                             <h2 className="studio-section-title">ACADEMIC MODULES</h2>
                        </div>
                        <div className="preview-syllabus-studio">
                            {media.loading ? (
                                <div className="lms-skeleton-pulse" style={{ height: '200px', borderRadius: '16px' }} />
                            ) : media.vids.length > 0 ? (
                                media.vids.slice(0, 6).map((v, idx) => (
                                    <div key={v.id || idx} className="syllabus-item-studio">
                                        <div className="item-index-hex">{String(idx + 1).padStart(2, '0')}</div>
                                        <div className="item-info">
                                            <div className="item-title">{v.title}</div>
                                            <div className="item-type-studio">VIDEO TRAINING • ARCHITECTURAL LEARNING</div>
                                        </div>
                                        <button className="preview-play-btn-mini"><Icons.Play s={12} /></button>
                                    </div>
                                ))
                            ) : (
                                <div className="lms-empty-state-small">Curriculum details are being finalized for this semester.</div>
                            )}
                            {media.vids.length > 6 && (
                                <div className="syllabus-more-studio">EXPAND HUB TO VIEW ALL {media.vids.length} LESSONS</div>
                            )}
                        </div>
                    </section>
                </div>

                <div className="preview-sidebar">
                    <div className="lms-premium-card enrollment-box-studio">
                        <div className="status-label-studio">ENROLLMENT STATUS</div>
                        <div className="box-header-studio">
                            <div className="price-tag-studio">{course.price > 0 ? `₹${course.price}` : 'SUBSCRIPTION INCLUDED'}</div>
                        </div>
                        
                        <div className="box-actions">
                            {isEnrolled ? (
                                <button className="lms-btn-commit full-width studio-action-btn" onClick={() => onStart(course)}>
                                    LAUNCH WORKSPACE <Icons.Play s={18} />
                                </button>
                            ) : (
                                <button className="lms-btn-commit full-width studio-action-btn success-mode" onClick={() => onSubscribe(id)}>
                                    INITIALIZE HUB <Icons.Plus s={18} />
                                </button>
                            )}
                        </div>
                        
                        <div className="box-features-studio">
                            <div className="feature-item-studio"><div className="dot" /> Industry Grade Certificate</div>
                            <div className="feature-item-studio"><div className="dot" /> Unlimited Workspace Access</div>
                            <div className="feature-item-studio"><div className="dot" /> Cloud Resouce Supplement</div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};
