import React from 'react';
import { Icons } from '../../../shared/components/lms/Icons';
import { toast } from 'react-toastify';
import { organizationApi } from '../api/organizationApi';
import './Organization.css';

interface OrganizationModalProps {
    ui: any;
    user: any;
    isSuperAdmin: boolean;
    db: any;
    orgEditTab: 'org' | 'admin';
    setOrgEditTab: React.Dispatch<React.SetStateAction<'org' | 'admin'>>;
    orgAdminData: any;
    handleCrud: (action: string, entity: string, data: any) => Promise<void>;
    fetchOrgAdmin: (org: any) => void;
}

export const OrganizationModal: React.FC<OrganizationModalProps> = ({
    ui,
    isSuperAdmin,
    orgEditTab,
    setOrgEditTab,
    orgAdminData,
    handleCrud,
    fetchOrgAdmin
}) => {
    const initialExpiry = ui.target?.linkExpiredAt || ui.target?.LinkExpiredAt || '';
    const formatDateTimeLocal = (dateStr: string) => {
        if (!dateStr) return '';
        try {
            const d = new Date(dateStr);
            if (isNaN(d.getTime())) return '';
            const pad = (n: number) => n.toString().padStart(2, '0');
            return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
        } catch {
            return '';
        }
    };
    const [expiry, setExpiry] = React.useState<string>(formatDateTimeLocal(initialExpiry)); // ISO string for datetime-local
    const [generatedLink, setGeneratedLink] = React.useState<string>('');

    React.useEffect(() => {
        const initialExpiry = ui.target?.linkExpiredAt || ui.target?.LinkExpiredAt || '';
        setExpiry(formatDateTimeLocal(initialExpiry));
        setGeneratedLink('');
    }, [ui.target]);
    const orgCode = ui.target?.orgCode || ui.target?.OrgCode || ui.target?.code || ui.target?.Code;
    const orgId = ui.target?.id || ui.target?.Id || ui.target?.tenantId || ui.target?.tid;
    const regToken = ui.target?.registrationToken || ui.target?.RegistrationToken;
    const defaultLink = regToken
        ? `${window.location.origin}/register?token=${encodeURIComponent(regToken)}`
        : orgCode
            ? `${window.location.origin}/register/${orgCode}`
            : orgId
                ? `${window.location.origin}/register?tenantId=${orgId}`
                : '';

    // Function to generate registration link via API (saves expiry into DB)
    const generateLink = async () => {
        const targetCode = orgCode || regToken;
        if (!targetCode && isSuperAdmin) {
            toast.error('Organization code not available to generate link');
            return;
        }

        try {
            const expiryParam = expiry ? new Date(expiry).toISOString() : undefined;
            const res = await organizationApi.generateLink(targetCode || undefined, expiryParam);
            if (res.data.success && res.data.data) {
                setGeneratedLink(res.data.data.url);
                toast.success('Registration link generated!');
            } else {
                toast.error(res.data.message || 'Failed to generate link');
            }
        } catch (err: any) {
            toast.error(err.message || 'An error occurred during link generation');
        }
    };

    const [pColor, setPColor] = React.useState(ui.target?.primaryColor || ui.target?.PrimaryColor || '#763121');
    const [sColor, setSColor] = React.useState(ui.target?.secondaryColor || ui.target?.SecondaryColor || '#4a2118');
    const [preview, setPreview] = React.useState<string | null>(null);

    if (ui.modal !== 'org_create' && ui.modal !== 'org_edit') return null;

    const isEdit = ui.modal === 'org_edit';
    const registrationLink = generatedLink || defaultLink;

    return (
        <form
            key={ui.target?.tid || ui.target?.tenantId || ui.target?.id || ui.target?.Id || 'new'}
            onSubmit={(e) => {
                e.preventDefault();
                const fd = new FormData(e.currentTarget);

                // Ensure IsActive is set correctly
                const isActiveCheckbox = e.currentTarget.elements.namedItem('IsActive') as HTMLInputElement;
                if (isActiveCheckbox) {
                    fd.set('IsActive', isActiveCheckbox.checked ? 'true' : 'false');
                } else if (!isEdit) {
                    fd.set('IsActive', 'true'); // Default for new orgs
                }

                // Clean up empty fields that might cause 400 errors
                const keysToRemove: string[] = [];
                fd.forEach((value, key) => {
                    if (value === '' || (value instanceof File && value.size === 0)) {
                        // Don't remove required fields, only optional assets
                        if (['Website', 'Logo', 'PrimaryColor', 'SecondaryColor'].includes(key)) {
                            keysToRemove.push(key);
                        }
                    }
                });
                keysToRemove.forEach(k => fd.delete(k));

                handleCrud(isEdit ? 'update' : 'create', 'org', fd);
            }} className="lms-fade-in lms-col-gap-md" autoComplete="off">
            {/* Decoy fields to catch aggressive browser autofill */}
            <div style={{ position: 'absolute', top: '-9999px', left: '-9999px', width: '1px', height: '1px', opacity: 0.01, overflow: 'hidden', pointerEvents: 'none' }}>
                <input type="text" name="fake_username_autofill" tabIndex={-1} autoComplete="off" style={{ width: '1px', height: '1px' }} />
                <input type="email" name="fake_email_autofill" tabIndex={-1} autoComplete="off" style={{ width: '1px', height: '1px' }} />
                <input type="password" name="fake_password_autofill" tabIndex={-1} autoComplete="new-password" style={{ width: '1px', height: '1px' }} />
            </div>
            {isEdit && (
                <div className="lms-view-toggle lms-org-modal-view-toggle">
                    <button type="button" onClick={() => setOrgEditTab('org')} className={`lms-view-btn lms-org-modal-view-btn ${orgEditTab === 'org' ? 'active' : ''}`}>
                        General
                    </button>
                    <button type="button" onClick={() => { setOrgEditTab('admin'); fetchOrgAdmin(ui.target); }} className={`lms-view-btn lms-org-modal-view-btn ${orgEditTab === 'admin' ? 'active' : ''}`}>
                        Admin
                    </button>
                </div>
            )}

            {isEdit && <input type="hidden" name="Id" value={ui.target?.tid || ui.target?.tenantId || ui.target?.id || ui.target?.Id || 0} />}

            <div className={`lms-col-gap-md ${isEdit && orgEditTab === 'admin' ? 'lms-org-modal-hide' : 'lms-org-modal-flex'}`}>
                <label className="lms-label-premium required">Name</label>
                <div className="lms-modal-panel-premium">
                    <input name="OrgName" defaultValue={ui.target?.orgName || ui.target?.OrgName} placeholder="Name" className="lms-input-premium" required={!isEdit || orgEditTab === 'org'} />
                </div>

                <div className="lms-form-grid lms-org-modal-form-grid">
                    <div>
                        <label className="lms-label-premium">Website</label>
                        <div className="lms-modal-panel-premium">
                            <input name="Website" defaultValue={ui.target?.website || ui.target?.Website} placeholder="https://domain.com" className="lms-input-premium" />
                        </div>
                    </div>
                </div>

                <div className="lms-form-grid lms-org-modal-form-grid">
                    <div>
                        <label className="lms-label-premium">Theme Color</label>
                        <div className="lms-modal-panel-premium">
                            <div className="lms-color-picker-wrap lms-org-modal-color-wrap">
                                <input
                                    type="color"
                                    name="PrimaryColor"
                                    value={pColor}
                                    className="lms-color-input"
                                    onChange={(e) => setPColor(e.target.value)}
                                />
                                <input
                                    value={pColor}
                                    className="lms-input-premium lms-org-modal-color-input"
                                    onChange={(e) => setPColor(e.target.value)}
                                    placeholder="#000000"
                                />
                            </div>

                        </div>
                    </div>
                    <div>
                        <label className="lms-label-premium">Accent Color</label>
                        <div className="lms-modal-panel-premium">
                            <div className="lms-color-picker-wrap lms-org-modal-color-wrap">
                                <input
                                    type="color"
                                    name="SecondaryColor"
                                    value={sColor}
                                    className="lms-color-input"
                                    onChange={(e) => setSColor(e.target.value)}
                                />
                                <input
                                    value={sColor}
                                    className="lms-input-premium lms-org-modal-color-input"
                                    onChange={(e) => setSColor(e.target.value)}
                                    placeholder="#000000"
                                />
                            </div>

                        </div>
                    </div>
                </div>

                <label className="lms-label-premium">Logo</label>
                <div className="lms-modal-panel-dashed lms-org-modal-logo-wrap">
                    <div className="lms-org-modal-logo-inner">
                        <input
                            type="file"
                            name="Logo"
                            accept="image/png, image/jpeg, image/webp"
                            className="lms-input-premium lms-org-modal-logo-input"
                            onChange={(e) => {
                                const file = e.target.files?.[0];
                                if (file) setPreview(URL.createObjectURL(file));
                            }}
                        />
                        <p className="lms-status-sub lms-org-modal-logo-msg">PNG, JPG or WEBP (Max 2MB recommended)</p>
                    </div>
                    {preview && (
                        <div className="lms-org-modal-preview-box">
                            <img src={preview} alt="Preview" className="lms-org-modal-preview-img" />
                        </div>
                    )}
                </div>

                {ui.modal === 'org_create' && (
                    <div className="lms-col-gap-md lms-org-modal-admin-section">
                        <div className="lms-org-modal-section-title-wrap">
                            <h4 className="lms-form-section-title lms-org-modal-admin-title">ADMIN</h4>
                            <p className="lms-status-sub lms-org-modal-admin-msg">Primary admin user.</p>
                        </div>
                        <div className="lms-form-grid lms-org-modal-form-grid">
                            <div>
                                <label className="lms-label-premium required">First Name</label>
                                <div className="lms-modal-panel-premium">
                                    <input name="FirstName" placeholder="Admin First Name" className="lms-input-premium" required={ui.modal === 'org_create'} />
                                </div>
                            </div>
                            <div>
                                <label className="lms-label-premium required">Last Name</label>
                                <div className="lms-modal-panel-premium">
                                    <input name="LastName" placeholder="Admin Last Name" className="lms-input-premium" required={ui.modal === 'org_create'} />
                                </div>
                            </div>
                        </div>
                        <label className="lms-label-premium required">Email Address</label>
                        <div className="lms-modal-panel-premium">
                            <input name="Email" type="email" autoComplete="new-email" placeholder="admin@org.com" className="lms-input-premium" required={ui.modal === 'org_create'} />
                        </div>
                        <div className="lms-form-grid lms-org-modal-form-grid">
                            <div>
                                <label className="lms-label-premium required">Mobile</label>
                                <div className="lms-modal-panel-premium">
                                    <input name="Mobile" placeholder="+1..." className="lms-input-premium" required={ui.modal === 'org_create'} />
                                </div>
                            </div>
                            <div>
                                <label className="lms-label-premium required">Password</label>
                                <div className="lms-modal-panel-premium">
                                    <input name="Password" type="password" autoComplete="new-password" placeholder="••••••••" className="lms-input-premium" required={ui.modal === 'org_create'} />
                                </div>
                            </div>
                        </div>
                    </div>
                )}

                {isSuperAdmin && isEdit && (
                    <div className="lms-switch-premium">
                        <span className="lms-org-modal-status-label">Status</span>
                        <input
                            type="checkbox"
                            name="IsActive"
                            defaultChecked={ui.target ? (ui.target.isActive !== false && ui.target.IsActive !== false) : true}
                            className="lms-org-modal-status-check"
                        />
                    </div>
                )}

                {(isEdit || ui.modal === 'org_create') && (
                    <div className="lms-col-gap-md" style={{ marginTop: '16px', paddingTop: '16px', borderTop: '1px dashed var(--color-border)' }}>
                        <label className="lms-label-premium">Link Expiry (UTC)</label>
                        <input type="datetime-local" value={expiry} onChange={e => setExpiry(e.target.value)} className="lms-input-premium" />
                        <button type="button" onClick={generateLink} className="lms-btn lms-btn-primary" style={{ marginTop: '8px' }}>Generate Link</button>
                        {registrationLink && (
                            <>
                                <label className="lms-label-premium" style={{ marginTop: '8px' }}>Registration Link</label>
                                <div className="lms-flex-row" style={{ gap: '8px', width: '100%', alignItems: 'stretch' }}>
                                    <div className="lms-modal-panel-premium" style={{ flex: 1, display: 'flex', alignItems: 'center' }}>
                                        <Icons.Link s={16} style={{ marginRight: '8px', opacity: 0.6 }} />
                                        <input readOnly value={registrationLink} className="lms-input-premium" style={{ flex: 1, background: 'transparent', border: 'none', padding: 0, fontFamily: 'monospace', fontSize: '13px' }} />
                                    </div>
                                    <button type="button"
                                        onClick={() => {
                                            navigator.clipboard.writeText(registrationLink);
                                            toast.success('Registration link copied to clipboard!');
                                        }}
                                        className="lms-btn lms-btn-secondary"
                                        style={{ display: 'flex', alignItems: 'center', gap: '6px', whiteSpace: 'nowrap', padding: '0 16px' }}>
                                        <Icons.Copy s={16} /> Copy
                                    </button>
                                </div>
                            </>
                        )}
                    </div>
                )}
            </div>

            {isEdit && orgEditTab === 'admin' && (
                ui.loading ? (
                    <div className="lms-org-modal-loader-wrap">
                        <div className="lms-loader-spinner"></div>
                        <span className="lms-org-modal-loader-text">LOADING...</span>
                    </div>
                ) : (
                    <div className="lms-col-gap-md lms-org-modal-ro-wrap">
                        <div className="lms-form-grid lms-org-modal-form-grid">
                            <div>
                                <label className="lms-label-premium">First Name</label>
                                <div className="lms-modal-panel-premium">
                                    <input readOnly defaultValue={orgAdminData?.firstName || orgAdminData?.FirstName || ''} className="lms-input-premium lms-org-modal-ro-input" />
                                </div>
                            </div>
                            <div>
                                <label className="lms-label-premium">Last Name</label>
                                <div className="lms-modal-panel-premium">
                                    <input readOnly defaultValue={orgAdminData?.lastName || orgAdminData?.LastName || ''} className="lms-input-premium lms-org-modal-ro-input" />
                                </div>
                            </div>
                        </div>
                        <label className="lms-label-premium">Email Address</label>
                        <div className="lms-modal-panel-premium">
                            <input readOnly defaultValue={orgAdminData?.email || orgAdminData?.Email || ''} className="lms-input-premium lms-org-modal-ro-input" />
                        </div>
                        <label className="lms-label-premium">Mobile Number</label>
                        <div className="lms-modal-panel-premium">
                            <input readOnly defaultValue={orgAdminData?.mobile || orgAdminData?.Mobile || ''} className="lms-input-premium lms-org-modal-ro-input" />
                        </div>
                        <p className="lms-status-sub lms-org-modal-ro-msg">
                            Admin info is read-only. Edit in <span className="lms-org-modal-ro-bold">Users</span> if needed.
                        </p>
                    </div>
                )
            )}

            <button type="submit" disabled={ui.loading} className="lms-btn-commit">
                {ui.loading ? 'Saving...' : 'Save'}
            </button>
        </form>
    );
};
