import React, { useState, useEffect } from 'react';
import { Icons } from '../../../shared/components/lms/Icons';
import { organizationApi } from '../api/organizationApi';
import { toast } from 'react-toastify';
import './Organization.css';

interface OrganizationRegisterProps {
    onBack: () => void;
    onSuccess: () => void;
}

export const OrganizationRegister: React.FC<OrganizationRegisterProps> = ({ onBack, onSuccess }) => {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setLoading(true);
        setError(null);

        const formData = new FormData(e.currentTarget);

        try {
            const res = await organizationApi.register(formData);
            if (res.data.success) {
                toast.success('Organization registered successfully! You can now log in.');
                onSuccess();
            } else {
                setError(res.data.message || 'Registration failed.');
                toast.error(res.data.message || 'Registration failed.');
            }
        } catch (err: any) {
            const msg = err.response?.data?.message || err.message || 'Synchronization failed.';
            setError(msg);
            toast.error(msg);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="lms-auth-simple-group lms-fade-in">
            <div className="lms-auth-simple-meta" style={{ marginBottom: '16px' }}>
                <button onClick={onBack} className="lms-auth-simple-footer-btn" style={{ fontSize: '12px' }}>
                    <Icons.Prev s={14} /> Back to Login
                </button>
            </div>

            <form onSubmit={handleSubmit} className="lms-auth-simple-group">
                <div className="lms-auth-simple-field">
                    <label>Organization Name <span style={{ color: 'var(--color-primary)' }}>*</span></label>
                    <input name="OrgName" required placeholder="Organization Name" className="lms-auth-simple-input" />
                </div>

                <div className="lms-form-row-2col">
                    <div className="lms-auth-simple-field">
                        <label>Org Code <span style={{ color: 'var(--color-primary)' }}>*</span></label>
                        <input name="OrgCode" required placeholder="Org Code" className="lms-auth-simple-input" />
                    </div>
                    <div className="lms-auth-simple-field">
                        <label>Website</label>
                        <input name="Website" placeholder="Website" className="lms-auth-simple-input" />
                    </div>
                </div>

                <div className="lms-auth-simple-field">
                    <label>Organization Logo</label>
                    <input type="file" name="Logo" accept="image/*" className="lms-auth-simple-input" style={{ padding: '8px' }} />
                </div>

                <div style={{ margin: '8px 0', borderTop: '1px solid var(--color-border)', paddingTop: '16px' }}>
                    <h4 style={{ fontSize: '11px', fontWeight: '800', color: 'var(--color-primary)', textTransform: 'uppercase', marginBottom: '12px', letterSpacing: '1px' }}>Admin Details</h4>
                </div>

                <div className="lms-form-row-2col">
                    <div className="lms-auth-simple-field">
                        <label>First Name <span style={{ color: 'var(--color-primary)' }}>*</span></label>
                        <input name="FirstName" required placeholder="First Name" className="lms-auth-simple-input" />
                    </div>
                    <div className="lms-auth-simple-field">
                        <label>Last Name <span style={{ color: 'var(--color-primary)' }}>*</span></label>
                        <input name="LastName" required placeholder="Last Name" className="lms-auth-simple-input" />
                    </div>
                </div>

                <div className="lms-auth-simple-field">
                    <label>Admin Email <span style={{ color: 'var(--color-primary)' }}>*</span></label>
                    <input name="Email" type="email" required placeholder="Email Address" className="lms-auth-simple-input" />
                </div>

                <div className="lms-form-row-2col">
                    <div className="lms-auth-simple-field">
                        <label>Mobile <span style={{ color: 'var(--color-primary)' }}>*</span></label>
                        <input name="Mobile" required placeholder="Mobile" className="lms-auth-simple-input" />
                    </div>
                    <div className="lms-auth-simple-field">
                        <label>Password <span style={{ color: 'var(--color-primary)' }}>*</span></label>
                        <input name="Password" type="password" required placeholder="Password" className="lms-auth-simple-input" />
                    </div>
                </div>

                <input type="hidden" name="PrimaryColor" value="#763121" />
                <input type="hidden" name="SecondaryColor" value="#4a2118" />



                {error && (
                    <div className="lms-auth-error-box" style={{ marginTop: '12px' }}>
                        {error}
                    </div>
                )}

                <button type="submit" disabled={loading} className="lms-auth-simple-btn" style={{ marginTop: '12px' }}>
                    {loading ? 'Provisioning...' : 'Provision Organization'}
                </button>
            </form>
        </div>
    );
};


