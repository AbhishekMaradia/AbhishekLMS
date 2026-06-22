import React, { useEffect } from 'react';
import { toast } from 'react-toastify';
import './SecureMediaViewer.css';

interface MediaData {
    url: string;
    name: string;
    type: 'video' | 'doc' | 'img';
}

interface SecureMediaViewerProps {
    media: MediaData;
    onClose: () => void;
    isDarkMode?: boolean;
}

export const SecureMediaViewer: React.FC<SecureMediaViewerProps & { user: any, accentColor?: string }> = ({ media, onClose, user, isDarkMode = true, accentColor = '#6366f1' }) => {
    const ACCENT = accentColor;
    console.log('[SMV] rendering. media=', media, 'user=', user);

    useEffect(() => {
        let frameId: number;
        const checkSecurity = () => {
            const overlay = document.getElementById('secure-blur-overlay');
            const hasFocus = document.hasFocus();
            const isHidden = document.visibilityState === 'hidden';

            if (!hasFocus || isHidden) {
                if (overlay && overlay.style.display !== 'flex') {
                    overlay.style.display = 'flex';
                }
            } else {
                if (overlay && overlay.style.display === 'flex' && hasFocus) {
                    overlay.style.display = 'none';
                }
            }
            frameId = requestAnimationFrame(checkSecurity);
        };
        frameId = requestAnimationFrame(checkSecurity);

        const handleSecurity = (e: KeyboardEvent) => {
            const isS = e.key.toLowerCase() === 's';
            const isP = e.key.toLowerCase() === 'p';
            const isU = e.key.toLowerCase() === 'u';
            const isPrtScr = e.key === 'PrintScreen';

            if ((e.metaKey && e.shiftKey && isS) || isPrtScr) {
                e.preventDefault();
                onClose();
                toast.error('SECURITY ALERT: System-level capture attempt detected.');
            }

            if ((e.ctrlKey || e.metaKey) && (isS || isP || isU)) {
                e.preventDefault();
                onClose();
                toast.error('SECURITY: Shortcuts disabled.');
            }

            if (e.key === 'F12' || (e.ctrlKey && e.shiftKey && e.key === 'I')) {
                e.preventDefault();
                onClose();
            }
        };

        const handleBeforePrint = () => {
            document.body.style.display = 'none';
            setTimeout(() => { document.body.style.display = 'block'; }, 500);
            onClose();
        };

        const handleContextMenu = (e: MouseEvent) => e.preventDefault();
        const handleCopy = (e: ClipboardEvent) => e.preventDefault();

        window.addEventListener('keydown', handleSecurity);
        window.addEventListener('beforeprint', handleBeforePrint);
        window.addEventListener('contextmenu', handleContextMenu);
        window.addEventListener('copy', handleCopy);

        return () => {
            cancelAnimationFrame(frameId);
            window.removeEventListener('keydown', handleSecurity);
            window.removeEventListener('beforeprint', handleBeforePrint);
            window.removeEventListener('contextmenu', handleContextMenu);
            window.removeEventListener('copy', handleCopy);
        };
    }, [onClose]);

    return (
        <div className="lms-secure-media-root">
            <div className={`lms-secure-media-container ${isDarkMode ? 'dark' : 'light'}`}>
                {/* HEADER */}
                <div className="lms-secure-media-header">
                    <div className="lms-secure-media-header-left">
                        <div className="lms-secure-media-badge" style={{ background: ACCENT }}>
                            {media.type === 'video' ? 'VIDEO_STREAM' : 'DOCUMENT_VIEW'}
                        </div>
                        <h3 className="lms-secure-media-title">{media.name}</h3>
                    </div>
                    <div className="lms-secure-media-header-right">
                        <button
                            onClick={onClose}
                            className="lms-secure-media-close"
                            title="Close Stream"
                        >×</button>
                    </div>
                </div>

                {/* CONTENT AREA */}
                <div className="lms-secure-media-content-shell">

                    {/* SECURE BLUR OVERLAY (Snapshot Prevention) */}
                    <div id="secure-blur-overlay" style={{ display: 'none' }}>
                        <div className="lms-secure-media-blur-icon">
                            <span>🔐</span>
                        </div>
                        <h2 className="lms-secure-media-blur-title">PROTECTED ENVIRONMENT</h2>
                        <p className="lms-secure-media-blur-desc">Safe Mode is active. Focus detection has paused playback to prevent unauthorized capture. Please refocus to continue.</p>
                    </div>

                    {/* WATERMARK */}
                    <div className="lms-secure-media-watermark">
                        <div className="lms-watermark-segment">{user?.email || 'Confidential'}</div>
                        <div className="lms-watermark-segment">{new Date().toLocaleDateString()}</div>
                        <div className="lms-watermark-segment">PROPERTY OF SOULCODE</div>
                    </div>

                    {media.type === 'video' ? (
                        <video
                            src={media.url}
                            controls
                            autoPlay
                            playsInline
                            controlsList="nodownload"
                            disablePictureInPicture
                            onContextMenu={e => e.preventDefault()}
                            className="lms-secure-media-video"
                        >
                            Your gadget does not support video streaming.
                        </video>
                    ) : media.type === 'img' ? (
                        <div className="lms-secure-media-doc-shell" style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', background: '#000' }}>
                            <img
                                src={media.url}
                                alt={media.name}
                                className="lms-secure-media-img"
                                style={{ maxWidth: '100%', maxHeight: '100%', objectFit: 'contain', userSelect: 'none' }}
                                onContextMenu={e => e.preventDefault()}
                                draggable={false}
                            />
                            <div className="lms-secure-media-shield" onContextMenu={e => e.preventDefault()} />
                        </div>
                    ) : (
                        <div className="lms-secure-media-doc-shell">
                            {/* object tag: browser uses Content-Type from server for rendering */}
                            <object
                                data={`${media.url}#toolbar=0&navpanes=0&view=FitH`}
                                type="application/pdf"
                                className="lms-secure-media-object"
                            >
                                <div className="lms-error-fallback">
                                    <p>Your browser cannot render this document securely within the frame.</p>
                                    <a href={media.url} target="_blank" rel="noopener noreferrer" className="lms-premium-btn small">
                                        Launch in Secure Tab
                                    </a>
                                </div>
                            </object>
                            <div className="lms-secure-media-shield" onContextMenu={e => e.preventDefault()} />
                        </div>
                    )}


                </div>
            </div>
        </div>
    );
};
