import React, { useState, useEffect, useMemo } from 'react';
import { Icons } from '../shared/components/lms/Icons';
import { GroupList } from '../features/group/components/GroupList';
import { GroupManagementStudio } from '../features/group/components/GroupManagementStudio';
import { GroupAttendance } from '../features/group/components/GroupAttendance';
import { AttendanceLogs } from '../features/group/components/AttendanceLogs';
import { groupApi } from '../features/group/api/groupApi';
import { organizationApi } from '../features/organization/api/organizationApi';
import '../features/group/components/GroupAttendance.css';
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
    groupTab: 'groups' | 'gc' | 'gu' | 'att' | 'att_logs';
    setGroupTab: (t: 'groups' | 'gc' | 'gu' | 'att' | 'att_logs') => void;
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
    previewMedia?: any;
    setPreviewMedia?: (m: any) => void;
}

export const GroupsPage: React.FC<GroupsPageProps> = ({
    db, ui, setUi, user, isSuperAdmin, searchTerm, setSearchTerm, viewMode, setViewMode,
    groupTab, setGroupTab, pagination, changePage, changePageSize, hasPermission, handleCrud,
    openGroupUsers, openGroupCourses, previewMedia, setPreviewMedia
}) => {
    const p = pagination['group'] || { page: 1, size: 50, total: 0 };
    const totalPages = Math.ceil((p.total || 0) / (p.size || 50)) || 1;

    const [studioTarget, setStudioTarget] = useState<any>(null);

    // Attendance specific filters in header
    const [attOrg, setAttOrg] = useState<number | ''>('');
    const [attOrgs, setAttOrgs] = useState<any[]>([]); // Local state for orgs fallback
    const [attGroup, setAttGroup] = useState<number | ''>('');
    const [attCourse, setAttCourse] = useState<number | ''>('');
    const [attStartDate, setAttStartDate] = useState<string>(new Date().toISOString().split('T')[0]);
    const [attEndDate, setAttEndDate] = useState<string>(new Date().toISOString().split('T')[0]);
    const [attStartTime, setAttStartTime] = useState<string>("09:00");
    const [attEndTime, setAttEndTime] = useState<string>("10:00");
    const [attSearch, setAttSearch] = useState('');
    
    // Local search for Management Studio
    const [studioSearch, setStudioSearch] = useState("");

    // Fetch Organizations for SuperAdmin if not available in db
    useEffect(() => {
        const fetchOrgs = async () => {
            if (isSuperAdmin && groupTab.startsWith('att')) {
                try {
                    const res: any = await organizationApi.list('', 1, 1000);
                    const raw: any = res.data?.data || (Array.isArray(res.data) ? res.data : (res.data || res));
                    const list = Array.isArray(raw) ? raw : (raw?.items || raw?.Items || raw?.data || []);
                    if (list.length > 0) setAttOrgs(list);
                } catch (err) {
                    console.error("Manual Org Fetch Failed", err);
                }
            }
        };
        fetchOrgs();
    }, [isSuperAdmin, groupTab]);

    const availableOrgs = useMemo(() => {
        // 1. Check all possible global DB locations
        let list: any[] = (db as any).orgs || (db as any).organizations || (db as any).tenants || [];
        
        // 2. If empty, check our local fetch
        if (list.length === 0) list = attOrgs;
        
        // 3. If STILL empty, add current user's org as emergency fallback
        if (list.length === 0 && user?.tenantId) {
            list = [{ id: user.tenantId, orgName: user.orgName || user.tenantName || 'My Organization' }];
        }
        
        return list;
    }, [db, attOrgs, user]);
    const [attCourses, setAttCourses] = useState<any[]>([]);
    const [attGroups, setAttGroups] = useState<any[]>([]);
    const [attLoading, setAttLoading] = useState(false);

    // Fetch Groups based on Org (for SuperAdmin)
    useEffect(() => {
        const fetchFilteredGroups = async () => {
            if (groupTab.startsWith('att')) {
                setAttLoading(true);
                try {
                    // If SuperAdmin and Org selected, fetch for that org. Else fetch all if possible or use db.
                    if (isSuperAdmin) {
                        const res = await groupApi.getGroups('', 1, 100, attOrg ? Number(attOrg) : null);
                        const raw = res.data?.data || res.data || [];
                        const list = Array.isArray(raw) ? raw : (raw.items || raw.Items || raw.data || []);
                        setAttGroups(list);
                    } else {
                        setAttGroups(db.groups || db.group || []);
                    }
                } catch (err) {
                    console.error("Failed to load groups");
                    setAttGroups([]);
                } finally {
                    setAttLoading(false);
                }
            }
        };
        fetchFilteredGroups();
    }, [attOrg, groupTab, isSuperAdmin, db.groups]);

    useEffect(() => {
        const fetchAttCourses = async () => {
            if (groupTab.startsWith('att') && attGroup) {
                try {
                    const res = await groupApi.getGroupCourses(Number(attGroup), 1, 100);
                    const raw = res.data?.data || res.data || [];
                    const list = Array.isArray(raw) ? raw : (raw.items || raw.Items || raw.data || []);
                    setAttCourses(list);
                } catch (err) {
                    console.error("Failed to load attendance courses");
                    setAttCourses([]);
                }
            } else {
                setAttCourses([]);
            }
        };
        fetchAttCourses();
    }, [attGroup, groupTab]);
    useEffect(() => {
        setAttGroup('');
        setAttCourse('');
    }, [attOrg]);

    useEffect(() => {
        setAttCourse('');
    }, [attGroup]);

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
                        <h1 className="lms-card-title">{groupTab.startsWith('att') ? 'Attendance Management' : 'Groups'}</h1>
                        <span className="lms-section-count lms-groups-section-count">
                            {groupTab.startsWith('att') ? (groupTab === 'att' ? 'Presence Tracking' : 'Activity Logs') : `${p.total} Units`}
                        </span>
                    </div>

                    <div className="lms-card-actions">
                        {groupTab && groupTab.toString().startsWith('att') && (
                            <div className="lms-sub-tabs-container" style={{ display: 'flex', alignItems: 'center', gap: '10px', marginRight: '15px' }}>
                                <div className="lms-sub-tabs">
                                    <button 
                                        className={`lms-sub-tab ${groupTab === 'att_logs' ? 'active' : ''}`}
                                        onClick={() => setGroupTab('att_logs')}
                                    >
                                        Attendance Logs
                                    </button>
                                    <button 
                                        className={`lms-sub-tab ${groupTab === 'att' ? 'active' : ''}`}
                                        onClick={() => setGroupTab('att')}
                                    >
                                        Mark Attendance
                                    </button>
                                </div>
                            </div>
                        )}
                        {groupTab && !groupTab.toString().startsWith('att') && (
                            <>
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
                            </>
                        )}
                    </div>
                </div>

                {groupTab && groupTab.toString().startsWith('att') ? (
                    <div className="lms-att-filters-wrapper lms-fade-in">
                        <div className="lms-att-fields-row" style={{ display: 'flex', gap: '15px', flexWrap: 'wrap', alignItems: 'flex-end' }}>
                            {groupTab === 'att' && (
                                <>
                                    {isSuperAdmin && (
                                        <div className="lms-att-field" style={{ flex: '1 1 200px' }}>
                                            <label className="lms-att-label">Organization</label>
                                            <select 
                                                className="lms-att-select"
                                                value={attOrg}
                                                onChange={(e) => {
                                                    const val = e.target.value;
                                                    setAttOrg(val === "" ? "" : Number(val));
                                                }}
                                            >
                                                <option value="">-- Choose Organization --</option>
                                                {(availableOrgs || []).map((o: any) => (
                                                    <option key={o.id || o.Id} value={o.id || o.Id}>{o.orgName || o.OrgName || o.name || o.Name || 'Unknown Org'}</option>
                                                ))}
                                            </select>
                                        </div>
                                    )}
                                    <div className="lms-att-field" style={{ flex: '1 1 180px' }}>
                                        <label className="lms-att-label">Select Group</label>
                                        <select
                                            className="lms-att-select"
                                            value={attGroup}
                                            onChange={(e) => setAttGroup(Number(e.target.value))}
                                            disabled={isSuperAdmin && !attOrg}
                                        >
                                            <option value="">-- Choose Group --</option>
                                            {attGroups.map((g: any) => (
                                                <option key={g.id || g.Id} value={g.id || g.Id}>{g.groupName || g.GroupName}</option>
                                            ))}
                                        </select>
                                    </div>
                                    <div className="lms-att-field" style={{ flex: '1 1 220px' }}>
                                        <label className="lms-att-label">Select Course</label>
                                        <select
                                            className="lms-att-select"
                                            value={attCourse}
                                            onChange={(e) => setAttCourse(Number(e.target.value))}
                                            disabled={!attGroup}
                                        >
                                            <option value="">-- Choose Course --</option>
                                            {attCourses.map((c: any) => (
                                                <option key={c.courseId || c.id || c.Id} value={c.courseId || c.id || c.Id}>{c.courseName || c.CourseName}</option>
                                            ))}
                                        </select>
                                    </div>
                                    <div className="lms-att-field" style={{ flex: '0 0 150px' }}>
                                        <label className="lms-att-label">Session Date</label>
                                        <input
                                            type="date"
                                            className="lms-att-input"
                                            value={attStartDate}
                                            onChange={(e) => setAttStartDate(e.target.value)}
                                        />
                                    </div>
                                    <div className="lms-att-field" style={{ flex: '0 0 100px' }}>
                                        <label className="lms-att-label">In Time</label>
                                        <input type="time" className="lms-att-input" value={attStartTime} onChange={(e) => setAttStartTime(e.target.value)} />
                                    </div>
                                    <div className="lms-att-field" style={{ flex: '0 0 100px' }}>
                                        <label className="lms-att-label">Out Time</label>
                                        <input type="time" className="lms-att-input" value={attEndTime} onChange={(e) => setAttEndTime(e.target.value)} />
                                    </div>
                                </>
                            )}
                            {groupTab === 'att_logs' && (
                                <div style={{ display: 'flex', gap: '20px', alignItems: 'center', width: '100%', flexWrap: 'wrap' }}>
                                    {isSuperAdmin && (
                                        <div className="lms-att-field" style={{ flex: '0 0 220px' }}>
                                            <label className="lms-att-label">Organization</label>
                                            <select 
                                                className="lms-att-select"
                                                value={attOrg}
                                                onChange={(e) => {
                                                    const val = e.target.value;
                                                    setAttOrg(val === "" ? "" : Number(val));
                                                }}
                                            >
                                                <option value="">All Organizations</option>
                                                {(availableOrgs || []).map((o: any) => (
                                                    <option key={o.id || o.Id} value={o.id || o.Id}>{o.orgName || o.OrgName || o.name || o.Name || 'Unknown Org'}</option>
                                                ))}
                                            </select>
                                        </div>
                                    )}
                                    <div className="lms-att-field" style={{ flex: '0 0 180px' }}>
                                        <label className="lms-att-label">Group</label>
                                        <select
                                            className="lms-att-select"
                                            value={attGroup}
                                            onChange={(e) => setAttGroup(Number(e.target.value))}
                                        >
                                            <option value="">All Groups</option>
                                            {attGroups.map((g: any) => (
                                                <option key={g.id || g.Id} value={g.id || g.Id}>{g.groupName || g.GroupName}</option>
                                            ))}
                                        </select>
                                    </div>
                                    <div className="lms-att-field" style={{ flex: '0 0 200px' }}>
                                        <label className="lms-att-label">Course</label>
                                        <select
                                            className="lms-att-select"
                                            value={attCourse}
                                            onChange={(e) => setAttCourse(Number(e.target.value))}
                                        >
                                            <option value="">All Courses</option>
                                            {attCourses.map((c: any) => (
                                                <option key={c.courseId || c.id || c.Id} value={c.courseId || c.id || c.Id}>{c.courseName || c.CourseName}</option>
                                            ))}
                                        </select>
                                    </div>
                                    <div className="lms-att-search-wrapper" style={{ flex: 1 }}>
                                        <label className="lms-att-label">Search Student</label>
                                        <SearchInput value={attSearch} onChange={setAttSearch} placeholder="Filter logs..." />
                                    </div>
                                </div>
                            )}
                        </div>
                    </div>
                ) : (
                    <div className="lms-entity-filters">
                        <div className="lms-groups-switcher-wrapper">
                            <GroupSwitcher tab={groupTab} setTab={setGroupTab} hasPermission={hasPermission} isSuperAdmin={isSuperAdmin} />
                        </div>
                        <div className="lms-entity-search">
                            <SearchInput value={searchTerm} onChange={setSearchTerm} placeholder="Search groups..." />
                        </div>
                    </div>
                )}
            </div>

            <div className="lms-container lms-groups-container">
                {groupTab === 'att' ? (
                    <GroupAttendance 
                        groups={db.groups} 
                        tenantId={Number(attOrg) || undefined}
                        filters={{ 
                            group: attGroup, 
                            course: attCourse, 
                            startDate: attStartDate, 
                            endDate: attEndDate,
                            startTime: attStartTime,
                            endTime: attEndTime,
                            search: attSearch 
                        }} 
                        onSearchChange={setAttSearch}
                        onClose={() => setGroupTab('groups')} 
                    />
                ) : groupTab === 'att_logs' ? (
                    <AttendanceLogs 
                        isSuperAdmin={isSuperAdmin} 
                        tenantId={Number(attOrg) || undefined}
                        groupId={Number(attGroup) || undefined}
                        courseId={Number(attCourse) || undefined}
                        search={attSearch}
                        setPreviewMedia={setPreviewMedia}
                    />
                ) : (
                    <>
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
                    </>
                )}
            </div>
        </div>
    );
};
