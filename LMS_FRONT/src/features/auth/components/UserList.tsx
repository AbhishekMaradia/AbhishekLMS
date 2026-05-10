import React from 'react';
import { Icons } from '../../../shared/components/lms/Icons';
import { CommonTable, CommonGrid } from '../../../shared/components/lms/LmsComponents';
import { userApi } from '../api/userApi';
import type { UserDto } from '../types/auth.types';
import '../Auth.css';

interface UserListProps {
    users: UserDto[];
    currentUser: UserDto | null;
    userStatusFilter: 'all' | 'active' | 'inactive';
    usersLoading: boolean;
    viewMode: 'table' | 'grid';
    hasPermission: (module: string, permission?: string) => boolean;
    setUi: (val: any) => void;
    ui: any;
    setFormTenantId?: (val: number | null) => void;
    handleCrud: (action: string, type: string, data: any) => void;
    extractData?: (res: any) => any;
    toggleUserStatus?: (u: any) => void;
}

export const UserList: React.FC<UserListProps> = ({
    users,
    currentUser,
    userStatusFilter,
    usersLoading,
    viewMode,
    hasPermission,
    setUi,
    ui,
    setFormTenantId,
    handleCrud,
    extractData,
}) => {
    const filteredUsers = (users || []).filter(u => {
        const uid = u.id || (u as any).Id;
        const currentUid = currentUser?.id || (currentUser as any)?.Id;
        const matchesStatus =
            userStatusFilter === 'all' ? true :
                userStatusFilter === 'active' ? (u.isActive !== false) :
                    (u.isActive === false);
        return uid !== currentUid && matchesStatus;
    });

    const headers = [
        { header: 'User Name', key: 'name' },
        { header: 'Email Address', key: 'email', hideOnMobile: true },
        { header: 'Role', key: 'role' },
        { header: 'Organization', key: 'org', hideOnMobile: true },
        { header: 'Status', key: 'status' },
        { header: 'Actions', key: 'actions', className: 'lms-text-right' }
    ];

    if (viewMode === 'table') {
        return (
            <CommonTable
                headers={headers}
                loading={usersLoading}
                empty={filteredUsers.length === 0}
            >
                {filteredUsers.map(u => (
                    <tr key={u.id || (u as any).Id}>
                        <td>
                            <div className="lms-cell-bold">{(u.firstName || '')} {(u.lastName || '')}</div>
                        </td>
                        <td className="lms-hide-mobile">
                            <div className="lms-cell-id">{u.email}</div>
                        </td>
                        <td>
                            <span className="lms-tag info">{(u.userRole || 'None').replace(/_/g, ' ')}</span>
                        </td>
                        <td className="lms-hide-mobile">
                            <span className="lms-tag info">{u.orgName || 'Super Admin'}</span>
                        </td>
                        <td>
                            <div className={`lms-status-dot ${u.isActive !== false ? 'active' : 'inactive'}`}>
                                {u.isActive !== false ? 'Active' : 'Inactive'}
                            </div>
                        </td>
                        <td>
                            <div className="lms-cell-actions lms-cl-actions-left">

                                {hasPermission('USER', 'USER_EDIT') && (
                                    <button
                                        onClick={async () => {
                                            try {
                                                const res = await userApi.getById(u.id || (u as any).Id);
                                                const full = extractData ? extractData(res) : (res.data?.data || res.data || res);
                                                const fullUser = Array.isArray(full) ? full[0] : full;
                                                setUi({ ...ui, modal: 'user_edit', target: fullUser || u });
                                                if (setFormTenantId) setFormTenantId(fullUser?.tenantId ?? u.tenantId ?? null);
                                            } catch {
                                                setUi({ ...ui, modal: 'user_edit', target: u });
                                            }
                                        }}
                                        className="lms-icon-btn-sm info"
                                        title="Edit"
                                    >
                                        <Icons.Edit s={16} />
                                    </button>
                                )}
                                {hasPermission('USER', 'USER_DELETE') && (
                                    <button
                                        onClick={() => handleCrud('delete', 'user', u.id || (u as any).Id)}
                                        className="lms-icon-btn-sm danger"
                                        title="Delete"
                                    >
                                        <Icons.Trash s={16} />
                                    </button>
                                )}

                            </div>
                        </td>
                    </tr>
                ))}
            </CommonTable>
        );
    }

    return (
        <CommonGrid
            loading={usersLoading}
            empty={filteredUsers.length === 0}
        >
            {filteredUsers.map(u => {
                const uid = u.id || (u as any).Id;
                const isActive = u.isActive !== false;

                return (
                    <div key={uid} className="lms-grid-card lms-fade-in">
                        <div className="lms-grid-banner primary">
                            <div className="lms-grid-overlay" />
                            <div className="lms-user-avatar-premium">
                                {(u.firstName?.[0] || 'U')}{(u.lastName?.[0] || '')}
                            </div>
                            <div className="lms-grid-badge">
                                <span className={`lms-tag ${isActive ? 'success' : 'danger'}`}>
                                    {isActive ? 'ACTIVE' : 'INACTIVE'}
                                </span>
                            </div>
                        </div>

                        <div className="lms-grid-body">
                            <h3 className="lms-grid-title">{(u.firstName || 'Unknown')} {(u.lastName || 'User')}</h3>

                            <div className="lms-grid-meta">
                                <Icons.Mail s={12} />
                                <span className="lms-user-list-email">{u.email}</span>
                            </div>

                            <div className="lms-grid-description lms-user-list-desc">
                                {u.userRole === 'SUPER_ADMIN' ? 'Platform Administrator.' :
                                    u.userRole === 'ORG_ADMIN' ? 'Organization Administrator.' :
                                        'Standard Platform User.'}
                            </div>

                            <div className="lms-grid-footer lms-user-list-footer">
                                <div className="lms-grid-actions">
                                    {hasPermission('USER', 'USER_EDIT') && (
                                        <button
                                            onClick={() => setUi({ ...ui, modal: 'user_edit', target: u })}
                                            className="lms-icon-btn-sm info"
                                            title="Edit"
                                        >
                                            <Icons.Edit s={16} />
                                        </button>
                                    )}
                                    {hasPermission('USER', 'USER_DELETE') && (
                                        <button
                                            onClick={() => handleCrud('delete', 'user', uid)}
                                            className="lms-icon-btn-sm danger"
                                            title="Delete"
                                        >
                                            <Icons.Trash s={16} />
                                        </button>
                                    )}

                                </div>
                                <div className="lms-grid-meta lms-user-list-meta-small">
                                    <Icons.Org s={10} /> {u.orgName || 'Platform'}
                                </div>
                            </div>
                        </div>
                    </div>
                );
            })}
        </CommonGrid>
    );
};
