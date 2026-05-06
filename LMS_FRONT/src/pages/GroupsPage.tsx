import React from 'react';
import { Icons } from '../shared/components/lms/Icons';
import { GroupList } from '../features/group/components/GroupList';
import { GroupManagementStudio } from '../features/group/components/GroupManagementStudio';
import { Pagination, Button, GroupSwitcher, SearchInput, PerspectiveSwitcher } from '../shared/components/lms/LmsComponents';
import '../features/group/Group.css';

interface GroupsPageProps {
    db: any;
    ui: any;
    setUi: (u: any) => void;
    user: any;
    isSuperAdmin: boolean;
    searchTerm: string;
    setSearchTerm: (s: string) => void;
    viewMode: 'table' | 'grid';
    setViewMode: (v: 'table' | 'grid') => void;
    groupTab: 'groups' | 'gc' | 'gu';
    setGroupTab: (t: 'groups' | 'gc' | 'gu') => void;
    pagination: any;
    changePage: (e: string, p: number) => void;
    changePageSize: (e: string, s: number) => void;
    hasPermission: (m: string, a?: string) => boolean;
    handleCrud: (a: string, t: string, d: any) => void;
    // Group Studio Props (from Orchestrator)
    guModal: any;
    setGuModal: (v: any) => void;
    gcModal: any;
    setGcModal: (v: any) => void;
    openGroupUsers: (g: any) => void;
    openGroupCourses: (g: any) => void;
    toggleCourse: (id: number) => void;
    saveGroupCourses: () => Promise<void>;
    toggleUserInGroup: (id: number) => void;
    saveGroupUsers: () => Promise<void>;
}

export const GroupsPage: React.FC<GroupsPageProps> = ({
    db, ui, setUi, user, isSuperAdmin, searchTerm, setSearchTerm, viewMode, setViewMode,
    groupTab, setGroupTab, pagination, changePage, changePageSize, hasPermission, handleCrud,
    openGroupUsers, openGroupCourses
}) => {
    const p = pagination['group'] || { page: 1, size: 50, total: 0 };
    const totalPages = Math.ceil((p.total || 0) / (p.size || 50)) || 1;

    const [studioTarget, setStudioTarget] = React.useState<any>(null);
    const [studioSearch, setStudioSearch] = React.useState("");

    const canCreate = isSuperAdmin || hasPermission('GROUP', 'GROUP_ADD');

    // Studio UI helper
    if (studioTarget) {
        return (
            <GroupManagementStudio
                tab={groupTab as 'gc' | 'gu'}
                target={studioTarget}
                setTarget={setStudioTarget}
                data={{
                    courses: db.groupCourses || [],
                    users: db.groupUsers || [],
                    loading: ui.loading
                }}
                search={studioSearch}
                setSearch={setStudioSearch}
                viewMode={viewMode}
                setViewMode={setViewMode}
                openAssignModal={(g) => groupTab === 'gc' ? openGroupCourses(g) : openGroupUsers(g)}
                removeAction={(id) => handleCrud('remove_item', groupTab === 'gc' ? 'group_course' : 'group_user', { groupId: studioTarget.id || studioTarget.Id, id })}
                getOrgNameByTenant={(tid) => db.orgs?.find((o: any) => Number(o.id || o.Id) === Number(tid))?.orgName || 'Local Node'}
                getTenantId={(g) => g.tenantId || g.TenantId}
                isGlobalTenant={(id) => !id || id === 0}
                db={db}
            />
        );
    }

    return (
        <div className="lms-groups-page lms-fade-in">
            <div className="lms-premium-card lms-groups-header-card">
                <div className="lms-entity-header">
                    <div className="lms-section-heading">
                        <h1 className="lms-card-title">Groups</h1>
                        <span className="lms-section-count lms-groups-section-count">{p.total} Units</span>
                    </div>

                    <div className="lms-card-actions">
                        <PerspectiveSwitcher viewMode={viewMode} setViewMode={setViewMode} />
                        {canCreate && (
                            <Button
                                className="lms-btn-primary lms-groups-add-btn"
                                icon={Icons.Plus}
                                onClick={() => setUi({ ...ui, modal: 'group_create' })}
                            >
                                ADD GROUP
                            </Button>
                        )}
                    </div>
                </div>

                <div className="lms-entity-filters">
                    <div className="lms-groups-switcher-wrapper">
                        <GroupSwitcher tab={groupTab} setTab={setGroupTab} hasPermission={hasPermission} isSuperAdmin={isSuperAdmin} />
                    </div>
                    <div className="lms-entity-search">
                        <SearchInput value={searchTerm} onChange={setSearchTerm} placeholder="Search groups..." />
                    </div>
                </div>
            </div>

            <div className="lms-container lms-groups-container">
                <GroupList
                    groups={db.groups || db.group}
                    orgs={db.orgs}
                    viewMode={viewMode}
                    groupTab={groupTab}
                    hasPermission={hasPermission}
                    setUi={setUi} ui={ui}
                    handleCrud={handleCrud}
                    user={user}
                    isSuperAdmin={isSuperAdmin}
                    openGroupUsers={openGroupUsers}
                    openGroupCourses={openGroupCourses}
                    fetchGroupCoursesInline={(g) => {
                        setStudioTarget(g);
                        handleCrud('fetch_group_courses', 'group', g);
                    }}
                    fetchGroupUsersInline={(g) => {
                        setStudioTarget(g);
                        handleCrud('fetch_group_users', 'group', g);
                    }}
                />
                <Pagination
                    current={p.page}
                    total={totalPages}
                    totalItems={p.total}
                    itemsPerPage={p.size}
                    onPageChange={(page: number) => changePage('group', page)}
                    onPageSizeChange={(size: number) => changePageSize('group', size)}
                />
            </div>
        </div>
    );
};
