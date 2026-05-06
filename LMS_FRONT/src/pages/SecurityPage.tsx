import React, { useState } from 'react';
import { SecuritySwitcher, PerspectiveSwitcher, Button, SearchInput, StatusFilter } from '../shared/components/lms/LmsComponents';
import { securityApi } from '../features/auth/api/securityApi';
import { SecurityList } from '../features/security/components/SecurityList';
import { Icons } from '../shared/components/lms/Icons';
import { toast } from 'react-toastify';
import { useAppSelector } from '../store/index';
import '../features/security/Security.css';

interface SecurityPageProps {
    db: any;
    user: any;
    isSuperAdmin: boolean;
    hasPermission: (m: string, a: string) => boolean;
    handleCrud: (a: string, t: string, d: any) => Promise<void>;
    ui: any;
    setUi: (u: any) => void;
    fetchData: () => void;
    searchTerm: string;
    setSearchTerm: (s: string) => void;
    pm: any;
    setPm: (v: any) => void;
    pmSearch: string;
    setPmSearch: (s: string) => void;
    togglePermission: (id: number) => void;
    openModPM: (m: any, r?: any, tId?: number | null) => Promise<void>;
    savePermissions: () => Promise<void>;
    pagination: any;
    changePage: (e: string, p: number) => void;
    changePageSize: (e: string, s: number) => void;
}

export const SecurityPage: React.FC<SecurityPageProps> = ({
    db, user, isSuperAdmin, hasPermission, handleCrud, ui, setUi, fetchData, searchTerm, setSearchTerm,
    pm, setPm, pmSearch, setPmSearch, togglePermission, openModPM, savePermissions,
    pagination, changePage, changePageSize
}) => {
    const [viewMode, setViewMode] = useState<'table' | 'grid'>('table');
    const { permissions } = useAppSelector((state: any) => state.auth);

    const tab = ui.secTab || 'sec';

    return (
        <div className="lms-security-page lms-fade-in">
            <div className="lms-premium-card lms-security-header-card">
                <div className="lms-entity-header">
                    <div className="lms-section-heading">
                        <h1 className="lms-card-title">Security</h1>
                        <span className="lms-section-count">{db.roles?.length || 0} records</span>
                    </div>

                    <div className="lms-card-actions">
                        <PerspectiveSwitcher viewMode={viewMode} setViewMode={setViewMode} />
                        <div className="lms-flex-row lms-security-actions-wrapper">
                            {tab === 'sec' && (hasPermission('ROLE', 'ROLE_ADD') || isSuperAdmin) && (
                                <Button className="lms-security-create-btn" icon={Icons.Plus} onClick={() => setUi({ ...ui, modal: 'role_create' })}>CREATE ROLE</Button>
                            )}
                            {tab === 'mod' && (hasPermission('MODULE', 'MODULE_ADD') || isSuperAdmin) && (
                                <Button className="lms-security-create-btn" icon={Icons.Plus} onClick={() => setUi({ ...ui, modal: 'module_create' })}>CREATE MODULE</Button>
                            )}
                            {tab === 'perm' && (hasPermission('PERMISSION', 'PERMISSION_ADD') || isSuperAdmin) && (
                                <Button className="lms-security-create-btn" icon={Icons.Plus} onClick={() => setUi({ ...ui, modal: 'perm_create' })}>CREATE PERMISSION</Button>
                            )}
                            {tab === 'mod_perms' && (hasPermission('MODULE', 'MODULE_PERMISSION_ADD') || isSuperAdmin) && (
                                <Button className="lms-security-create-btn" icon={Icons.Plus} onClick={() => setUi({ ...ui, modal: 'mod_perm_assign' })}>ADD MOD-PERMS</Button>
                            )}
                            {tab === 'role_mod_perms' && (hasPermission('ROLE_MODULE', 'ROLE_MODULE_PERMISSION_ADD') || isSuperAdmin) && (
                                <Button className="lms-security-create-btn" icon={Icons.Plus} onClick={() => setUi({ ...ui, modal: 'role_mod_assign' })}>ADD TO ROLE</Button>
                            )}
                            {tab === 'role_modules' && (hasPermission('ROLE_MODULE', 'ROLE_MODULE_ADD') || isSuperAdmin) && (
                                <Button className="lms-security-create-btn" icon={Icons.Plus} onClick={() => setUi({ ...ui, modal: 'role_mod_assign' })}>ADD MODULE</Button>
                            )}
                            {tab === 'user_roles' && (hasPermission('USER_ROLE', 'USER_ROLE_ADD') || isSuperAdmin) && (

                                <Button className="lms-security-create-btn" icon={Icons.Plus} onClick={() => setUi({ ...ui, modal: 'user_role_assign' })}>ADD TO USER</Button>
                            )}
                        </div>
                    </div>
                </div>

                <SecuritySwitcher
                    tab={tab}
                    setTab={(t: any) => setUi({ ...ui, secTab: t })}
                    hasPermission={hasPermission}
                    isSuperAdmin={isSuperAdmin}
                />

                <div className="lms-entity-filters">
                    <div className="lms-entity-search">
                        <SearchInput value={searchTerm || ''} onChange={(v: string) => setSearchTerm(v)} placeholder="Search security..." />
                    </div>
                    {!['mod_perms', 'role_modules', 'role_mod_perms'].includes(tab) && (
                        <StatusFilter
                            value={ui.statusFilter}
                            onChange={(v) => setUi({ ...ui, statusFilter: v })}
                        />
                    )}
                </div>
            </div>

            <div className="lms-container">
                <SecurityList
                    tab={tab === 'mod' ? 'mods' : tab === 'perm' ? 'perms' : tab}
                    db={db}
                    user={user}
                    secStatusFilter={ui.statusFilter}
                    modStatusFilter={ui.statusFilter}
                    permStatusFilter={ui.statusFilter}
                    userRoleStatusFilter={ui.statusFilter}
                    viewMode={viewMode}
                    hasPermission={hasPermission}
                    setUi={setUi}
                    ui={ui}
                    handleCrud={handleCrud}
                    searchTerm={searchTerm}
                    openPM={(r: any) => setUi({ ...ui, modal: 'role_mod_assign', target: r })}
                    openModPM={(m: any) => setUi({ ...ui, modal: 'mod_perm_assign', target: m })}
                    editRoleModPerms={(rmp: any) => setUi({ ...ui, modal: 'role_mod_perm_assign', target: rmp })}
                    deleteRoleModPerm={(id: any) => handleCrud('delete', 'roleModPerm', id)}
                    deleteUserRole={async (userId: any, roleId: any) => {
                        try {
                            const res = await securityApi.removeUserRole(userId, roleId);
                            if (res) { toast.success("User role assignment removed!"); fetchData(); }
                        } catch (err: any) { toast.error(err.message || "Removal failed"); }
                    }}
                    deleteRoleModule={async (id: any) => {
                        try {
                            const res = await securityApi.deleteRoleModule(id);
                            if (res) { toast.success("Role module assignment removed!"); fetchData(); }
                        } catch (err: any) { toast.error(err.message || "Removal failed"); }
                    }}
                    deleteModPerm={async (id: any) => {
                        try {
                            const res = await securityApi.deletePermission(id);
                            if (res) { toast.success("Module permission removed!"); fetchData(); }
                        } catch (err: any) { toast.error(err.message || "Removal failed"); }
                    }}
                    isSuperAdmin={isSuperAdmin}
                    pagination={pagination}
                    changePage={changePage}
                    changePageSize={changePageSize}
                />
            </div>
        </div>
    );
};
