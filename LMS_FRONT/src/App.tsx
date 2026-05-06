import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { store, persistor, useAppSelector, useAppDispatch } from './store/index';
import { setCredentials } from './features/auth/store/authSlice';
import { AuthGate } from './shared/components/lms/LmsComponents';
import { decryptPermissions } from './shared/utils/decryptPermissions';

// Core Orchestration
import { AppProviders } from './app/providers/AppProviders';
import { AuthGuard } from './app/guards/AuthGuard';
import { AppRoutes } from './app/routes/AppRoutes';
import { StudioLayout } from './layouts/StudioLayout';
import { StudentLayout } from './layouts/StudentLayout';
import { GlobalModalOrchestrator } from './shared/components/modals/GlobalModalOrchestrator';
import { useLmsOrchestrator } from './shared/hooks/useLmsOrchestrator';
import { StudioErrorBoundary } from './app/guards/StudioErrorBoundary';

/**
 * LMS Studio App Orchestrator
 * This is the primary entry point for the LMS Frontend.
 * It coordinates the layout, security guards, and global data flow.
 */
const AppContent: React.FC = () => {
    const dispatch = useAppDispatch();
    const orch = useLmsOrchestrator();
    const { isAuthenticated, user } = orch;

    console.log("[LMS DEBUG] Current User:", user);
    console.log("[LMS DEBUG] Is Student:", orch.isStudent);

    const { changePage } = orch;

    const onAuthComplete = (user: any, perms: any) => {
        localStorage.setItem('permissions', JSON.stringify(perms));
        dispatch(setCredentials({
            user,
            token: localStorage.getItem('token') || '',
            permissions: perms
        }));
    };

    return (
        <StudioErrorBoundary>
            {isAuthenticated ? (
                <Routes>
                    <Route
                        path="/*"
                        element={
                            <AuthGuard>
                                <GlobalModalOrchestrator {...orch} />
                                {orch.isUnauthorized ? (
                                    <div className="lms-unauthorized-overlay" style={{ height: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'var(--color-bg-subtle)' }}>
                                        <div className="lms-premium-card" style={{ padding: '40px', textAlign: 'center', maxWidth: '400px' }}>
                                            <h2 style={{ color: 'var(--color-danger)', marginBottom: '16px' }}>Access Denied</h2>
                                            <p style={{ opacity: 0.7, marginBottom: '24px' }}>Your account is pending approval or has no assigned roles/permissions. Please contact an administrator.</p>
                                            <button 
                                                onClick={() => {
                                                    localStorage.clear();
                                                    window.location.href = '/login';
                                                }} 
                                                className="lms-btn lms-btn-primary"
                                                style={{ width: '100%', justifyContent: 'center' }}
                                            >
                                                Back to Login
                                            </button>
                                        </div>
                                    </div>
                                ) : orch.isStudent ? (
                                    <StudentLayout
                                        {...orch}
                                        activeOrg={orch.activeOrg}
                                        activeCourse={orch.activeCourse}
                                        setActiveCourse={orch.setActiveCourse}
                                    >
                                        <AppRoutes
                                            {...orch}
                                            changePage={orch.changePage}
                                            onAuthComplete={onAuthComplete}
                                        />
                                    </StudentLayout>
                                ) : (
                                    <StudioLayout {...orch} activeOrg={orch.activeOrg}>
                                        <AppRoutes
                                            {...orch}
                                            changePage={changePage}
                                            onAuthComplete={onAuthComplete}
                                        />
                                    </StudioLayout>
                                )}
                            </AuthGuard>
                        }
                    />
                </Routes>
            ) : (
                <Routes>
                    <Route path="/login" element={<AuthGate onComplete={onAuthComplete} decryptor={decryptPermissions} />} />
                    <Route path="/register" element={<AuthGate onComplete={onAuthComplete} decryptor={decryptPermissions} />} />
                    <Route path="/organization/register" element={<AuthGate onComplete={onAuthComplete} decryptor={decryptPermissions} />} />
                    <Route path="*" element={<Navigate to="/login" replace />} />
                </Routes>
            )}
        </StudioErrorBoundary>
    );
};

// Root Entry with Global Providers
const Root: React.FC = () => {
    const { isAuthenticated } = useAppSelector((state: any) => state.auth);
    return <AppContent key={isAuthenticated ? 'studio' : 'guest'} />;
};

// Root Entry with Global Providers
const App: React.FC = () => (
    <AppProviders>
        <Root />
    </AppProviders>
);

export default App;
