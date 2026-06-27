import React from 'react';
import { Icons, SecureImage, CommonGrid } from '../../../shared/components/lms/LmsComponents';
import { CourseGridSkeleton } from '../../../shared/components/lms/LmsSkeleton';

interface StudentCourseGridProps {
    courses: any[];
    subscriptions?: number[];
    onSubscribe?: (id: number) => void;
    onPlay?: (course: any) => void;
    onShowPreview?: (course: any) => void;
    title: string;
    description: string;
    loading?: boolean;
}

export const StudentCourseGrid: React.FC<StudentCourseGridProps> = ({
    courses, subscriptions = [], onSubscribe, onPlay, onShowPreview, title, description, loading
}) => {
    if (loading) {
        return (
            <div className="lms-student-courses lms-fade-in">
                <div className="lms-premium-card" style={{ marginBottom: '24px' }}>
                    <div className="lms-section-header">
                        <h1 className="lms-card-title">{title}</h1>
                        <p className="lms-status-sub">{description}</p>
                    </div>
                </div>
                <CourseGridSkeleton />
            </div>
        );
    }

    return (
        <div className="lms-student-courses lms-fade-in">
            <div className="lms-premium-card" style={{ marginBottom: '24px' }}>
                <div className="lms-section-header">
                    <h1 className="lms-card-title">{title}</h1>
                    <p className="lms-status-sub">{description}</p>
                </div>
            </div>

            <CommonGrid
                loading={loading}
                empty={courses.length === 0}
                emptyMessage="There are currently no courses assigned to your group."
            >
                {courses.map((course) => {
                    const id = Number(course.courseId || course.CourseId || course.id || course.Id);
                    const isSubscribed = subscriptions.includes(id);

                    return (
                        <div
                            key={id}
                            className={`lms-grid-card lms-fade-in ${isSubscribed ? 'enrolled' : ''}`}
                            onClick={() => onShowPreview?.(course)}
                        >
                            <div className="lms-grid-banner">
                                <div className="lms-grid-overlay" />
                                {(course.imageUrl || course.courseMainImageUrl || course.thumbnailUrl || course.ImageUrl || course.CourseMainImageUrl || course.ThumbnailUrl) ? (
                                    <SecureImage src={course.imageUrl || course.courseMainImageUrl || course.thumbnailUrl || course.ImageUrl || course.CourseMainImageUrl || course.ThumbnailUrl} className="lms-grid-banner-img" />
                                ) : (
                                    <div className="lms-status-icon-bg"><Icons.Book s={28} /></div>
                                )}

                                {isSubscribed && (
                                    <div className="lms-grid-badge">
                                        <span className="lms-tag success">ENROLLED</span>
                                    </div>
                                )}
                            </div>

                            <div className="lms-grid-body">
                                <div className="lms-grid-tag">{course.categoryName || course.CategoryName || 'General Curriculum'}</div>
                                <h3 className="lms-grid-title">{course.title || course.courseName || course.Title || course.CourseName}</h3>

                                <div className="lms-grid-meta">
                                    <Icons.Video s={12} />
                                    <span>{course.videoUrls?.length || course.lectures || course.Lectures || course.videoCount || course.VideoCount || 0} Lessons</span>
                                    <span className="lms-dot-sep" />
                                    <Icons.Doc s={12} />
                                    <span>{course.docCount || course.DocCount || 0} Docs</span>
                                </div>

                                <div className="lms-grid-description">
                                    {course.description || course.Description || course.courseDescription || course.CourseDescription || "Start your learning journey with this comprehensive course module."}
                                </div>

                                <div className="lms-grid-footer">
                                    {onSubscribe ? (
                                        <button
                                            className={`lms-btn lms-flex-1 ${isSubscribed ? 'lms-btn-success' : 'lms-btn-primary'}`}
                                            onClick={() => onSubscribe(id)}
                                        >
                                            {isSubscribed ? <><Icons.Check s={18} /> ENROLLED</> : <><Icons.Plus s={18} /> SUBSCRIBE</>}
                                        </button>
                                    ) : isSubscribed ? (
                                        <button
                                            className="lms-btn lms-btn-primary lms-flex-1"
                                            onClick={() => onPlay?.(course)}
                                        >
                                            <Icons.Play s={18} /> CONTINUE LEARNING
                                        </button>
                                    ) : (
                                        <div className="lms-btn lms-btn-disabled lms-flex-1">UNAVAILABLE</div>
                                    )}
                                </div>
                            </div>
                        </div>
                    );
                })}
            </CommonGrid>
        </div>
    );
};
