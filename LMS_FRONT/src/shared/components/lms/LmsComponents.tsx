import React, { useState, useRef, useEffect } from 'react';
import { Icons } from './Icons';
export { Icons };
import { apiClient } from '../../../core/api/apiClient';


import './LmsComponents.css';


// --- SHARED COMPONENTS: BUTTON ---
interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
    variant?: 'primary' | 'secondary' | 'accent' | 'danger' | 'success' | 'info' | 'ghost';
    solid?: boolean;
    size?: 'sm' | 'md' | 'lg';
    loading?: boolean;
    icon?: React.ElementType;
}

export const Button: React.FC<ButtonProps> = ({
    children,
    variant = 'primary',
    size = 'md',
    loading = false,
    icon: Icon,
    className = '',
    ...props
}) => {
    const variantClass = `lms-btn-${variant}`;
    const sizeClass = size !== 'md' ? `lms-btn-${size}` : '';

    return (
        <button
            className={`lms-btn ${variantClass} ${sizeClass} ${loading ? 'loading' : ''} ${className}`}
            disabled={loading || props.disabled}
            {...props}
        >
            {loading ? <div className="lms-loader-spinner" /> : Icon && <Icon s={18} />}
            {!loading && children}
        </button>
    );
};

// --- SHARED COMPONENTS: CARD ---
export const Card: React.FC<{ children: React.ReactNode; className?: string; title?: string; subtitle?: string; actions?: React.ReactNode }> = ({
    children, className = '', title, subtitle, actions
}) => (
    <div className={`lms-card lms-fade-in ${className}`}>
        {(title || subtitle || actions) && (
            <div className="lms-flex-row" style={{ justifyContent: 'space-between', marginBottom: '24px', alignItems: 'center' }}>
                <div>
                    {title && <h2 className="lms-card-title">{title}</h2>}
                    {subtitle && <p className="lms-status-sub">{subtitle}</p>}
                </div>
                {actions && <div className="lms-card-actions">{actions}</div>}
            </div>
        )}
        {children}
    </div>
);

// --- SHARED COMPONENTS: TABLE (Standardized) ---
export interface Column {
    header: string;
    key?: string;
    className?: string;
    hideOnMobile?: boolean;
    hideOnTablet?: boolean;
}

export const CommonTable: React.FC<{
    headers: (string | Column)[];
    children: React.ReactNode;
    loading?: boolean;
    empty?: boolean;
    emptyMessage?: string;
}> = ({ headers, children, loading, empty, emptyMessage = "No data found." }) => (
    <div className="lms-table-wrapper">
        {loading && !empty && (
            <div style={{ position: 'absolute', top: '12px', right: '24px', zIndex: 10 }}>
                <div className="lms-loader-spinner" style={{ width: '16px', height: '16px' }} />
            </div>
        )}
        <div className="lms-table-scroll">
            <table className="lms-table-main">
                <thead>
                    <tr>
                        {headers.map((h, i) => {
                            const label = typeof h === 'string' ? h : h.header;
                            const className = typeof h === 'string' ? '' : `${h.className || ''} ${h.hideOnMobile ? 'lms-hide-mobile' : ''} ${h.hideOnTablet ? 'lms-hide-tablet' : ''}`;
                            return <th key={i} className={className}>{label}</th>;
                        })}
                    </tr>
                </thead>
                <tbody>
                    {loading && empty ? (
                        <tr>
                            <td colSpan={headers.length} className="lms-table-empty">
                                <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', height: '100%', gap: '16px' }}>
                                    <div className="lms-loader-spinner" style={{ borderColor: 'rgba(var(--color-primary-rgb), 0.2)', borderTopColor: 'var(--color-primary)', width: '32px', height: '32px', borderWidth: '3px' }} />
                                    <div style={{ fontSize: '14px', fontWeight: '700', color: 'var(--color-text-dim)' }}>Loading data...</div>
                                </div>
                            </td>
                        </tr>
                    ) : empty ? (
                        <tr>
                            <td colSpan={headers.length} className="lms-table-empty">
                                <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', height: '100%', gap: '16px' }}>
                                    <Icons.Search s={40} style={{ opacity: 0.1 }} />
                                    <div style={{ fontSize: '14px', fontWeight: '700', color: 'var(--color-text-dim)' }}>{emptyMessage}</div>
                                </div>
                            </td>
                        </tr>
                    ) : children}
                </tbody>
            </table>
        </div>
    </div>
);

