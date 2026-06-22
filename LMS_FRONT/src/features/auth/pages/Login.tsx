import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAppDispatch } from '../../../store';
import { setCredentials } from '../store/authSlice';
import { authApi } from '../api/authApi';
import { toast } from 'react-toastify';
import { Icons } from '../../../shared/components/lms/Icons';
import { APP_CONFIG } from '../../../config/app.config';
import { decryptPermissions } from '../../../shared/utils/decryptPermissions';
import { useTheme } from '../../../app/providers/ThemeProvider';
import '../Auth.css';

const Login = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);
    const [showPassword, setShowPassword] = useState(false);

    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    const { setTheme } = useTheme();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            setLoading(true);
            const response = await authApi.login({ email, password });

            if (response && response.data.success && response.data.data?.token && response.data.data?.user) {
                const { user, token, encryptedPermissions } = response.data.data;
                dispatch(setCredentials({ user: user, token: token }));
                localStorage.setItem("token", token);
                localStorage.setItem("user", JSON.stringify(user));
                if (encryptedPermissions) {
                    localStorage.setItem("encryptedPermissions", encryptedPermissions);
                    const permsMap = await decryptPermissions(encryptedPermissions);
                    localStorage.setItem("permissions", JSON.stringify(permsMap));
                } else {
                    localStorage.removeItem("encryptedPermissions");
                    localStorage.removeItem("permissions");
                }
                setTheme('light');
                toast.success("Welcome back to SoulCode!");
                navigate("/dashboard");
            } else {
                throw new Error(response.data.message || "Invalid response from server");
            }
        } catch (err: any) {
            console.error(err);
            toast.error(err.message || "Failed to login. Please check your credentials.");
        } finally {
            setLoading(false);
        }
    };

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
                    {/* Hexagon style background decoration */}
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
                            Empowering<br />
                            <span className="lms-login-brand-highlight">Communication.</span>
                        </h1>

                        <p className="lms-login-brand-desc">
                            The most secure and advanced learning management system for modern organizations. Built for scale, designed for excellence.
                        </p>
                    </div>
                </div>

                {/* RIGHT AUTH FORM */}
                <div className="lms-login-form-panel">
                    <h2 className="lms-login-form-title">Welcome Back</h2>
                    <p className="lms-login-form-subtitle">Sign in to your dashboard</p>

                    <form onSubmit={handleSubmit} className="lms-login-form">
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

                        <div className="lms-login-forgot-wrap">
                            <Link to="/forgot-password" className="lms-login-forgot-link">Forgot Password?</Link>
                        </div>

                        <button type="submit" disabled={loading} className="lms-btn lms-btn-primary lms-login-submit-btn">
                            {loading ? (
                                <Icons.Loader s={20} />
                            ) : (
                                <>
                                    Sign In <Icons.Next s={16} />
                                </>
                            )}
                        </button>
                    </form>

                    <div className="lms-login-signup-wrap">
                        <p className="lms-login-signup-text">
                            Don't have an account?{' '}
                            <Link to="/register" className="lms-login-signup-link">
                                Sign Up
                            </Link>
                        </p>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Login;
