import React from 'react';
import { Icons } from '../../../shared/components/lms/Icons';
import { securityApi } from '../../auth/api/securityApi';
import { toast } from 'react-toastify';
import { CommonTable, CommonGrid, Pagination, type Column } from '../../../shared/components/lms/LmsComponents';
import '../Security.css';

interface SecurityListProps {
    tab: string;
    db: any;
    secStatusFilter?: 'all' | 'active' | 'inactive';
    modStatusFilter?: 'all' | 'active' | 'inactive';
    permStatusFilter?: 'all' | 'active' | 'inactive';
    userRoleStatusFilter?: 'all' | 'active' | 'inactive';
    user: any;
    viewMode: 'table' | 'grid';
    hasPermission: (module: string, permission: string) => boolean;
    setUi: (val: any) => void;
    ui: any;
    handleCrud: (action: string, type: string, data: any) => void;
    openPM: (r: any) => void;
    openModPM: (m: any) => void;
    editRoleModPerms: (rmp: any) => void;
    deleteRoleModPerm: (id: any) => void;
    deleteUserRole?: (userId: any, roleId: any) => void;
    deleteRoleModule?: (id: any) => void;
    deleteModPerm?: (id: any) => void;
    permissions?: Record<string, string[]>;
    isSuperAdmin?: boolean;
    searchTerm?: string;
    pagination: any;
    changePage: (e: string, p: number) => void;
    changePageSize: (e: string, s: number) => void;
}

