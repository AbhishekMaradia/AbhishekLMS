import React, { useState } from 'react';
import { Icons } from '../../../shared/components/lms/LmsComponents';
import { PeerListSkeleton } from '../../../shared/components/lms/LmsSkeleton';

/**
 * Minimalist Enterprise Peer Directory
 * Clean, reduced-noise interface focusing on essential identity data.
 */
export const StudentPeerList = ({ peers, currentUser, loading }: any) => {
    const [selectedPeer, setSelectedPeer] = useState<any>(null);

    const filteredPeers = peers.filter((p: any) => {
        const pid = p.id || p.Id || p.userId || p.UserId;
        const cid = currentUser.id || currentUser.Id || currentUser.userId || currentUser.UserId;
        return Number(pid) !== Number(cid);
    });

    if (loading) {
        return (
            <div className="lms-peers-minimal lms-fade-in">
                <header className="minimal-header">
                    <div className="lms-skeleton-pulse" style={{ width: '180px', height: '28px', borderRadius: '4px' }} />
                </header>
                <div style={{ marginTop: '20px' }}>
                    <PeerListSkeleton />
                </div>
            </div>
        );
    }

    return (
        <div className={`lms-peers-minimal lms-fade-in ${selectedPeer ? 'pane-active' : ''}`}>
            {/* Minimal Header */}
            <header className="minimal-header">
                <div>
                    <h1 className="minimal-title">Network</h1>
                    <p className="minimal-subtitle">{filteredPeers.length} Members Online</p>
                </div>
            </header>

            <main className="minimal-layout">
                {/* List: Reduced friction */}
                <section className="minimal-stack lms-custom-scrollbar">
                    {filteredPeers.length > 0 ? (
                        filteredPeers.map((peer: any, idx: number) => {
                            const isSelected = selectedPeer?.id === (peer.id || peer.Id);
                            return (
                                <div
                                    key={peer.id || peer.Id || idx}
                                    className={`minimal-row ${isSelected ? 'active' : ''}`}
                                    onClick={() => setSelectedPeer(peer)}
                                >
                                    <div className="row-lead">
                                        <div className={`mini-avatar color-${(idx % 5) + 1}`}>
                                            {peer.firstName?.[0]}{peer.lastName?.[0] || ''}
                                        </div>
                                        <div className="row-text">
                                            <div className="peer-name">{peer.firstName} {peer.lastName}</div>
                                            <div className="peer-mail-sub">{peer.email}</div>
                                        </div>
                                    </div>
                                    <div className="row-tail">
                                        <div className="mini-status-dot" />
                                    </div>
                                </div>
                            );
                        })
                    ) : (
                        <div className="empty-quiet">No other members yet.</div>
                    )}
                </section>

                {/* Profile Pane: Essential Data Only */}
                <aside className={`minimal-pane ${selectedPeer ? 'visible' : ''}`}>
                    {selectedPeer ? (
                        <div className="pane-content lms-fade-in">
                            <button className="minimal-close-btn" onClick={() => setSelectedPeer(null)}>
                                <Icons.Close s={18} />
                            </button>

                            <div className="pane-hero">
                                <div className="large-avatar">
                                    {selectedPeer.firstName?.[0]}{selectedPeer.lastName?.[0] || ''}
                                </div>
                                <div className="online-pill">ONLINE</div>
                                <h1 className="profile-name-text">{selectedPeer.firstName} {selectedPeer.lastName}</h1>
                            </div>

                            <div className="pane-field-stack">
                                <div className="p-field">
                                    <label>EMAIL</label>
                                    <span>{selectedPeer.email}</span>
                                </div>
                                <div className="p-field">
                                    <label>COHORT</label>
                                    <span>Standard Professional</span>
                                </div>
                            </div>

                            <div className="pane-footer">
                                <button className="lms-btn lms-btn-primary full-width" onClick={() => window.location.href = `mailto:${selectedPeer.email}`}>
                                    <Icons.Mail s={16} /> CONTACT
                                </button>
                            </div>
                        </div>
                    ) : (
                        <div className="pane-idle">Select a profile to view details</div>
                    )}
                </aside>
            </main>

            <style>{`
                .lms-peers-minimal { padding: 40px; height: calc(100vh - 120px); display: flex; flex-direction: column; font-family: 'Inter', sans-serif; }
                
                .minimal-header { margin-bottom: 32px; flex-shrink: 0; }
                .minimal-title { font-size: 32px; font-weight: 900; color: #111; letter-spacing: -1px; font-family: 'Outfit', sans-serif; }
                .minimal-subtitle { font-size: 13px; color: #888; font-weight: 500; margin-top: 4px; }

                .minimal-layout { flex: 1; display: grid; grid-template-columns: 1fr 0px; transition: 0.4s cubic-bezier(0.16, 1, 0.3, 1); min-height: 0; }
                .pane-active .minimal-layout { grid-template-columns: 1fr 420px; gap: 40px; }

                .minimal-stack { display: flex; flex-direction: column; gap: 4px; overflow-y: auto; padding-right: 12px; }
                
                .minimal-row { 
                    padding: 14px 20px; border-radius: 16px; border: 1px solid transparent; 
                    display: flex; align-items: center; justify-content: space-between; cursor: pointer; transition: 0.2s; 
                }
                .minimal-row:hover { background: #f8f8f8; }
                .minimal-row.active { background: var(--color-primary-soft); border-color: rgba(var(--color-primary-rgb), 0.1); }

                .row-lead { display: flex; align-items: center; gap: 16px; }
                .mini-avatar { 
                    width: 40px; height: 40px; border-radius: 12px; display: flex; align-items: center; justify-content: center; 
                    font-size: 14px; font-weight: 800; color: #fff; text-transform: uppercase;
                }
                .peer-name { font-size: 15px; font-weight: 700; color: #1a1a1a; }
                .peer-mail-sub { font-size: 12px; color: #aaa; }

                .mini-status-dot { width: 8px; height: 8px; border-radius: 50%; background: #10b981; opacity: 0.6; }

                /* Pane Styling */
                .minimal-pane { 
                    background: #fff; border-radius: 32px; border: 1px solid #f2f2f2; 
                    position: relative; box-shadow: 0 40px 100px -20px rgba(0,0,0,0.06); 
                    opacity: 0; transform: translateX(20px); pointer-events: none; transition: 0.3s;
                }
                .minimal-pane.visible { opacity: 1; transform: translateX(0); pointer-events: all; }

                .pane-content { padding: 48px; height: 100%; display: flex; flex-direction: column; }
                
                .minimal-close-btn { 
                    position: absolute; top: 24px; right: 24px; width: 36px; height: 36px; border-radius: 50%; 
                    border: none; background: #f8f8f8; cursor: pointer; color: #777; 
                    display: flex; align-items: center; justify-content: center; transition: 0.2s;
                }
                .minimal-close-btn:hover { background: #eee; color: #000; }

                @media (max-width: 1024px) {
                    .pane-active .minimal-layout { grid-template-columns: 1fr 320px; gap: 20px; }
                    .pane-content { padding: 32px; }
                }

                @media (max-width: 768px) {
                    .lms-peers-minimal { padding: 20px; }
                    .pane-active .minimal-layout { grid-template-columns: 1fr; }
                    .minimal-pane { position: fixed; top: 0; left: 0; width: 100%; height: 100%; z-index: 100; border-radius: 0; }
                    .minimal-pane:not(.visible) { display: none; }
                }

                .pane-hero { text-align: center; margin-bottom: 40px; }
                .large-avatar { 
                    width: 100px; height: 100px; border-radius: 36px; background: var(--accent-gradient); 
                    margin: 0 auto 20px; display: flex; align-items: center; justify-content: center; 
                    font-size: 36px; font-weight: 900; color: #fff; box-shadow: 0 20px 40px rgba(var(--color-primary-rgb), 0.25);
                }
                .online-pill { font-size: 9px; font-weight: 950; color: #10b981; background: #ecfdf5; padding: 4px 12px; border-radius: 100px; display: inline-block; letter-spacing: 0.5px; }
                .profile-name-text { font-size: 28px; font-weight: 900; color: #111; margin: 12px 0 0; letter-spacing: -1px; font-family: 'Outfit', sans-serif; }

                .pane-field-stack { display: flex; flex-direction: column; gap: 24px; margin-bottom: 40px; }
                .p-field label { font-size: 9px; font-weight: 900; color: #ccc; letter-spacing: 1px; display: block; margin-bottom: 6px; }
                .p-field span { font-size: 16px; font-weight: 600; color: #333; }

                .pane-footer { margin-top: auto; }
                .pane-idle { height: 100%; display: flex; align-items: center; justify-content: center; color: #ccc; font-weight: 500; font-size: 14px; }

                .color-1 { background: #FF6B6B; } .color-2 { background: #4ECDC4; }
                .color-3 { background: #45B7D1; } .color-4 { background: #96CEB4; }
                .color-5 { background: #FFEEAD; color: #888; }
            `}</style>
        </div>
    );
};