// --- SHARED COMPONENTS: GRID (Standardized) ---
export const CommonGrid: React.FC<{
    children: React.ReactNode;
    className?: string;
    loading?: boolean;
    empty?: boolean;
    emptyMessage?: string;
}> = ({ children, className = '', loading, empty, emptyMessage = "No items to display" }) => (
    <div className={`lms-grid-container ${className}`}>
        {loading && !empty && (
            <div style={{ position: 'absolute', top: '-10px', right: '10px', zIndex: 10 }}>
                <div className="lms-loader-spinner" style={{ width: '16px', height: '16px' }} />
            </div>
        )}
        {loading && empty ? (
            Array.from({ length: 4 }).map((_, i) => (
                <div key={i} className="lms-grid-card skeleton">
                    <div className="lms-grid-banner" style={{ background: 'var(--color-bg-alt)', opacity: 0.5 }} />
                    <div className="lms-grid-body">
                        <div style={{ height: '20px', background: 'var(--color-bg-alt)', borderRadius: '4px', width: '60%', marginBottom: '12px' }} />
                        <div style={{ height: '14px', background: 'var(--color-bg-alt)', borderRadius: '4px', width: '90%' }} />
                    </div>
                </div>
            ))
        ) : empty ? (
            <div className="lms-grid-empty">
                <Icons.Search s={40} style={{ opacity: 0.1, marginBottom: '12px' }} />
                <div style={{ fontSize: '14px', fontWeight: '700', color: 'var(--color-text-dim)' }}>{emptyMessage}</div>
            </div>
        ) : children}
    </div>
);

// Backward Compatibility Aliases
export const Table = CommonTable;
export const Grid = CommonGrid;

