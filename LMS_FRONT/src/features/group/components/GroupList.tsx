import React from 'react';
import { Icons } from '../../../shared/components/lms/Icons';
import { CommonTable, CommonGrid, type Column } from '../../../shared/components/lms/LmsComponents';
import '../Group.css';

interface GroupListProps {
    groups: any[];
    orgs: any[];
    viewMode: 'table' | 'grid';
    hasPermission: (module: string, permission?: string) => boolean;
    setUi: (val: any) => void;
    ui: any;
    handleCrud: (action: string, type: string, data: any) => void;
    groupTab: string;
    fetchGroupCoursesInline?: (g: any) => void;
    fetchGroupUsersInline?: (g: any) => void;
    openGroupCourses?: (g: any) => void;
    openGroupUsers?: (g: any, e: any) => void;
    user: any;
    isSuperAdmin: boolean;
    statusFilter?: string;
}

export const GroupList: React.FC<GroupListProps> = ({
    groups,
    orgs,
    viewMode,
    hasPermission,
    setUi,
    ui,
    handleCrud,
    groupTab,
    openGroupCourses,
    openGroupUsers,
    user,
    isSuperAdmin
}) => {
    const filteredGroups = groups || [];

    const getOrgName = (g: any) => {
        if (g.orgName || g.OrgName) return g.orgName || g.OrgName;
        const tid = g.tenantId || g.TenantId;
        if (!tid || tid === 0) return 'Super Admin';
        const org = (orgs || []).find((o: any) => Number(o.id || o.Id) === Number(tid));
        if (org) return org.orgName || org.Name || org.OrgName;
        if (!isSuperAdmin && (user?.orgName || user?.OrgName)) {
            return user.orgName || user.OrgName;
        }
        return 'SUPER ADMIN';
    };

    const getHeaders = (): Column[] => {
        const base: Column[] = [
            { header: 'Group', key: 'name' },
            { header: 'Organization', key: 'org', hideOnMobile: true }
        ];
        if (groupTab === 'groups') {
            base.push({ header: 'Actions', key: 'actions', className: 'lms-text-center' });
        } else {
            base.push({ header: 'Control', key: 'assign', className: 'lms-text-center' });
        }
        return base;
    };

    if (viewMode === 'table') {
        return (
            <CommonTable
                headers={getHeaders()}
                loading={ui.loading}
                empty={filteredGroups.length === 0}
            >
                {filteredGroups.map(g => {
                    const id = g.id || g.Id;
                    return (
                        <tr key={id}>
                            <td className="lms-gl-td-center">
                                <div className="lms-cell-bold lms-gl-cell-bold">{g.groupName || g.GroupName}</div>
                            </td>
                            <td className="lms-hide-mobile lms-gl-td-center">
                                <span className="lms-tag info lms-gl-tag-bold">{getOrgName(g)}</span>
                            </td>
                            {groupTab === 'groups' && (
                                <td className="lms-gl-td-center">
                                    <div className="lms-cell-actions lms-cl-actions-left">
                                        {hasPermission('GROUP', 'GROUP_EDIT') && (
                                            <button onClick={() => setUi({ ...ui, modal: 'group_edit', target: g })} className="lms-icon-btn-sm info" title="Edit">
                                                <Icons.Edit s={16} />
                                            </button>
                                        )}
                                        {hasPermission('GROUP', 'GROUP_DELETE') && (
                                            <button onClick={() => handleCrud('delete', 'group', g)} className="lms-icon-btn-sm danger" title="Delete">
                                                <Icons.Trash s={16} />
                                            </button>
                                        )}
                                    </div>
                                </td>
                            )}
                            {groupTab !== 'groups' && (
                                <td className="lms-gl-td-center">
                                    <div className="lms-cell-actions lms-cl-actions-left">
                                        {groupTab === 'gc' ? (
                                            (hasPermission('GROUP', 'GROUP_COURSE_EDIT') || isSuperAdmin) && (
                                                <button onClick={() => openGroupCourses?.(g)} className="lms-btn-pill-sm info solid">Manage</button>
                                            )
                                        ) : (
                                            (hasPermission('GROUP', 'GROUP_USER_EDIT') || isSuperAdmin) && (
                                                <button onClick={(e) => openGroupUsers?.(g, e)} className="lms-btn-pill-sm accent solid">Manage</button>
                                            )
                                        )}
                                    </div>
                                </td>
                            )}
                        </tr>
                    );
                })}
            </CommonTable>
        );
    }

    return (
        <CommonGrid
            loading={ui.loading}
            empty={filteredGroups.length === 0}
        >
            {filteredGroups.map((g: any) => {
                const id = g.id || g.Id;
                const orgName = getOrgName(g);
                const isActive = g.isActive !== false;

                return (
                    <div key={id} className="lms-grid-card lms-fade-in">
                        <div className={`lms-grid-banner ${groupTab === 'gc' ? 'accent' : 'primary'}`}>
                            <div className="lms-grid-overlay" />
                            <div className="lms-status-icon-bg lms-gl-icon-bg">
                                {groupTab === 'gc' ? <Icons.Book s={28} /> : groupTab === 'gu' ? <Icons.Users s={28} /> : <Icons.Groups s={28} />}
                            </div>
                            <div className="lms-grid-badge">
                                <span className={`lms-tag ${isActive ? 'success' : 'danger'}`}>
                                    {isActive ? 'ACTIVE' : 'INACTIVE'}
                                </span>
                            </div>
                        </div>

                        <div className="lms-grid-body">
                            <h3 className="lms-grid-title">{g.groupName || g.GroupName}</h3>

                            <div className="lms-grid-meta">
                                <Icons.Org s={12} />
                                <span>{orgName}</span>
                            </div>

                            <div className="lms-grid-description lms-gl-grid-desc">
                                {groupTab === 'gc' ? 'Link courses to this group.' :
                                    groupTab === 'gu' ? 'Link members to this group.' :
                                        'Settings and members.'}
                            </div>

                            <div className="lms-grid-footer lms-gl-grid-footer">
                                <div className="lms-grid-meta lms-gl-grid-meta">
                                    <Icons.Clock s={10} /> CREATED
                                </div>
                                <div className="lms-grid-actions">
                                    {groupTab === 'groups' ? (
                                        <>
                                            {hasPermission('GROUP', 'GROUP_EDIT') && (
                                                <button onClick={() => setUi({ ...ui, modal: 'group_edit', target: g })} className="lms-icon-btn-sm info" title="Edit">
                                                    <Icons.Edit s={16} />
                                                </button>
                                            )}
                                            {hasPermission('GROUP', 'GROUP_DELETE') && (
                                                <button onClick={() => handleCrud('delete', 'group', g)} className="lms-icon-btn-sm danger" title="Delete">
                                                    <Icons.Trash s={16} />
                                                </button>
                                            )}
                                        </>
                                    ) : (
                                        <>
                                            {groupTab === 'gc' ? (
                                                (hasPermission('GROUP', 'GROUP_COURSE_EDIT') || isSuperAdmin) && (
                                                    <button onClick={() => openGroupCourses?.(g)} className="lms-btn accent solid lms-gl-btn-assign">
                                                        <Icons.Plus s={14} /> MANAGE
                                                    </button>
                                                )
                                            ) : (
                                                (hasPermission('GROUP', 'GROUP_USER_EDIT') || isSuperAdmin) && (
                                                    <button onClick={(e) => openGroupUsers?.(g, e)} className="lms-btn accent solid lms-gl-btn-assign">
                                                        <Icons.Plus s={14} /> MANAGE
                                                    </button>
                                                )
                                            )}
                                        </>
                                    )}
                                </div>
                            </div>
                        </div>
                    </div>
                );
            })}
        </CommonGrid>
    );
};
