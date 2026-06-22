import React, { useEffect } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../store/index';
import { setCredentials } from '../../features/auth/store/authSlice';

export const AuthGuard: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const dispatch = useAppDispatch();
    const location = useLocation();
    const { isAuthenticated } = useAppSelector((state: any) => state.auth);

    useEffect(() => {
        const token = localStorage.getItem('token');
        const storedUser = localStorage.getItem('user');
        const storedPerms = localStorage.getItem('permissions');
        if (token && storedUser && !isAuthenticated) {
            try {
                dispatch(setCredentials({ 
                    user: JSON.parse(storedUser), 
                    token,
                    permissions: storedPerms ? JSON.parse(storedPerms) : {}
                }));
            } catch (e) {
                localStorage.clear();
            }
        }
    }, [dispatch, isAuthenticated]);

    if (!isAuthenticated && !['/login', '/register', '/organization/register'].includes(location.pathname)) {
        return <Navigate to="/login" replace />;
    }

    return <>{children}</>;
};