// --- SHARED COMPONENTS: DROPDOWN / SELECT ---
export const Dropdown: React.FC<{ options: { value: string; label: string }[]; value: string; onChange: (v: string) => void; placeholder?: string; className?: string; style?: React.CSSProperties }> = ({ options, value, onChange, placeholder, className = '', style }) => (
    <select
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className={`lms-select ${className}`}
        style={{ ...style }}
    >
        {placeholder && <option value="" disabled>{placeholder}</option>}
        {options.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
    </select>
);

// --- SHARED COMPONENTS: CUSTOM SELECT (Premium) ---
export const CustomSelect: React.FC<{
    options: { value: string | number; label: string }[];
    value: string | number;
    onChange: (v: any) => void;
    placeholder?: string;
    style?: React.CSSProperties;
    className?: string;
}> = ({ options, value, onChange, placeholder, style, className = '' }) => {
    const [isOpen, setIsOpen] = useState(false);
    const containerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const handleClickOutside = (e: MouseEvent) => {
            if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
                setIsOpen(false);
            }
        };
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    const selectedOption = options.find(o => String(o.value) === String(value));
    const currentLabel = selectedOption ? selectedOption.label : (placeholder || 'Select...');

    return (
        <div className={`lms-custom-select-container ${className}`} style={{ ...style }} ref={containerRef}>
            <div
                className={`lms-custom-select-trigger ${isOpen ? 'open' : ''}`}
                onClick={() => setIsOpen(!isOpen)}
            >
                <span className="lms-custom-select-label">{currentLabel}</span>
                <Icons.Prev s={12} className={`lms-custom-select-icon ${isOpen ? 'open' : 'closed'}`} />
            </div>

            {isOpen && (
                <div className="lms-custom-select-menu lms-fade-in">
                    {options.map((o, idx) => (
                        <div
                            key={idx}
                            className={`lms-custom-select-item${String(value) === String(o.value) ? ' active' : ''}`}
                            onClick={() => {
                                onChange(o.value);
                                setIsOpen(false);
                            }}
                        >
                            {o.label}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};


// --- SHARED COMPONENTS: STATUS FILTER ---
export const StatusFilter: React.FC<{ value: string; onChange: (v: string) => void; style?: React.CSSProperties }> = ({ value, onChange, style }) => {
    const options = [
        { value: 'all', label: 'All Status' },
        { value: 'active', label: 'Active' },
        { value: 'inactive', label: 'Inactive' }
    ];

    return <CustomSelect options={options} value={value || 'all'} onChange={onChange} style={style} />;
};

// --- SHARED COMPONENTS: PAGINATION (Upgraded) ---
export const Pagination = ({
    current,
    total,
    totalItems,
    itemsPerPage,
    onPageChange,
    onPageSizeChange
}: {
    current: number,
    total: number,
    totalItems?: number,
    itemsPerPage?: number,
    onPageChange: (p: number) => void,
    onPageSizeChange?: (s: number) => void
}) => {
    const totalSafe = Math.max(1, total);
    const safeCurrent = Math.min(Math.max(1, current), totalSafe);

    let pages: (number | string)[] = [];
    if (totalSafe <= 7) {
        pages = Array.from({ length: totalSafe }, (_, i) => i + 1);
    } else {
        if (safeCurrent <= 4) {
            pages = [1, 2, 3, 4, 5, '...', totalSafe];
        } else if (safeCurrent >= totalSafe - 3) {
            pages = [1, '...', totalSafe - 4, totalSafe - 3, totalSafe - 2, totalSafe - 1, totalSafe];
        } else {
            pages = [1, '...', safeCurrent - 1, safeCurrent, safeCurrent + 1, '...', totalSafe];
        }
    }

    return (
        <div className="lms-pagination-orchestrator">
            <div className="lms-pagination-info lms-hide-mobile">
                Showing <span className="lms-pagination-count">{(safeCurrent - 1) * (itemsPerPage || 10) + 1}</span> to <span className="lms-pagination-count">{Math.min(safeCurrent * (itemsPerPage || 10), totalItems || 0)}</span> of <span className="lms-pagination-count">{totalItems || 0}</span> results
            </div>

            <div className="lms-pagination-info lms-show-mobile">
                Page <span className="lms-pagination-count">{safeCurrent}</span> of <span className="lms-pagination-count">{totalSafe}</span>
            </div>

            <div className="lms-pagination-controls">
                <button
                    disabled={safeCurrent === 1}
                    onClick={() => onPageChange(safeCurrent - 1)}
                    className="lms-page-btn nav"
                >
                    <Icons.Prev s={14} />
                </button>

                <div className="lms-page-numbers">
                    {pages.map((p, idx) => (
                        typeof p === 'number' ? (
                            <button
                                key={idx}
                                onClick={() => onPageChange(p)}
                                className={`lms-page-btn${safeCurrent === p ? ' active' : ''}`}
                            >
                                {p}
                            </button>
                        ) : (
                            <span key={idx} className="lms-page-ellipsis">{p}</span>
                        )
                    ))}
                </div>

                <button
                    disabled={safeCurrent >= totalSafe}
                    onClick={() => onPageChange(safeCurrent + 1)}
                    className="lms-page-btn nav"
                >
                    <Icons.Next s={14} />
                </button>
            </div>

            {onPageSizeChange && (
                <div className="lms-pagination-size lms-hide-mobile">
                    <label>Show</label>
                    <select
                        className="lms-pagination-select"
                        value={itemsPerPage}
                        onChange={(e) => onPageSizeChange(Number(e.target.value))}
                    >
                        {[10, 25, 50, 100].map(s => (
                            <option key={s} value={s}>{s} items</option>
                        ))}
                    </select>
                </div>
            )}
        </div>
    );
};

// --- SHARED COMPONENTS: INPUT ---
export const Input = ({ label, icon: Icon, ...props }: React.InputHTMLAttributes<HTMLInputElement> & { label?: string; icon?: React.ElementType }) => (
    <div className="lms-form-group">
        {label && <label className="lms-form-label">{label}</label>}
        <div className="lms-input-wrapper">
            {Icon && <Icon s={18} className="lms-input-icon" />}
            <input className={`lms-login-auth-input ${Icon ? 'with-icon' : ''}`} {...props} />
        </div>
    </div>
);

// --- SHARED COMPONENTS: SEARCH ---
export const SearchInput: React.FC<{ value: string; onChange: (v: string) => void; placeholder?: string; style?: React.CSSProperties; className?: string }> = ({ value, onChange, placeholder, style, className = '' }) => {
    const [localValue, setLocalValue] = useState(value);

    // Sync local value when external value changes (e.g., cleared by parent)
    useEffect(() => {
        setLocalValue(value);
    }, [value]);

    // Debounce internal change
    useEffect(() => {
        const handler = setTimeout(() => {
            if (localValue !== value) {
                onChange(localValue);
            }
        }, 400); // 400ms is the "Goldilocks" zone for responsiveness vs. stability
        return () => clearTimeout(handler);
    }, [localValue, onChange, value]);

    return (
        <div className={`lms-search-input-group ${className}`} style={{ ...style }}>
            <div className="lms-search-input-inner">
                <Icons.Search s={18} className="lms-search-inner-icon" />
                <input
                    className="lms-search-input-core-new"
                    placeholder={placeholder || 'Search records...'}
                    value={localValue}
                    onChange={e => setLocalValue(e.target.value)}
                />
            </div>
        </div>
    );
};

// --- SHARED COMPONENTS: SECURE IMAGE ---
export const SecureImage = ({ src, className, alt = "" }: { src: string; className?: string; alt?: string }) => {
    const [imgUrl, setImgUrl] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(false);

    useEffect(() => {
        if (!src) {
            setLoading(false);
            return;
        }

        let isMounted = true;
        setLoading(true);
        setError(false);

        let requestUrl = src;
        // Only wrap if it's an encrypted path and not already routed through Crypto controller
        if (src.endsWith('.enc') && !src.includes('Crypto/get')) {
            requestUrl = `Crypto/get?path=${encodeURIComponent(src)}`;
        }
        // If it starts with /api/, remove it as apiClient base already has it
        if (requestUrl.startsWith('/api/')) {
            requestUrl = requestUrl.substring(5);
        }

        apiClient.get(requestUrl, { responseType: 'blob' })
            .then(res => {
                if (isMounted) {
                    const url = URL.createObjectURL(res.data);
                    setImgUrl(url);
                    setLoading(false);
                }
            })
            .catch(err => {
                console.error("[LMS SECURE IMAGE] Load Fault:", err);
                if (isMounted) {
                    setError(true);
                    setLoading(false);
                }
            });

        return () => { isMounted = false; };
    }, [src]);

    if (loading) return <div className={`lms-secure-image-loading ${className}`}><div className="lms-loader-spinner small" /></div>;
    if (error || !imgUrl) return <div className={`lms-secure-image-error ${className}`}><Icons.Check s={16} /></div>;

    return <img src={imgUrl} className={className} alt={alt} />;
};


// --- SHARED COMPONENTS: BUTTON ---
export const PermissionButton = ({
    hasPermission, fallbackTooltip = "Insufficient Permissions", children, className = '', disabled, style, icon: Icon, ...props
}: any) => {
    if (!hasPermission) {
        return (
            <button
                title={fallbackTooltip}
                className={`${className}`}
                disabled={true}
                style={{ ...style, opacity: 0.4, cursor: 'not-allowed', filter: 'grayscale(100%)' }}
                {...props}
            >
                {Icon && <Icon s={18} className="lms-btn-icon" />}
                {children}
            </button>
        );
    }
    return (
        <button className={className} disabled={disabled} style={style} {...props}>
            {Icon && <Icon s={18} className="lms-btn-icon" />}
            {children}
        </button>
    );
};

// --- NAVIGATION primitives ---
export const NavItem = ({ id, icon: Icon, label, tab, setTab }: any) => (
    <div onClick={() => setTab(id)} className={`lms-nav-item${tab === id ? ' active' : ''}`}>
        {Icon && <Icon s={18} strokeWidth={2} />}
        <span>{label}</span>
    </div>
);

// --- TAB SWITCHER (Standardized) ---
export const TabSwitcher = ({ tabs, activeTab, onTabSelect, style }: { tabs: { id: string, label: string, icon?: any, disabled?: boolean }[], activeTab: string, onTabSelect: (id: string) => void, style?: React.CSSProperties }) => {
    // Disable instead of removing layout elements
    return (
        <div className="lms-view-toggle" style={style}>
            {tabs.map(t => (
                <button
                    key={t.id}
                    disabled={t.disabled}
                    onClick={() => !t.disabled && onTabSelect(t.id)}
                    className={`lms-view-btn${activeTab === t.id ? ' active' : ''}`}
                    style={{ opacity: t.disabled ? 0.3 : 1, cursor: t.disabled ? 'not-allowed' : 'pointer' }}
                    title={t.disabled ? 'Access Restricted' : ''}
                >
                    {t.icon && <t.icon s={16} />} {t.label}
                </button>
            ))}
        </div>
    );
};

// --- SECURITY SWITCHER ---
export const SecuritySwitcher: React.FC<any> = ({ tab, setTab, hasPermission, isSuperAdmin }) => {
    const items = [
        { id: 'sec', label: 'ROLES', icon: Icons.Shield, visible: isSuperAdmin || hasPermission('ROLE', 'ROLE_VIEW') },
        { id: 'mod', label: 'MODULES', icon: Icons.Grid, visible: isSuperAdmin },
        { id: 'perm', label: 'PERMS', icon: Icons.Lock, visible: isSuperAdmin },
        { id: 'mod_perms', label: 'MOD-PERMS', icon: Icons.Shield, visible: isSuperAdmin },
        { id: 'role_modules', label: 'ROLE-MODS', icon: Icons.Shield, visible: isSuperAdmin || hasPermission('ROLE_MODULE', 'ROLE_MODULE_VIEW') },
        { id: 'role_mod_perms', label: 'ROLE MATRIX', icon: Icons.Shield, visible: isSuperAdmin || hasPermission('ROLE_MODULE', 'ROLE_MODULE_PERMISSION_VIEW') },
        { id: 'user_roles', label: 'USER ROLES', icon: Icons.User, visible: isSuperAdmin || hasPermission('USER_ROLE', 'USER_ROLE_VIEW') },
    ].filter(i => i.visible);

    return <TabSwitcher tabs={items as any} activeTab={tab} onTabSelect={setTab} style={{ marginTop: '24px' }} />;
};


// --- GROUP SWITCHER ---
export const GroupSwitcher: React.FC<any> = ({ tab, setTab, hasPermission, isSuperAdmin }) => {
    const items = [
        { id: 'groups', label: 'GROUPS LIST', icon: Icons.Groups, visible: isSuperAdmin || hasPermission('GROUP', 'GROUP_VIEW') },
        { id: 'gc', label: 'GROUP COURSE', icon: Icons.Book, visible: isSuperAdmin || hasPermission('GROUP', 'GROUP_COURSE_VIEW') || hasPermission('GROUP', 'GROUP_COURSE_EDIT') },
        { id: 'gu', label: 'GROUP USER', icon: Icons.Users, visible: isSuperAdmin || hasPermission('GROUP', 'GROUP_USER_VIEW') || hasPermission('GROUP', 'GROUP_USER_EDIT') },
    ].filter(i => i.visible);

    return <TabSwitcher tabs={items as any} activeTab={tab} onTabSelect={setTab} style={{ marginTop: '24px' }} />;
};


// --- SWITCHERS ---
export const PerspectiveSwitcher = ({ viewMode, setViewMode }: any) => (
    <div className="lms-view-toggle">
        <button onClick={() => setViewMode('table')} className={`lms-view-btn${viewMode === 'table' ? ' active' : ''}`} style={{ width: '34px', height: '34px', padding: 0, justifyContent: 'center' }}><Icons.Table s={18} /></button>
        <button onClick={() => setViewMode('grid')} className={`lms-view-btn${viewMode === 'grid' ? ' active' : ''}`} style={{ width: '34px', height: '34px', padding: 0, justifyContent: 'center' }}><Icons.Grid s={18} /></button>
    </div>
);
export const ViewToggle = PerspectiveSwitcher;

// --- AUTH GATE ---
import { authApi } from '../../../features/auth/api/authApi';
import { OrganizationRegister } from '../../../features/organization/components/OrganizationRegister';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';

export const AuthGate = ({ onComplete, decryptor }: any) => {
    const navigate = useNavigate();
    const [mode, setMode] = useState<'login' | 'register' | 'org_register'>(() => {
        if (window.location.pathname === '/organization/register') return 'org_register';
        return 'login';
    });

    const [loading, setLoading] = useState(false);

    const handleLogin = async (e: any) => {
        e.preventDefault();
        setLoading(true);
        try {
            const response = await authApi.login({ email: e.target.email.value, password: e.target.password.value });
            const res = response.data;
            if (res.success && res.data) {
                const data = Array.isArray(res.data) ? res.data[0] : res.data;
                const token = data.token || data.Token;
                const user = data.user || data.User;

                localStorage.setItem('token', token);
                localStorage.setItem('user', JSON.stringify(user));

                let perms = {};
                const encPerms = data.encryptedPermissions || data.EncryptedPermissions;
                if (encPerms) {
                    try { perms = await decryptor(encPerms); }
                    catch (err) { console.error("[LMS CRYPTO] Permission Decryption Fault:", err); }
                }
                localStorage.setItem('permissions', JSON.stringify(perms));
                onComplete(user, perms);
                toast.success('Login Successful');
                setTimeout(() => navigate('/dashboard', { replace: true }), 150);
            } else { toast.error(res.message || 'Incorrect email or password.'); }
        } catch (err: any) {
            toast.error(err.message || 'Login failed.');
        } finally { setLoading(false); }
    };

    const handleRegister = async (e: any) => {
        e.preventDefault();
        setLoading(true);
        const data = {
            firstName: e.target.firstName.value,
            lastName: e.target.lastName.value,
            mobile: e.target.mobile.value,
            email: e.target.email.value,
            password: e.target.password.value,
            organizationCode: e.target.orgCode.value
        };
        try {
            const res = await authApi.register(data);
            if (res.data.success) {
                toast.success('Account Created Successfully.');
                setMode('login');
            } else {
                toast.error(res.data.message || 'Registration failed.');
            }
        } catch (err: any) {
            toast.error(err.message || 'An error occurred during registration.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="lms-auth-centered-root">
            <div className="lms-auth-simple-card lms-fade-in">
                <div className="lms-auth-brand-minimal">
                    <h1>SoulCode</h1>
                    <p>
                        {mode === 'login' && "Sign in to your account"}
                        {mode === 'register' && "Create personal account"}
                        {mode === 'org_register' && "Register Organization"}
                    </p>
                </div>

                <div className="lms-auth-form-body">
                    {mode === 'login' && (
                        <form onSubmit={handleLogin} className="lms-auth-simple-group">
                            <div className="lms-auth-simple-field">
                                <label>Email Address <span style={{ color: 'var(--color-primary)' }}>*</span></label>
                                <input name="email" type="email" placeholder="email@example.com" className="lms-auth-simple-input" required />
                            </div>
                            <div className="lms-auth-simple-field">
                                <label>Password <span style={{ color: 'var(--color-primary)' }}>*</span></label>
                                <input name="password" type="password" placeholder="••••••••" className="lms-auth-simple-input" required />
                            </div>

                            <button type="submit" disabled={loading} className="lms-auth-simple-btn">
                                {loading ? "Signing in..." : "Sign In"}
                            </button>

                            <div className="lms-auth-simple-footer">
                                <button type="button" onClick={() => setMode('register')} className="lms-auth-simple-footer-btn">Create account</button>
                                <div style={{ marginTop: '8px' }}>
                                    <button type="button" onClick={() => setMode('org_register')} className="lms-auth-simple-footer-btn" style={{ opacity: 0.7, fontSize: '12px' }}>Enterprise Registration</button>
                                </div>
                            </div>
                        </form>
                    )}

                    {mode === 'register' && (
                        <form onSubmit={handleRegister} className="lms-auth-simple-group">
                            <div className="lms-form-row-2col">
                                <div className="lms-auth-simple-field">
                                    <label>First Name <span style={{ color: 'var(--color-primary)' }}>*</span></label>
                                    <input name="firstName" placeholder="First Name" className="lms-auth-simple-input" required />
                                </div>
                                <div className="lms-auth-simple-field">
                                    <label>Last Name <span style={{ color: 'var(--color-primary)' }}>*</span></label>
                                    <input name="lastName" placeholder="Last Name" className="lms-auth-simple-input" required />
                                </div>
                            </div>
                            <div className="lms-auth-simple-field">
                                <label>Email <span style={{ color: 'var(--color-primary)' }}>*</span></label>
                                <input name="email" type="email" placeholder="Email" className="lms-auth-simple-input" required />
                            </div>
                            <div className="lms-auth-simple-field">
                                <label>Org Code <span style={{ color: 'var(--color-primary)' }}>*</span></label>
                                <input name="orgCode" placeholder="Organization Code" className="lms-auth-simple-input" required />
                            </div>
                            <div className="lms-auth-simple-field">
                                <label>Password <span style={{ color: 'var(--color-primary)' }}>*</span></label>
                                <input name="password" type="password" placeholder="••••••••" className="lms-auth-simple-input" required />
                            </div>

                            <button type="submit" disabled={loading} className="lms-auth-simple-btn">
                                {loading ? "Registering..." : "Create Account"}
                            </button>

                            <div className="lms-auth-simple-footer">
                                <button type="button" onClick={() => setMode('login')} className="lms-auth-simple-footer-btn">Back to Login</button>
                            </div>
                        </form>
                    )}


                    {mode === 'org_register' && (
                        <div className="lms-fade-in">
                            <OrganizationRegister onBack={() => setMode('login')} onSuccess={() => setMode('login')} />
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

