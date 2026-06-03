import React, { useState, useEffect, useRef } from 'react';
import { Icons } from '../../../shared/components/lms/Icons';
import { apiClient as api, API_ORIGIN } from '../../../core/api/apiClient';
import { toast } from 'react-toastify';
import './StudentCoursePlayer.css';

interface StudentCoursePlayerProps {
    course: any;
    media: { vids: any[], docs: any[], loading: boolean };
    onClose: () => void;
    setPreviewMedia?: (m: any) => void;
}

const StudentCoursePlayer: React.FC<StudentCoursePlayerProps> = ({ course, media, onClose }) => {
    const [selectedVideo, setSelectedVideo] = useState<any>(null);
    const [selectedDoc, setSelectedDoc] = useState<any>(null);
    const [activeTab, setActiveTab] = useState<'videos' | 'docs'>('videos');
    const [videoUrl, setVideoUrl] = useState<string>('');
    const [videoLoading, setVideoLoading] = useState<boolean>(false);
    const videoRef = useRef<HTMLVideoElement>(null);
    const furthestTimeRef = useRef<number>(0);
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    const selectedVideoRef = useRef<any>(null);
    const [videoProgress, setVideoProgress] = useState<{ watchedPercentage: number, isCompleted: boolean } | null>(null);

    useEffect(() => {
        selectedVideoRef.current = selectedVideo;
    }, [selectedVideo]);

    const getAuthUrl = (isDoc = false) => {
        const token = localStorage.getItem('token');
        const sfx = token ? `?access_token=${token}` : '';
        let base = API_ORIGIN;
        if (!base.startsWith('http')) base = window.location.origin + (base.startsWith('/') ? '' : '/') + base;
        const origin = base.endsWith('/') ? base.slice(0, -1) : base;

        if (isDoc && selectedDoc) return `${origin}/CourseDocument/download/${selectedDoc.id || selectedDoc.Id}${sfx}#toolbar=0&navpanes=0&view=Fit`;
        if (selectedVideo) return `${origin}/CourseVideo/stream/${selectedVideo.id || selectedVideo.Id}${sfx}`;
        return '';
    };

    // Auto-select assets on load
    useEffect(() => {
        if (media.vids.length > 0 && !selectedVideo) setSelectedVideo(media.vids[0]);
        if (media.docs.length > 0 && !selectedDoc) setSelectedDoc(media.docs[0]);
    }, [media.vids, media.docs]);

    // SECURITY LOGIC
    useEffect(() => {
        let frameId: number;
        const checkSecurity = () => {
            const overlay = document.getElementById('secure-blur-overlay-student');
            const hasFocus = document.hasFocus();
            const isHidden = document.visibilityState === 'hidden';
            if (!hasFocus || isHidden) {
                if (overlay) overlay.classList.add('blur-visible');
            } else {
                if (overlay) overlay.classList.remove('blur-visible');
            }
            frameId = requestAnimationFrame(checkSecurity);
        };
        frameId = requestAnimationFrame(checkSecurity);

        const handleSecurity = (e: KeyboardEvent) => {
            if (e.key === 'PrintScreen' || (e.metaKey && e.shiftKey && e.key.toLowerCase() === 's')) {
                e.preventDefault();
                onClose();
                toast.error('SECURITY ALERT: System capture attempt blocked.');
            }
            if (e.key === 'F12' || (e.ctrlKey && e.shiftKey && e.key === 'I')) {
                e.preventDefault();
                onClose();
            }
        };
        window.addEventListener('keydown', handleSecurity);
        const handleContextMenu = (e: MouseEvent) => e.preventDefault();
        window.addEventListener('contextmenu', handleContextMenu);

        return () => {
            cancelAnimationFrame(frameId);
            window.removeEventListener('keydown', handleSecurity);
            window.removeEventListener('contextmenu', handleContextMenu);
        };
    }, [onClose]);

    const syncInterval = useRef<any>(null);

    const saveProgress = async (isCompleted = false) => {
        const currentVideo = selectedVideoRef.current;
        if (!currentVideo || !videoRef.current) return;

        const initialPercentage = videoProgress?.watchedPercentage || 0;
        const wasCompleted = videoProgress?.isCompleted || initialPercentage >= 95;
        
        // If already completed, do not send progress updates
        if (wasCompleted && !isCompleted) return;

        const v = videoRef.current;
        if (v.duration === 0 || isNaN(v.duration)) return;
        
        // Progress calculation based on furthest point watched, not current time
        const watched = (furthestTimeRef.current / v.duration) * 100;
        const finalPercentage = isCompleted ? 100 : Math.min(100, Math.max(0, Math.floor(watched)));

        // If the new percentage is not greater than the already saved progress, skip the API call
        if (finalPercentage <= initialPercentage && !isCompleted) return;

        try {
            await api.post('CourseVideo/update-progress', {
                videoId: currentVideo.id || currentVideo.Id,
                groupId: user.groupId,
                watchedPercentage: finalPercentage,
                isCompleted: isCompleted || finalPercentage >= 95
            });

            // Update local state in memory immediately
            setVideoProgress({
                watchedPercentage: finalPercentage,
                isCompleted: isCompleted || finalPercentage >= 95
            });
        } catch (e) { console.error("[LMS] Sync Error:", e); }
    };

    const selectedVideoId = selectedVideo?.id || selectedVideo?.Id || null;

    // Video Blob Downloader & Progress Loader - Only runs when the actual video ID changes!
    useEffect(() => {
        if (!selectedVideoId) {
            setVideoUrl('');
            setVideoLoading(false);
            setVideoProgress(null);
            return;
        }

        let active = true;
        setVideoLoading(true);
        setVideoUrl('');
        setVideoProgress(null);
        furthestTimeRef.current = 0;

        const downloadAndLoadProgress = async () => {
            try {
                // 1. Fetch user progress for this specific video
                const progressRes = await api.get(`CourseVideo/progress/${selectedVideoId}`).catch(() => ({ data: { data: [] } }));
                const progressData = progressRes.data?.data?.[0] || progressRes.data?.[0] || null;
                
                let initialPercentage = 0;
                let isComp = false;

                if (progressData) {
                    initialPercentage = progressData.watchedPercentage ?? progressData.WatchedPercentage ?? 0;
                    isComp = progressData.isCompleted ?? progressData.IsCompleted ?? false;
                }

                if (!active) return;
                setVideoProgress({ watchedPercentage: initialPercentage, isCompleted: isComp });

                // 2. Fetch the video stream as Blob
                const response = await fetch(getAuthUrl());
                if (!response.ok) throw new Error("Failed to load secure stream");
                
                const blob = await response.blob();
                if (!active) return;
                
                const localUrl = URL.createObjectURL(blob);
                setVideoUrl(localUrl);
                setVideoLoading(false);
            } catch (err) {
                console.error("[LMS] Video download error:", err);
                if (active) {
                    toast.error("Failed to load video stream.");
                    setVideoLoading(false);
                }
            }
        };

        downloadAndLoadProgress();

        return () => {
            active = false;
            // Revoke the old object URL to prevent memory leaks
            setVideoUrl(prev => {
                if (prev) {
                    URL.revokeObjectURL(prev);
                    console.log(`[LMS_PLAYER] Revoked video URL for video ID: ${selectedVideoId}`);
                }
                return '';
            });
        };
    }, [selectedVideoId]);

    // Periodic Progress Sync - Only runs/re-registers when video ID changes!
    useEffect(() => {
        if (!selectedVideoId) return;

        const interval = setInterval(() => saveProgress(), 15000);
        return () => {
            clearInterval(interval);
            saveProgress(); 
        };
    }, [selectedVideoId]);

    const isAlreadyCompleted = Boolean(
        videoProgress?.isCompleted || 
        (videoProgress?.watchedPercentage && videoProgress.watchedPercentage >= 95)
    );

    const handleLoadedMetadata = (e: React.SyntheticEvent<HTMLVideoElement, Event>) => {
        const v = e.currentTarget;
        const initialPercentage = videoProgress?.watchedPercentage || 0;
        furthestTimeRef.current = (initialPercentage / 100) * v.duration;
        console.log(`[LMS_PLAYER] Metadata Loaded. Duration: ${v.duration}s. Initial percentage: ${initialPercentage}%, Furthest time set to: ${furthestTimeRef.current}s`);
    };

    const handleTimeUpdate = (e: React.SyntheticEvent<HTMLVideoElement, Event>) => {
        const v = e.currentTarget;
        if (v.duration === 0 || isNaN(v.duration)) return;
        
        // Lazy initialize furthestTimeRef.current if needed
        if (furthestTimeRef.current === 0 && videoProgress) {
            const initialPercentage = videoProgress.watchedPercentage || 0;
            furthestTimeRef.current = (initialPercentage / 100) * v.duration;
            console.log(`[LMS_PLAYER] Lazy initialized furthestTimeRef to: ${furthestTimeRef.current}s (${initialPercentage}%)`);
        }

        // If already completed, let them seek freely
        if (isAlreadyCompleted) {
            furthestTimeRef.current = Math.max(furthestTimeRef.current, v.currentTime);
            return;
        }

        // Check if user is seeking forward beyond the furthest time watched (+ 2 seconds buffer)
        if (v.currentTime > furthestTimeRef.current + 2) {
            // Block seeking forward! Snap back to furthestTimeRef.current
            v.currentTime = furthestTimeRef.current;
            toast.warning("Seeking forward is disabled. Please watch the content sequentially.");
        } else {
            // Keep track of the furthest time reached
            furthestTimeRef.current = Math.max(furthestTimeRef.current, v.currentTime);
        }
    };

    // (getAuthUrl moved to top)

    return (
        <div className="student-player-overlay lms-fade-in">
            <div className="student-player-container">
                
                {/* PREMIUM HEADER */}
                <header className="student-player-header">
                    <div className="header-left-group">
                        <div className="player-status-badge">
                            {activeTab === 'videos' ? 'SECURE_STREAM' : 'SECURE_DOC'}
                        </div>
                        <h3 className="player-course-title">{course.title || course.Title}</h3>
                    </div>
                    
                    <div className="header-right-group">
                        <div className="tab-control-wrapper">
                            <button className={`lms-tab-mini ${activeTab === 'videos' ? 'active' : ''}`} onClick={() => setActiveTab('videos')}>Curriculum</button>
                            <button className={`lms-tab-mini ${activeTab === 'docs' ? 'active' : ''}`} onClick={() => setActiveTab('docs')}>Resources</button>
                        </div>
                        <button className="player-close-btn" onClick={onClose}>×</button>
                    </div>
                </header>

                {/* CINEMATIC WORKSPACE */}
                <div className="player-main-workspace">
                    
                    {/* THEATRE AREA */}
                    <div className="player-theatre">
                        
                        {/* SECURITY BLUR */}
                        <div id="secure-blur-overlay-student" className="player-security-blur">
                            <div className="blur-icon"><span>🔐</span></div>
                            <h2 className="blur-title">SECURE ENVIRONMENT</h2>
                            <p className="blur-text">Content protection is active. Please refocus the window to continue your learning session.</p>
                        </div>

                        {/* WATERMARK */}
                        <div className="lms-secure-media-watermark">
                            <div className="lms-watermark-segment">{user?.email}</div>
                            <div className="lms-watermark-segment">{new Date().toLocaleDateString()}</div>
                            <div className="lms-watermark-segment">SOULCODE PROPERTY</div>
                        </div>

                        {activeTab === 'docs' ? (
                            selectedDoc ? (
                                <iframe src={getAuthUrl(true)} className="player-media-content" title="Document Viewer" />
                            ) : <div className="empty-state">Select a resource file</div>
                        ) : videoLoading ? (
                            <div className="empty-state">
                                <div className="lms-loader-spinner"></div>
                                <div style={{ marginTop: 12 }}>DECRYPTING SECURE STREAM...</div>
                            </div>
                        ) : selectedVideo && videoUrl ? (
                            <video 
                                ref={videoRef} 
                                src={videoUrl} 
                                controls autoPlay 
                                className="player-media-content"
                                onEnded={() => { console.log('[LMS_PLAYER] Video Ended'); saveProgress(true); }} 
                                onPause={() => { console.log('[LMS_PLAYER] Video Paused'); saveProgress(); }}
                                onTimeUpdate={handleTimeUpdate}
                                onLoadedMetadata={handleLoadedMetadata}
                                onStalled={() => console.warn('[LMS_PLAYER] Video Stalled - Network or Buffer issue')}
                                onError={(e) => console.error('[LMS_PLAYER] Video Error:', e)}
                                onWaiting={() => console.log('[LMS_PLAYER] Video Waiting/Buffering...')}
                                onLoadedData={() => console.log('[LMS_PLAYER] Video Data Loaded')}
                            />
                        ) : <div className="empty-state">Select a module to begin</div>}
                    </div>

                    {/* SIDEBAR */}
                    <aside className="player-sidebar">
                        <div className="sidebar-header">
                            <div className="sidebar-label">
                                {activeTab === 'videos' ? 'COURSE CONTENT' : 'STUDY MATERIALS'}
                            </div>
                            <div className="sidebar-count">
                                {activeTab === 'videos' ? `${media.vids.length} Lessons` : `${media.docs.length} Resources`}
                            </div>
                        </div>
                        
                        <div className="sidebar-list lms-custom-scrollbar">
                            {(activeTab === 'videos' ? media.vids : media.docs).map((m, i) => {
                                const isSel = (m.id || m.Id) === (activeTab === 'videos' ? selectedVideo?.id || selectedVideo?.Id : selectedDoc?.id || selectedDoc?.Id);
                                return (
                                    <div 
                                        key={i} 
                                        onClick={() => activeTab === 'videos' ? setSelectedVideo(m) : setSelectedDoc(m)}
                                        className={`media-item ${isSel ? 'active' : ''}`}
                                    >
                                        <div className="item-icon-box">
                                            {activeTab === 'videos' ? <Icons.Play s={16} /> : <Icons.Doc s={16} />}
                                        </div>
                                        <div style={{ flex: 1, minWidth: 0 }}>
                                            <div className="item-title">
                                                {m.title || m.Title || m.docName}
                                            </div>
                                            <div className="item-meta">
                                                {activeTab === 'videos' ? 'MODULE ' + (i+1).toString().padStart(2, '0') : 'SECURE PDF'}
                                            </div>
                                        </div>
                                    </div>
                                );
                            })}
                        </div>
                    </aside>
                </div>
            </div>
        </div>
    );
};

export default StudentCoursePlayer;
