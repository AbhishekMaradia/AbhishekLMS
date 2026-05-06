import React from 'react';
import { Icons } from '../shared/components/lms/Icons';
import { UserList } from '../features/auth/components/UserList';
import { Pagination, Button, PerspectiveSwitcher, SearchInput, StatusFilter } from '../shared/components/lms/LmsComponents';
import '../features/auth/Auth.css';

interface UsersPageProps {
    db: any;
    ui: any;
    setUi: (u: any) => void;
    user: any;
    isSuperAdmin: boolean;
    searchTerm: string;
    setSearchTerm: (s: string) => void;
    viewMode: 'table' | 'grid';
    setViewMode: (v: 'table' | 'grid') => void;
    pagination: any;
    changePage: (e: string, p: number) => void;
    changePageSize: (e: string, s: number) => void;
    hasPermission: (m: string, a?: string) => boolean;
    handleCrud: (a: string, t: string, d: any) => void;
}

export const UsersPage: React.FC<UsersPageProps> = ({
    db, ui, setUi, user, isSuperAdmin, searchTerm, setSearchTerm,
    viewMode, setViewMode, pagination, changePage, changePageSize,
    hasPermission, handleCrud
}) => {
    const p = pagination['users'] || { page: 1, size: 50, total: 0 };
    const totalPages = Math.ceil((p.total || 0) / (p.size || 50)) || 1;

    const canCreate = isSuperAdmin || hasPermission('USER', 'USER_ADD');

    return (
        <div className="lms-users-page lms-fade-in">
            <div className="lms-premium-card">
                <div className="lms-entity-header">
                    <div className="lms-section-heading">
                        <h1 className="lms-card-title">Users</h1>
                        <span className="lms-section-count">{p.total} users</span>
                    </div>

                    <div className="lms-card-actions">
                        <PerspectiveSwitcher viewMode={viewMode} setViewMode={setViewMode} />
                        {canCreate && (
                            <Button
                                variant="primary"
                                onClick={() => setUi({ ...ui, modal: 'user_create' })}
                                className="lms-btn-primary lms-users-add-btn"
                            >
                                <Icons.Plus s={18} /> ADD USER
                            </Button>
                        )}
                    </div>
                </div>

                <div className="lms-entity-filters">
                    <div className="lms-entity-search">
                        <SearchInput value={searchTerm} onChange={setSearchTerm} placeholder="Filter by user name or email..." />
                    </div>
                    <StatusFilter
                        value={ui.statusFilter}
                        onChange={(v) => setUi({ ...ui, statusFilter: v })}
                    />
                </div>
            </div>

            <div className="lms-container">
                <UserList
                    users={db.users}
                    currentUser={user}
                    userStatusFilter={ui.statusFilter}
                    usersLoading={ui.loading}
                    viewMode={viewMode}
                    hasPermission={hasPermission}
                    setUi={setUi}
                    ui={ui}
                    handleCrud={handleCrud}
                    toggleUserStatus={(u: any) => handleCrud('toggle', 'user', u)}
                />

                <Pagination
                    current={p.page}
                    total={totalPages}
                    totalItems={p.total}
                    itemsPerPage={p.size}
                    onPageChange={(page: number) => changePage('users', page)}
                    onPageSizeChange={(size: number) => changePageSize('users', size)}
                />
            </div>
        </div>
    );
};
