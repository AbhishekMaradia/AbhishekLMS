import React from 'react';
import { Icons } from '../../../shared/components/lms/Icons';
import '../Group.css';

interface GroupUserModalProps {
    guModal: any;
    setGuModal: (val: any) => void;
    toggleAllVisibleUsers: (select: boolean) => void;
    toggleUserInGroup: (userId: number) => void;
    saveGroupUsers: () => Promise<void>;
}

export const GroupUserModal: React.FC<GroupUserModalProps> = ({
    guModal, setGuModal,
    toggleAllVisibleUsers, toggleUserInGroup, saveGroupUsers
}) => {
    if (!guModal) return null;

    const searchQ = (guModal.search || "").toLowerCase();
    const masterList = (guModal.users || []).filter((u: any) => {
        const name = String(u.firstName || "") + " " + String(u.lastName || "");
        const email = String(u.email || "");
        return name.toLowerCase().includes(searchQ) || email.toLowerCase().includes(searchQ);
    });

    const assignedCount = (guModal.users || []).filter((u: any) => u.isAssigned).length;

    return (
        <div className="lms-modal-overlay lms-modal-overlay-smooth lms-gu-overlay" onClick={() => setGuModal(null)}>
            <div 
                className="lms-modal-content lms-slide-up lms-gu-content" 
                onClick={e => e.stopPropagation()}
            >
                {/* Header */}
                <div className="lms-gu-header">
                    <div className="lms-gu-header-left">
                        <div className="lms-gu-icon-box">
                            <Icons.User s={22} />
                        </div>
                        <div>
                            <h2 className="lms-gu-title">Group Enrollment</h2>
                            <div className="lms-gu-subtitle">{guModal.groupName}</div>
                        </div>
                    </div>
                    <button onClick={() => setGuModal(null)} className="lms-gu-close-btn">
                        <Icons.Close s={24} />
                    </button>
                </div>

                {/* Body */}
                <div className="lms-gu-body">
                    <div className="lms-gu-sidebar">
                        <div className="lms-gu-stats-box">
                            <div className="lms-gu-stats-label">ASSIGNED</div>
                            <div className="lms-gu-stats-val">{assignedCount}</div>
                            <div className="lms-gu-stats-sub">MEMBERS</div>
                        </div>
                        <div className="lms-gu-sidebar-actions">
                            <button className="lms-btn lms-btn-ghost lms-gu-sidebar-btn" onClick={() => toggleAllVisibleUsers(true)}>Select All</button>
                            <button className="lms-btn lms-btn-ghost lms-gu-sidebar-btn" onClick={() => toggleAllVisibleUsers(false)}>Clear All</button>
                        </div>
                    </div>
                    <div className="lms-gu-main">
                        <div className="lms-gu-search-wrap">
                            <div className="lms-gu-search-inner">
                                <Icons.Search className="lms-gu-search-icon" s={18} />
                                <input placeholder="Search members..." value={guModal.search} onChange={e => setGuModal({ ...guModal, search: e.target.value })}
                                    className="lms-gu-search-input"
                                />
                            </div>
                        </div>
                        <div className="lms-custom-scrollbar lms-gu-grid-wrap">
                            <div className="lms-gu-grid">
                                {guModal.loading ? (
                                    Array.from({ length: 8 }).map((_, i) => (
                                        <div key={i} className="lms-skeleton-pulse lms-gu-skeleton" />
                                    ))
                                ) : masterList.length === 0 ? (
                                    <div className="lms-gu-empty">
                                        <Icons.Alert s={32} className="lms-gu-empty-icon" />
                                        <div className="lms-gu-empty-text">No matching members</div>
                                    </div>
                                ) : (
                                    masterList.map((u: any) => {
                                        const uId = u.userId || u.id || u.Id;
                                        return (
                                            <div key={uId} onClick={() => toggleUserInGroup(uId)} className={`lms-gu-card ${u.isAssigned ? 'active' : ''}`}>
                                                <div className={`lms-gu-checkbox ${u.isAssigned ? 'active' : ''}`}>
                                                    {u.isAssigned && <Icons.Check s={14} />}
                                                </div>
                                                <div className="lms-gu-card-info">
                                                    <div className="lms-gu-card-name">{u.firstName} {u.lastName}</div>
                                                    <div className="lms-gu-card-email">{u.email}</div>
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
                <div className="lms-gu-footer">
                    <button onClick={() => setGuModal(null)} className="lms-gu-cancel">Cancel</button>
                    <button
                        className="lms-btn-commit lms-gu-commit"
                        style={{ opacity: guModal.saving ? 0.7 : 1 }}
                        onClick={saveGroupUsers}
                        disabled={guModal.saving}
                    >
                        {guModal.saving ? 'Saving...' : 'Save'}
                    </button>
                </div>
            </div>
        </div>
    );
};
