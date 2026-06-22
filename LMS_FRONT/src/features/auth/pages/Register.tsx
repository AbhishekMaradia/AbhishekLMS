import React, { useState } from 'react';
import { useNavigate, useLocation, useParams } from 'react-router-dom';
import { authApi } from '../api/authApi';
import { toast } from 'react-toastify';
import { Icons } from '../../../shared/components/lms/Icons';
import { APP_CONFIG } from '../../../config/app.config';
import '../Auth.css';

const Register = () => {
    const navigate = useNavigate();
    const location = useLocation();
    const { code: pathCode } = useParams();

    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [email, setEmail] = useState('');
    const [mobile, setMobile] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);
    const [showPassword, setShowPassword] = useState(false);

    // Resolve organization context from URL
    const getOrgContext = () => {
        const searchParams = new URLSearchParams(location.search);
        const token = searchParams.get('token') || '';
        const orgCode = searchParams.get('orgCode') || pathCode || '';
        const tenantId = searchParams.get('tenantId') ? Number(searchParams.get('tenantId')) : null;

        return { token, orgCode, tenantId };
    };

    const { token: registrationToken, orgCode: organizationCode, tenantId } = getOrgContext();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);

        if (!organizationCode && !tenantId && !registrationToken) {
            toast.error('Organization context not found. Please use your organization registration link.');
            setLoading(false);
            return;
        }

        const data: any = {
            firstName,
            lastName,
            email,
            mobile,
            password
        };

        if (organizationCode) {
            data.organizationCode = organizationCode.trim().toUpperCase();
        }
        if (tenantId) {
            data.tenantId = tenantId;
        }
        if (registrationToken) {
            data.registrationToken = registrationToken.trim().toUpperCase();
            data.organizationCode = registrationToken.trim().toUpperCase(); // For backend compatibility
        }

        try {
            const res = await authApi.register(data);
            if (res.data.success) {
                toast.success('Account Created Successfully! You can now log in.');
                navigate('/login');
            } else {
                toast.error(res.data.message || 'Registration failed.');
            }
        } catch (err: any) {
            toast.error(err.response?.data?.message || err.message || 'An error occurred during registration.');
        } finally {
            setLoading(false);
        }
    };

    const hasContext = !!(organizationCode || tenantId || registrationToken);

    return (
        <div className="lms-login-root">
            {/* Ambient Background Visuals */}
            <div className="lms-ambient">
                <div className="lms-ambient-blob a" />
                <div className="lms-ambient-blob b" />
                <div className="lms-ambient-grid" />
            </div>

            <div className="auth-card-container lms-login-card-container">
                {/* LEFT BRANDING PANEL */}
                <div className="auth-branding-panel lms-login-branding-panel">
                    <div className="lms-login-hex-bg" />
                    <div className="lms-login-blur-circle" />

                    <div className="lms-login-brand-content">
                        <div className="lms-flex-row lms-login-brand-header">
                            <div className="lms-login-brand-icon-wrap">
                                <Icons.Lock s={24} className="lms-login-brand-icon" />
                            </div>
                            <span className="lms-login-brand-name">{APP_CONFIG.name}</span>
                        </div>

                        <h1 className="lms-login-brand-title">
                            Join your<br />
                            <span className="lms-login-brand-highlight">Organization.</span>
                        </h1>

                        <p className="lms-login-brand-desc">
                            Create your personal student account and gain access to courses, videos, and certificates assigned to your organization.
                        </p>
                    </div>
                </div>

                {/* RIGHT AUTH FORM */}
                <div className="lms-login-form-panel">
                    <h2 className="lms-login-form-title">Create Account</h2>
                    <p className="lms-login-form-subtitle">Register to access your organization portal</p>

                    {!hasContext ? (
                        <div className="lms-premium-card" style={{ padding: '20px', textAlign: 'center', marginTop: '24px', border: '1px dashed var(--color-danger)' }}>
                            <Icons.Alert s={32} style={{ color: 'var(--color-danger)', marginBottom: '12px' }} />
                            <p style={{ fontSize: '14px', fontWeight: 'bold' }}>Invalid Registration Link</p>
                            <p style={{ opacity: 0.7, fontSize: '13px', marginTop: '4px' }}>Please use the registration link provided by your organization administrator.</p>
                            <button onClick={() => navigate('/login')} className="lms-btn lms-btn-secondary" style={{ marginTop: '16px', width: '100%', justifyContent: 'center' }}>
                                Back to Login
                            </button>
                        </div>
                    ) : (
                        <form onSubmit={handleSubmit} className="lms-login-form" style={{ marginTop: '20px' }}>
                            <div className="lms-form-row-2col" style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
                                <div className="lms-login-input-wrap">
                                    <div className="lms-login-input-icon">
                                        <Icons.User s={18} />
                                    </div>
                                    <input
                                        type="text"
                                        required
                                        value={firstName}
                                        onChange={(e) => setFirstName(e.target.value)}
                                        className="lms-login-auth-input"
                                        placeholder="First Name"
                                    />
                                </div>
                                <div className="lms-login-input-wrap">
                                    <div className="lms-login-input-icon">
                                        <Icons.User s={18} />
                                    </div>
                                    <input
                                        type="text"
                                        required
                                        value={lastName}
                                        onChange={(e) => setLastName(e.target.value)}
                                        className="lms-login-auth-input"
                                        placeholder="Last Name"
                                    />
                                </div>
                            </div>

                            <div className="lms-login-input-wrap">
                                <div className="lms-login-input-icon">
                                    <Icons.Mail s={18} />
                                </div>
                                <input
                                    type="email"
                                    required
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)}
                                    className="lms-login-auth-input"
                                    placeholder="Email Address"
                                />
                            </div>

                            <div className="lms-login-input-wrap">
                                <div className="lms-login-input-icon">
                                    <Icons.Phone s={18} />
                                </div>
                                <input
                                    type="tel"
                                    required
                                    value={mobile}
                                    onChange={(e) => setMobile(e.target.value)}
                                    className="lms-login-auth-input"
                                    placeholder="Mobile Number"
                                />
                            </div>

                            <div className="lms-login-input-wrap">
                                <div className="lms-login-input-icon">
                                    <Icons.Lock s={18} />
                                </div>
                                <input
                                    type={showPassword ? "text" : "password"}
                                    required
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)}
                                    className="lms-login-auth-input lms-login-auth-input-pass"
                                    placeholder="Password"
                                />
                                <button
                                    type="button"
                                    onClick={() => setShowPassword(!showPassword)}
                                    className="lms-login-pass-toggle"
                                >
                                    {showPassword ? <Icons.EyeOff s={18} /> : <Icons.Eye s={18} />}
                                </button>
                            </div>

                            <button type="submit" disabled={loading} className="lms-btn lms-btn-primary lms-login-submit-btn" style={{ marginTop: '16px' }}>
                                {loading ? (
                                    <Icons.Loader s={20} />
                                ) : (
                                    <>
                                        Register <Icons.Next s={16} />
                                    </>
                                )}
                            </button>
                        </form>
                    )}

                    <div className="lms-login-signup-wrap" style={{ marginTop: '20px' }}>
                        <p className="lms-login-signup-text">
                            Already have an account?{' '}
                            <Link to="/login" className="lms-login-signup-link">
                                Sign In
                            </Link>
                        </p>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Register;
