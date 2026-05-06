import React, { useState, useEffect, useRef } from 'react';
import { Icons } from '../../../shared/components/lms/LmsComponents';
import { apiClient as api, API_ORIGIN } from '../../../core/api/apiClient';

interface StudentCoursePlayerProps {
    course: any;
    media: { vids: any[], docs: any[], loading: boolean };
    onClose: () => void;
    setPreviewMedia?: (m: any) => void;
}

/**
 * High-Fidelity Learning Studio - Progress Persistence Optimized
 * Ensures real-time synchronization of learning metrics on pause, seek, and close.
 */
const StudentCoursePlayer: React.FC<StudentCoursePlayerProps> = ({ course, media, onClose, setPreviewMedia }) => {
    const [selectedVideo, setSelectedVideo] = useState<any>(null);
    const [selectedDoc, setSelectedDoc] = useState<any>(null);
    const [activeTab, setActiveTab] = useState<'curriculum' | 'resources'>('curriculum');
    const [sidebarOpen, setSidebarOpen] = useState(true);
    const videoRef = useRef<HTMLVideoElement>(null);
    const theatreRef = useRef<HTMLDivElement>(null);
    const progressInterval = useRef<any>(null);
    const user = JSON.parse(localStorage.getItem('user') || '{}');

    // Auto-select assets
    useEffect(() => {
        if (media.vids.length > 0 && !selectedVideo) setSelectedVideo(media.vids[0]);
        if (media.docs.length > 0 && !selectedDoc) setSelectedDoc(media.docs[0]);
    }, [media.vids, media.docs]);

    const saveProgress = async (isCompleted = false) => {
        // We use a local capture of current time if possible or the ref
        if (!selectedVideo || !videoRef.current) return;
        const v = videoRef.current;
        
        // Safety: If duration is not yet loaded, don't save 0 inaccurately
        if (v.duration === 0 || isNaN(v.duration)) return;

        const p = (v.currentTime / v.duration) * 100;
        const finalPercentage = isCompleted ? 100 : parseFloat(p.toFixed(4));
        
        console.log(`[LMS] Precision Sync: ${finalPercentage}% for Video ID: ${selectedVideo.id || selectedVideo.Id}`);

        try {
            await api.post('CourseVideo/update-progress', {
                videoId: selectedVideo.id || selectedVideo.Id,
                groupId: user.groupId,
                watchedPercentage: finalPercentage,
                isCompleted: isCompleted || finalPercentage >= 95
            });
        } catch (e) {
            console.error("[LMS] Progress Sync Failed", e);
        }
    };

    // Robust effect for background sync and unmount persistence
    useEffect(() => {
        if (selectedVideo) {
            // Reduced interval to 10s for better accuracy
            progressInterval.current = setInterval(() => saveProgress(), 10000);
            
            return () => {
                if (progressInterval.current) clearInterval(progressInterval.current);
                // The cleanup function is critical - save one last time before component dies
                saveProgress();
            };
        }
    }, [selectedVideo]);

    const getAuthUrl = (path: string, isDoc = false) => {
        if (!path && !isDoc) return '';
        const token = localStorage.getItem('token');
        const sfx = token ? `${path?.includes('?') ? '&' : '?'}access_token=${token}` : '';
        const origin = API_ORIGIN.endsWith('/') ? API_ORIGIN.slice(0, -1) : API_ORIGIN;

        if (isDoc && selectedDoc) {
            return `${origin}/CourseDocument/download/${selectedDoc?.id || selectedDoc?.Id}${sfx}#toolbar=0&navpanes=0&scrollbar=1`;
        }

        const id = selectedVideo?.id || selectedVideo?.Id;
        return `${origin}/CourseVideo/stream/${id}${sfx}`;
    };

    const toggleNativeFullscreen = () => {
        if (!theatreRef.current) return;
        if (!document.fullscreenElement) {
            theatreRef.current.requestFullscreen().catch(err => {
                console.error(`Error entering fullscreen: ${err.message}`);
            });
        } else {
            document.exitFullscreen();
        }
    };

    const handleBackClick = async () => {
        // Priority save before navigation triggers
        await saveProgress();
        onClose();
    };

    return (
        <div className="lms-studio-pro lms-fade-in theme-sync">
            {/* Header: Unified Precision Controls */}
            <header className="studio-top-nav">
                <div className="nav-group-left">
                    <button className="pro-circular-btn back" onClick={handleBackClick} title="Exit Session">
                        <Icons.Prev s={16} />
                    </button>
                    <div className="v-divider" />
                    <div className="brand-meta">
                        <span className="over-text">PREMIUM LEARNING ENV</span>
                        <h1 className="main-title-text">{course.title || course.Title}</h1>
                    </div>
                </div>

                <div className="nav-group-right">
                    <div className="elite-tab-system">
                        <button className={`elite-tab ${activeTab === 'curriculum' ? 'active' : ''}`} onClick={() => setActiveTab('curriculum')}>
                            <Icons.Video s={13} /> <span>Curriculum</span>
                        </button>
                        <button className={`elite-tab ${activeTab === 'resources' ? 'active' : ''}`} onClick={() => setActiveTab('resources')}>
                            <Icons.Doc s={13} /> <span>Resources</span>
                        </button>
                    </div>
                    <div className="v-divider" />
                    <button className={`pro-circular-btn toggle ${!sidebarOpen ? 'active' : ''}`} onClick={() => setSidebarOpen(!sidebarOpen)} title="Focus mode">
                        <Icons.Grid s={18} />
                    </button>
                </div>
            </header>

            <main className={`lms-studio-workspace ${!sidebarOpen ? 'focus-mode' : ''}`}>
                <section className="lms-studio-content-flow lms-custom-scrollbar">
                    {/* Cinema Stage */}
                    <div className="studio-theatre-stage" ref={theatreRef}>
                        {media.loading ? (
                            <div className="studio-loading-mask">
                                <div className="loading-spinner" />
                                <span className="loading-text">Syncing Assets...</span>
                            </div>
                        ) : activeTab === 'resources' ? (
                            selectedDoc ? (
                                <div className="cinema-media-viewport">
                                    <iframe 
                                        src={getAuthUrl('', true)} 
                                        title="Secure Doc Viewer" 
                                        className="cinema-media-element" 
                                        onContextMenu={e => e.preventDefault()}
                                    />
                                    <div className="cinema-overlay top-left">{user?.email}</div>
                                    <button className="video-fs-btn" onClick={toggleNativeFullscreen}>
                                        <Icons.Maximize s={16} />
                                    </button>
                                </div>
                            ) : (
                                <div className="studio-status-placeholder">
                                    <Icons.Doc s={64} opacity={0.1} />
                                    <p>Select a resource to begin.</p>
                                </div>
                            )
                        ) : selectedVideo ? (
                            <div className="cinema-media-viewport video" key={selectedVideo.id || selectedVideo.Id}>
                                <video
                                    ref={videoRef}
                                    src={getAuthUrl(selectedVideo.videoUrl || selectedVideo.VideoUrl)}
                                    className="cinema-media-element"
                                    controls
                                    autoPlay
                                    muted
                                    playsInline
                                    controlsList="nodownload"
                                    onContextMenu={e => e.preventDefault()}
                                    onPause={() => saveProgress()}
                                    onEnded={() => saveProgress(true)}
                                />
                                <div className="cinema-overlay top-right">{user?.email}</div>
                            </div>
                        ) : (
                            <div className="studio-status-placeholder">
                                <Icons.Video s={72} opacity={0.1} />
                                <p>Select a module to start learning.</p>
                            </div>
                        )}
                    </div>

                    {/* Meta Detail */}
                    {activeTab === 'curriculum' && selectedVideo && (
                        <article className="pro-lesson-details lms-fade-in">
                            <div className="lesson-details-header">
                                <span className="lesson-badge">CHAPTER {(media.vids.findIndex(v => (v.id || v.Id) === (selectedVideo.id || selectedVideo.Id)) + 1)}</span>
                                <h2 className="lesson-details-title">{selectedVideo.title || selectedVideo.Title}</h2>
                                <div className="lesson-details-pills">
                                    <span className="pill"><Icons.Play s={12} /> SECURE HD</span>
                                    <span className="pill"><Icons.Clock s={12} /> {selectedVideo.duration || 'N/A'}</span>
                                </div>
                            </div>
                            <div className="lesson-details-body">
                                <p>{selectedVideo.description || selectedVideo.Description || 'No additional module overview available.'}</p>
                            </div>
                        </article>
                    )}
                </section>

                {/* Sidebar */}
                <aside className="lms-studio-sidebar">
                    <div className="sidebar-header-pro">
                        <span>{activeTab === 'curriculum' ? 'CURRICULUM' : 'RESOURCES'}</span>
                        <span className="count-label">{activeTab === 'curriculum' ? media.vids.length : media.docs.length} Items</span>
                    </div>
                    <nav className="pro-sidebar-list lms-custom-scrollbar">
                        {(activeTab === 'curriculum' ? media.vids : media.docs).map((item: any, i: number) => {
                            const isSel = (activeTab === 'curriculum') 
                                ? (item.id || item.Id) === (selectedVideo?.id || selectedVideo?.Id)
                                : (item.id || item.Id) === (selectedDoc?.id || selectedDoc?.Id);
                            
                            return (
                                <div key={item.id || i} className={`pro-nav-row ${isSel ? 'active' : ''}`} onClick={() => activeTab === 'curriculum' ? setSelectedVideo(item) : setSelectedDoc(item)}>
                                    <div className="pro-nav-idx">{(i + 1).toString().padStart(2, '0')}</div>
                                    <div className="pro-nav-icon">
                                        {isSel ? <div className="active-dot" /> : <Icons.Play s={14} />}
                                    </div>
                                    <div className="pro-nav-data">
                                        <span className="pro-nav-title">{activeTab === 'curriculum' ? (item.title || item.Title) : item.docName}</span>
                                        <span className="pro-nav-meta">{activeTab === 'curriculum' ? `VIDEO • ${item.duration || 'ENCRYPTED'}` : 'SECURE PDF'}</span>
                                    </div>
                                </div>
                            );
                        })}
                    </nav>
                </aside>
            </main>

            <style>{`
                .lms-studio-pro { height: 100%; width: 100%; display: flex; flex-direction: column; background: #fff; font-family: 'Outfit', sans-serif; overflow: hidden; }
                
                /* Precision Navigation Bar */
                .studio-top-nav { height: 80px; padding: 0 40px; display: flex; align-items: center; justify-content: space-between; border-bottom: 1px solid #f2f2f2; z-index: 1000; flex-shrink: 0; }
                .nav-group-left, .nav-group-right { display: flex; align-items: center; gap: 20px; }
                .v-divider { width: 1px; height: 32px; background: #eee; }
                
                .over-text { font-size: 9px; font-weight: 950; color: #bbb; letter-spacing: 1.5px; margin-bottom: 2px; display: block; }
                .main-title-text { font-size: 20px; font-weight: 900; color: #1a1a1a; margin: 0; letter-spacing: -0.5px; }

                /* Circular Precision Buttons */
                .pro-circular-btn { 
                    width: 42px; height: 42px; border-radius: 50%; border: 1px solid #eee; background: #fff; 
                    display: flex; align-items: center; justify-content: center; cursor: pointer; transition: 0.2s cubic-bezier(0.4, 0, 0.2, 1);
                    color: #999;
                }
                .pro-circular-btn:hover { border-color: var(--color-primary); color: var(--color-primary); transform: translateY(-2px); box-shadow: 0 8px 16px rgba(0,0,0,0.05); }
                .pro-circular-btn.active { background: var(--color-primary); color: #fff; border-color: var(--color-primary); }

                /* Elite Tab System */
                .elite-tab-system { background: #f5f5f5; border-radius: 100px; padding: 4px; display: flex; border: 1px solid #eee; }
                .elite-tab { 
                    padding: 8px 20px; border-radius: 100px; border: none; background: none; 
                    font-size: 13px; font-weight: 850; color: #888; cursor: pointer; 
                    display: flex; align-items: center; gap: 8px; transition: 0.2s;
                }
                .elite-tab.active { background: #fff; color: var(--color-primary); box-shadow: 0 4px 10px rgba(0,0,0,0.04); border: 1px solid #eee; }
                .elite-tab:hover:not(.active) { color: #333; }

                .lms-studio-workspace { flex: 1; display: grid; grid-template-columns: 1fr 380px; transition: 0.4s cubic-bezier(0.19, 1, 0.22, 1); min-height: 0; }
                .lms-studio-workspace.focus-mode { grid-template-columns: 1fr 0px; }
                
                .lms-studio-content-flow { overflow-y: auto; background: #fafafa; padding: 40px; display: flex; flex-direction: column; gap: 40px; }
                
                .studio-theatre-stage { background: #000; border-radius: 28px; overflow: hidden; position: relative; box-shadow: 0 40px 100px -30px rgba(0,0,0,0.3); height: calc(100vh - 420px); min-height: 520px; }

                .cinema-media-viewport { width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; background: #000; position: relative; }
                .cinema-media-element { max-width: 100%; max-height: 100%; width: auto !important; height: auto !important; border: none; object-fit: contain; }
                .cinema-media-viewport.video .cinema-media-element { width: 100% !important; height: 100% !important; }

                .video-fs-btn { position: absolute; bottom: 20px; right: 20px; width: 44px; height: 44px; background: rgba(255,255,255,0.9); border-radius: 12px; border: none; display: flex; align-items: center; justify-content: center; cursor: pointer; box-shadow: 0 8px 16px rgba(0,0,0,0.15); z-index: 200; }

                .pro-lesson-details { max-width: 1000px; padding-bottom: 60px; }
                .lesson-badge { font-size: 11px; font-weight: 950; color: var(--color-primary); letter-spacing: 1px; }
                .lesson-details-title { font-size: 30px; font-weight: 900; margin: 10px 0 16px; color: #111; letter-spacing: -1px; }
                .pill { font-size: 11px; font-weight: 900; color: #888; background: #eee; padding: 4px 14px; border-radius: 100px; display: flex; align-items: center; gap: 8px; }
                .lesson-details-body p { font-size: 17px; line-height: 1.8; color: #444; margin-top: 24px; white-space: pre-line; }

                .lms-studio-sidebar { background: #fff; border-left: 1px solid #eee; display: flex; flex-direction: column; overflow: hidden; }
                .sidebar-header-pro { padding: 36px 32px 20px; display: flex; justify-content: space-between; font-size: 11px; font-weight: 950; color: #bbb; letter-spacing: 0.5px; }
                .pro-sidebar-list { flex: 1; overflow-y: auto; padding: 12px; }
                .pro-nav-row { display: flex; align-items: center; gap: 14px; padding: 16px 20px; border-radius: 16px; cursor: pointer; margin-bottom: 4px; transition: 0.2s; border: 1px solid transparent; }
                .pro-nav-row.active { background: var(--color-primary-soft); border-color: rgba(var(--color-primary-rgb), 0.15); }
                .pro-nav-icon { width: 36px; height: 36px; background: #fff; border-radius: 10px; display: flex; align-items: center; justify-content: center; box-shadow: 0 4px 10px rgba(0,0,0,0.02); color: #666; }
                .active .pro-nav-icon { background: var(--color-primary); color: #fff; }

                .loading-spinner { width: 32px; height: 32px; border: 3px solid rgba(0,0,0,0.1); border-top-color: var(--color-primary); border-radius: 50%; animation: spin 0.8s linear infinite; }
                @keyframes spin { to { transform: rotate(360deg); } }

                /* Mobile & Tablet Adaptation */
                @media (max-width: 1024px) {
                    .lms-studio-workspace { grid-template-columns: 1fr; }
                    .lms-studio-sidebar { border-left: none; border-top: 1px solid #eee; height: 500px; }
                    .studio-theatre-stage { height: auto; aspect-ratio: 16/9; min-height: unset; border-radius: 12px; }
                    .lms-studio-content-flow { padding: 20px; gap: 20px; }
                    .studio-top-nav { padding: 0 20px; height: 70px; }
                    .main-title-text { font-size: 16px; }
                    .pro-circular-btn { width: 36px; height: 36px; }
                    .elite-tab { padding: 6px 12px; font-size: 12px; }
                    .v-divider { height: 24px; }
                }

                @media (max-width: 640px) {
                    .brand-meta { display: none; }
                    .nav-group-left { gap: 10px; }
                    .studio-top-nav { height: 60px; }
                    .pro-lesson-details { padding: 0 10px 40px; }
                    .lesson-details-title { font-size: 24px; }
                    .lesson-details-body p { font-size: 15px; }
                    .video-fs-btn { width: 36px; height: 36px; bottom: 10px; right: 10px; }
                }
            `}</style>
        </div>
    );
};

export default StudentCoursePlayer;