export const SecurityList: React.FC<SecurityListProps> = ({
    tab,
    db,
    user,
    viewMode,
    hasPermission,
    setUi,
    ui,
    handleCrud,
    openPM,
    openModPM,
    editRoleModPerms,
    deleteRoleModPerm,
    deleteUserRole,
    deleteRoleModule,
    isSuperAdmin,
    pagination,
    changePage,
    changePageSize
}) => {

    const getFilteredData = () => {
        switch (tab) {
            case 'sec': return db.roles || [];
            case 'mods': return db.modules || [];
            case 'perms': return db.perms || [];
            case 'mod_perms': return db.modPerms || [];
            case 'role_modules': return db.roleModules || [];
            case 'role_mod_perms': return db.roleModPerms || [];
            case 'user_roles': return db.userRoles || [];
            default: return [];
        }
    };

    const data = getFilteredData().filter((item: any) => {
        if (tab === 'user_roles') {
            const uid = item.userId || item.UserId;
            const currentUid = user?.id || user?.Id;
            return Number(uid) !== Number(currentUid);
        }
        return true;
    });

    const columnsMain: Record<string, Column[]> = {
        sec: [
            { header: 'Code', key: 'code' },
            { header: 'Name', key: 'name' },
            { header: 'Org', key: 'orgName', hideOnMobile: true },
            { header: 'Status', key: 'isActive', hideOnMobile: true },
            { header: 'Actions', key: 'actions' }
        ],
        mods: [
            { header: 'Code', key: 'code' },
            { header: 'Name', key: 'name' },
            { header: 'Status', key: 'isActive', hideOnMobile: true },
            { header: 'Actions', key: 'actions' }
        ],
        perms: [
            { header: 'Code', key: 'code' },
            { header: 'Name', key: 'name' },
            { header: 'Status', key: 'isActive', hideOnMobile: true },
            { header: 'Actions', key: 'actions' }
        ],
        mod_perms: [
            { header: 'Module', key: 'moduleName' },
            { header: 'Permission', key: 'permissionName' },
            { header: 'Actions', key: 'actions' }
        ],
        role_modules: [
            { header: 'Role', key: 'roleName' },
            { header: 'Module', key: 'moduleName' },
            { header: 'Actions', key: 'actions' }
        ],
        role_mod_perms: [
            { header: 'Role', key: 'roleName' },
            { header: 'Module', key: 'moduleName' },
            { header: 'Perm', key: 'permissionName', hideOnMobile: true },
            { header: 'Org', key: 'orgName', hideOnMobile: true },
            { header: 'Actions', key: 'actions' }
        ],
        user_roles: [
            { header: 'User', key: 'userEmail' },
            { header: 'Role', key: 'roleName' },
            { header: 'Org', key: 'orgName', hideOnMobile: true },
            { header: 'Status', key: 'isActive', hideOnMobile: true },
            { header: 'Actions', key: 'actions' }
        ]
    };

    const renderTableRow = (item: any) => {
        if (tab === 'sec') return (
            <tr key={item.id}>
                <td><code className="lms-card-code lms-security-table-code">{item.code}</code></td>
                <td><div className="lms-status-title lms-security-status-title">{item.name}</div></td>
                <td className="lms-hide-mobile"><span className="lms-tag info">{item.orgName || db.orgs.find((o: any) => (o.id || o.Id) === (item.tenantId || item.TenantId))?.orgName || 'Super Admin'}</span></td>
                <td className="lms-hide-mobile"><div className={`lms-tag ${(item.isActive ?? item.IsActive) !== false ? 'success' : 'danger'}`}>{(item.isActive ?? item.IsActive) !== false ? 'Active' : 'Inactive'}</div></td>
                <td>
                    <div className="lms-flex-row lms-security-action-group">
                        {(hasPermission('ROLE_MODULE', 'ROLE_MODULE_ADD') || isSuperAdmin) && <button onClick={() => openPM(item)} className="lms-btn accent lms-security-setup-btn"><Icons.Lock s={12} /> SETUP</button>}
                        {(hasPermission('ROLE', 'ROLE_EDIT') || isSuperAdmin) && <button onClick={() => setUi({ ...ui, modal: 'role_edit', target: item })} className="lms-icon-btn-sm info"><Icons.Edit s={16} /></button>}
                        {(hasPermission('ROLE', 'ROLE_DELETE') || isSuperAdmin) && <button onClick={() => handleCrud('delete', 'role', item.id)} className="lms-icon-btn-sm danger"><Icons.Trash s={16} /></button>}
                    </div>
                </td>
            </tr>
        );
        if (tab === 'mods') return (
            <tr key={item.id}>
                <td><code className="lms-card-code lms-security-table-code">{item.code}</code></td>
                <td><div className="lms-status-title lms-security-status-title">{item.name}</div></td>
                <td className="lms-hide-mobile"><div className={`lms-tag ${item.isActive !== false ? 'success' : 'danger'}`}>{item.isActive !== false ? 'Active' : 'Inactive'}</div></td>
                <td>
                    <div className="lms-flex-row lms-security-action-group">
                        {(hasPermission('MODULE', 'MODULE_PERMISSION_ADD') || isSuperAdmin) && <button onClick={() => openModPM(item)} className="lms-icon-btn-sm accent"><Icons.Lock s={16} /></button>}
                        {(hasPermission('MODULE', 'MODULE_EDIT') || isSuperAdmin) && <button onClick={() => setUi({ ...ui, modal: 'module_edit', target: item })} className="lms-icon-btn-sm info"><Icons.Edit s={16} /></button>}
                        {(hasPermission('MODULE', 'MODULE_DELETE') || isSuperAdmin) && <button onClick={() => handleCrud('delete', 'module', item.id)} className="lms-icon-btn-sm danger"><Icons.Trash s={16} /></button>}
                    </div>
                </td>
            </tr>
        );
        if (tab === 'perms') return (
            <tr key={item.id}>
                <td><code className="lms-card-code lms-security-table-code">{item.code}</code></td>
                <td><div className="lms-status-title lms-security-status-title">{item.name}</div></td>
                <td className="lms-hide-mobile"><div className={`lms-tag ${item.isActive !== false ? 'success' : 'danger'}`}>{item.isActive !== false ? 'Active' : 'Inactive'}</div></td>
                <td>
                    <div className="lms-flex-row lms-security-action-group">
                        {(hasPermission('PERMISSION', 'PERMISSION_EDIT') || isSuperAdmin) && <button onClick={() => setUi({ ...ui, modal: 'perm_edit', target: item })} className="lms-icon-btn-sm info"><Icons.Edit s={16} /></button>}
                        {(hasPermission('PERMISSION', 'PERMISSION_DELETE') || isSuperAdmin) && <button onClick={() => handleCrud('delete', 'perm', item.id)} className="lms-icon-btn-sm danger"><Icons.Trash s={16} /></button>}
                    </div>
                </td>
            </tr>
        );
        if (tab === 'mod_perms') return (
            <tr key={item.id}>
                <td><code className="lms-card-code lms-security-table-code">{item.moduleCode}</code><div className="lms-status-sub lms-security-status-sub">{item.moduleName}</div></td>
                <td><code className="lms-card-code lms-security-table-code">{item.permissionCode}</code><div className="lms-status-sub lms-security-status-sub">{item.permissionName}</div></td>
                <td>
                    {(hasPermission('MODULE', 'MODULE_PERMISSION_ADD') || isSuperAdmin) && (
                        <button onClick={() => handleCrud('delete', 'modPerm', { moduleId: item.moduleId, permissionId: item.permissionId })} className="lms-icon-btn-sm danger" title="Delete Alignment"><Icons.Trash s={16} /></button>
                    )}
                </td>
            </tr>
        );
        if (tab === 'role_modules') return (
            <tr key={item.id}>
                <td><div className="lms-flex-row lms-security-role-alignment"><div className="lms-status-icon accent lms-security-role-icon"><Icons.Lock s={13} /></div><div className="lms-status-title lms-security-status-title">{item.roleName}</div></div></td>
                <td><div className="lms-flex-row lms-security-role-alignment"><div className="lms-status-icon info lms-security-role-icon"><Icons.Grid s={13} /></div><div className="lms-status-title lms-security-status-title">{item.moduleName}</div></div></td>
                <td>
                    {(hasPermission('ROLE_MODULE', 'ROLE_MODULE_DELETE') || isSuperAdmin) && (
                        <button onClick={() => deleteRoleModule && deleteRoleModule(item.id || item.Id)} className="lms-icon-btn-sm danger" title="Remove Assignment"><Icons.Trash s={16} /></button>
                    )}
                </td>
            </tr>
        );
        if (tab === 'role_mod_perms') return (
            <tr key={item.id}>
                <td><div className="lms-status-title lms-security-status-title">{item.roleName}</div><code className="lms-card-code lms-security-table-code">{item.roleCode}</code></td>
                <td><div className="lms-status-title lms-security-status-title">{item.moduleName}</div><code className="lms-card-code lms-security-table-code">{item.moduleCode}</code></td>
                <td className="lms-hide-mobile"><code className="lms-card-code lms-security-table-code">{item.permissionCode}</code><div className="lms-status-sub lms-security-status-sub">{item.permissionName}</div></td>
                <td className="lms-hide-mobile"><span className="lms-tag info">{item.orgName || 'Organization'}</span></td>
                <td>
                    <div className="lms-flex-row lms-security-action-group">
                        {(hasPermission('ROLE_MODULE', 'ROLE_MODULE_PERMISSION_ADD') || isSuperAdmin) && (
                            <>
                                <button onClick={() => editRoleModPerms(item)} className="lms-icon-btn-sm info" title="Edit Matrix Assignment"><Icons.Edit s={16} /></button>
                                <button onClick={() => deleteRoleModPerm(item.id)} className="lms-icon-btn-sm danger" title="Delete Matrix Assignment"><Icons.Trash s={16} /></button>
                            </>
                        )}
                    </div>
                </td>
            </tr>
        );
        if (tab === 'user_roles') return (
            <tr key={`${item.userId}_${item.roleId}`}>
                <td><div className="lms-status-title lms-security-status-title">{db.users.find((u: any) => Number(u.id || u.Id) === Number(item.userId))?.firstName || item.userEmail}</div><div className="lms-status-sub lms-security-status-sub">{item.userEmail}</div></td>
                <td><span className="lms-tag accent">{item.roleName}</span></td>
                <td className="lms-hide-mobile"><span className="lms-tag info">{item.orgName || 'Super Admin'}</span></td>
                <td className="lms-hide-mobile"><div className={`lms-tag ${(item.isActive ?? item.IsActive) !== false ? 'success' : 'danger'}`}>{(item.isActive ?? item.IsActive) !== false ? 'Active' : 'Inactive'}</div></td>
                <td>
                    <div className="lms-flex-row lms-security-action-group">
                        {(hasPermission('USER_ROLE', 'USER_ROLE_ADD') || isSuperAdmin) && (
                            <>
                                <button onClick={() => setUi({ ...ui, modal: 'user_role_edit', target: item })} className="lms-icon-btn-sm info" title="Update User Assignment"><Icons.Edit s={16} /></button>
                                <button onClick={() => deleteUserRole && deleteUserRole(item.userId || item.UserId || item.id || item.Id, item.roleId || item.RoleId)} className="lms-icon-btn-sm danger" title="Delete Alignment"><Icons.Trash s={16} /></button>
                            </>
                        )}
                    </div>
                </td>
            </tr>
        );
        return null;
    };

    const renderGridCard = (item: any) => {
        const isActive = (item.isActive ?? item.IsActive) !== false;

        return (
            <div key={item.id || item.userId} className="lms-grid-card lms-fade-in">
                <div className={`lms-grid-banner ${tab === 'sec' ? 'primary' : 'accent'}`}>
                    <div className="lms-grid-overlay" />
                    <div className="lms-status-icon-bg lms-security-icon-scale">
                        {tab === 'sec' ? <Icons.Shield s={28} /> : tab === 'mods' ? <Icons.Grid s={28} /> : <Icons.Lock s={28} />}
                    </div>
                    <div className="lms-grid-badge">
                        <span className={`lms-tag ${isActive ? 'success' : 'danger'}`}>
                            {isActive ? 'ACTIVE' : 'INACTIVE'}
                        </span>
                    </div>
                </div>

                <div className="lms-grid-body">
                    <div className="lms-flex-row lms-security-grid-header">
                        <h3 className="lms-grid-title">{item.name || item.roleName || item.userEmail}</h3>
                    </div>

                    <div className="lms-grid-meta">
                        <Icons.Lock s={12} />
                        <code>{item.code || item.roleCode || item.moduleCode || 'SYSTEM'}</code>
                    </div>

                    <div className="lms-grid-description lms-security-grid-card-desc">
                        {tab === 'sec' ? 'Manage roles and module access.' :
                            tab === 'mods' ? 'System module for settings.' :
                                tab === 'user_roles' ? 'Manage user role assignments.' :
                                    'Security node settings.'}
                    </div>

                    <div className="lms-grid-footer lms-security-grid-footer">
                        <div className="lms-grid-actions">
                            {tab === 'sec' && (hasPermission('ROLE_MODULE', 'ROLE_MODULE_ADD') || isSuperAdmin) && (
                                <button onClick={() => openPM(item)} className="lms-icon-btn-sm accent" title="Permissions">
                                    <Icons.Lock s={16} />
                                </button>
                            )}
                            {tab === 'mods' && (hasPermission('MODULE', 'MODULE_PERMISSION_ADD') || isSuperAdmin) && (
                                <button onClick={() => openModPM(item)} className="lms-icon-btn-sm accent" title="Permissions">
                                    <Icons.Lock s={16} />
                                </button>
                            )}
                            {tab === 'sec' && (hasPermission('ROLE', 'ROLE_EDIT') || isSuperAdmin) && (
                                <button onClick={() => setUi({ ...ui, modal: 'role_edit', target: item })} className="lms-icon-btn-sm info" title="Edit"><Icons.Edit s={16} /></button>
                            )}
                            {tab === 'mods' && (hasPermission('MODULE', 'MODULE_EDIT') || isSuperAdmin) && (
                                <button onClick={() => setUi({ ...ui, modal: 'module_edit', target: item })} className="lms-icon-btn-sm info" title="Edit"><Icons.Edit s={16} /></button>
                            )}
                            {tab === 'perms' && (hasPermission('PERMISSION', 'PERMISSION_EDIT') || isSuperAdmin) && (
                                <button onClick={() => setUi({ ...ui, modal: 'perm_edit', target: item })} className="lms-icon-btn-sm info" title="Edit"><Icons.Edit s={16} /></button>
                            )}
                            {tab === 'user_roles' && (hasPermission('USER_ROLE', 'USER_ROLE_ADD') || isSuperAdmin) && (
                                <button onClick={() => setUi({ ...ui, modal: 'user_role_edit', target: item })} className="lms-icon-btn-sm info" title="Edit"><Icons.Edit s={16} /></button>
                            )}

                            {tab === 'sec' && (hasPermission('ROLE', 'ROLE_DELETE') || isSuperAdmin) && (
                                <button onClick={() => handleCrud('delete', 'role', item.id)} className="lms-icon-btn-sm danger" title="Delete"><Icons.Trash s={16} /></button>
                            )}
                            {tab === 'mods' && (hasPermission('MODULE', 'MODULE_DELETE') || isSuperAdmin) && (
                                <button onClick={() => handleCrud('delete', 'module', item.id)} className="lms-icon-btn-sm danger" title="Delete"><Icons.Trash s={16} /></button>
                            )}
                            {tab === 'perms' && (hasPermission('PERMISSION', 'PERMISSION_DELETE') || isSuperAdmin) && (
                                <button onClick={() => handleCrud('delete', 'perm', item.id)} className="lms-icon-btn-sm danger" title="Delete"><Icons.Trash s={16} /></button>
                            )}
                            {tab === 'user_roles' && (hasPermission('USER_ROLE', 'USER_ROLE_ADD') || isSuperAdmin) && (
                                <button onClick={() => deleteUserRole && deleteUserRole(item.userId || item.UserId, item.roleId || item.RoleId)} className="lms-icon-btn-sm danger" title="Delete"><Icons.Trash s={16} /></button>
                            )}
                        </div>
                        {item.orgName && (
                            <div className="lms-grid-meta lms-security-grid-org">
                                <Icons.Org s={10} /> {item.orgName}
                            </div>
                        )}
                    </div>
                </div>
            </div>
        );
    };

    const pageKey = (() => {
        const keyMap: any = {
            'sec': 'roles', 'mods': 'mods', 'perms': 'perms',
            'user_roles': 'user_roles', 'mod_perms': 'mod_perms',
            'role_modules': 'role_modules', 'role_mod_perms': 'role_mod_perms'
        };
        return keyMap[tab] || 'roles';
    })();

    const p = pagination[pageKey] || { page: 1, size: 10, total: 0 };

    let adjustedTotal = p.total;
    if (tab === 'user_roles') {
        const matchesSearch = !searchTerm || 
            (user?.email || '').toLowerCase().includes(searchTerm.toLowerCase()) || 
            `${user?.firstName || ''} ${user?.lastName || ''}`.toLowerCase().includes(searchTerm.toLowerCase());
        const status = userRoleStatusFilter || ui.statusFilter || 'all';
        const matchesStatus = status === 'all' || status === 'active';
        const isCurrentUserCounted = matchesSearch && matchesStatus;
        adjustedTotal = isCurrentUserCounted ? Math.max(0, p.total - 1) : p.total;
    }
    const totalPages = Math.ceil(adjustedTotal / (p.size || 10)) || 1;

    if (viewMode === 'table') {
        return (
            <>
                <CommonTable
                    headers={columnsMain[tab] || []}
                    empty={data.length === 0}
                >
                    {data.map((item: any) => renderTableRow(item))}
                </CommonTable>

                <Pagination
                    current={p.page}
                    total={totalPages}
                    totalItems={adjustedTotal}
                    itemsPerPage={p.size}
                    onPageChange={(val: number) => changePage(pageKey, val)}
                    onPageSizeChange={(val: number) => changePageSize(pageKey, val)}
                />
            </>
        );
    }

    return (
        <>
            <CommonGrid
                empty={data.length === 0}
            >
                {data.map((item: any) => renderGridCard(item))}
            </CommonGrid>

            <Pagination
                current={p.page}
                total={totalPages}
                totalItems={adjustedTotal}
                itemsPerPage={p.size}
                onPageChange={(val: number) => changePage(pageKey, val)}
                onPageSizeChange={(val: number) => changePageSize(pageKey, val)}
            />
        </>
    );
};
