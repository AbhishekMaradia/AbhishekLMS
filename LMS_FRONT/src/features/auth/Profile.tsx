import React from 'react';
import { THEME, STYLES } from '../../shared/components/lms/theme';
import { Icons } from '../../shared/components/lms/Icons';
import { userApi } from './api/userApi';
import { toast } from 'react-toastify';
import './Auth.css';

export const Profile = ({ user, setUser, ui, setUi, setTab, extractData }: any) => {
    return (
        <div className="lms-section lms-profile-root">
            <form onSubmit={async (e: React.FormEvent<HTMLFormElement>) => {
                e.preventDefault();
                setUi({ ...ui, loading: true });
                try {
                    const formData = new FormData(e.currentTarget);
                    const data = Object.fromEntries(formData.entries());
                    // If password is empty, don't send it
                    if (!data.Password) delete (data as any).Password;
                    // FORCE IsActive to true for profile updates (prevents auto deactivation)
                    (data as any).IsActive = true;

                    const res = await userApi.update(user.id || (user as any).Id, data as any);
                    const updated = Array.isArray(extractData(res)) ? extractData(res) : [extractData(res)];
                    const newUser = { ...user, ...(updated[0] || updated) };
                    setUser(newUser);
                    localStorage.setItem('user', JSON.stringify(newUser));
                    toast.success("Profile synchronized successfully.");
                } catch (e: any) {
                    toast.error(e.message || "Update failed");
                } finally {
                    setUi({ ...ui, loading: false });
                }
            }}>
                <div className="premium-card lms-profile-card">
                    {/* Header Banner */}
                    <div className="lms-profile-banner">
                        <div className="lms-profile-avatar">
                            {user.firstName?.[0]}{user.lastName?.[0]}
                        </div>
                        <div className="lms-profile-info">
                            <h2 className="lms-profile-name">{user.firstName} {user.lastName}</h2>
                            <div className="lms-profile-meta-row">
                                <span className="lms-profile-role">{user.userRole?.replace(/_/g, ' ')}</span>
                                <div className="lms-profile-dot" />
                                <span className="lms-profile-id">#{user.id}</span>
                            </div>
                        </div>
                        <div className="lms-profile-actions">
                            <button
                                type="button"
                                onClick={() => setTab('dash')}
                                className="lms-profile-btn-close"
                                title="Close Profile"
                            >
                                <Icons.Close s={20} />
                            </button>
                            <button
                                type="submit"
                                disabled={ui.loading}
                                className="lms-profile-btn-save"
                            >
                                {ui.loading ? 'Saving...' : <><Icons.Check s={18} /> Save Changes</>}
                            </button>
                        </div>
                    </div>

                    <div className="lms-profile-body">
                        {/* Primary Fields */}
                        <div className="lms-profile-grid-main">
                            {[
                                { label: 'First Name', name: 'FirstName', value: user.firstName },
                                { label: 'Last Name', name: 'LastName', value: user.lastName },
                                { label: 'Email Address', name: 'Email', value: user.email },
                                { label: 'Mobile Number', name: 'Mobile', value: user.mobile || '' },
                            ].map((f, i) => (
                                <div key={i}>
                                    <label className="lms-profile-label">{f.label}</label>
                                    <input name={f.name} defaultValue={f.value} className="lms-input lms-profile-input" />
                                </div>
                            ))}
                            <div className="lms-profile-pass-wrap">
                                <label className="lms-profile-pass-label">Change Password (Optional)</label>
                                <div className="lms-profile-pass-inner">
                                    <input
                                        name="Password"
                                        type="password"
                                        placeholder="Enter new password to update..."
                                        className="lms-input lms-profile-pass-input"
                                    />
                                    <Icons.Lock className="lms-profile-pass-icon" s={16} />
                                </div>
                                <p className="lms-profile-pass-hint">Leave blank to keep your current password.</p>
                            </div>
                        </div>

                        {/* Account Meta */}
                        <div className="lms-profile-meta-grid">
                            {[
                                { label: 'Organization', value: user.orgName || 'Global' },
                                { label: 'Active Group', value: user.groupName || 'Primary' },
                                { label: 'Member Since', value: user.createdAt ? new Date(user.createdAt).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }) : 'N/A' },
                                { label: 'Role', value: user.userRole?.replace(/_/g, ' ') },
                                { label: 'ID', value: `#SC-${user.id}` },
                            ].map((m, i) => (
                                <div key={i} className="lms-profile-meta-card">
                                    <div className="lms-profile-meta-label">{m.label}</div>
                                    <div className="lms-profile-meta-value">{m.value}</div>
                                </div>
                            ))}
                        </div>
                    </div>
                </div>
            </form>
        </div>
    );
};
