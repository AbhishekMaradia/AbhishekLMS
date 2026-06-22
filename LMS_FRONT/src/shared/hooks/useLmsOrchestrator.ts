import { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { useTheme } from '../../app/providers/ThemeProvider';
import { useAppSelector, useAppDispatch } from '../../store/index';
import { toast } from 'react-toastify';
import { organizationApi } from '../../features/organization/api/organizationApi';
import { userApi } from '../../features/auth/api/userApi';
import { categoryApi } from '../../features/course/api/categoryApi';
import { courseApi } from '../../features/course/api/courseApi';
import { groupApi } from '../../features/group/api/groupApi';
import { securityApi } from '../../features/auth/api/securityApi';
import { subscriptionApi } from '../../features/student/api/subscriptionApi';
import { reportApi } from '../../features/reports/api/reportApi';
import { setCredentials } from '../../features/auth/store/authSlice';

import { useLocation, useNavigate } from 'react-router-dom';

const hexToRgb = (hex: string) => {
    if (!hex) return "118, 49, 33";
    let r = 0, g = 0, b = 0;
    if (hex.length === 4) {
        r = parseInt(hex[1] + hex[1], 16);
        g = parseInt(hex[2] + hex[2], 16);
        b = parseInt(hex[3] + hex[3], 16);
    } else if (hex.length === 7) {
        r = parseInt(hex[1] + hex[2], 16);
        g = parseInt(hex[3] + hex[4], 16);
        b = parseInt(hex[5] + hex[6], 16);
    }
    return `${r}, ${g}, ${b}`;
};


export const useLmsOrchestrator = () => {
    const dispatch = useAppDispatch();
    const location = useLocation();
    const navigate = useNavigate();
    const { theme } = useTheme();
    const { user, isAuthenticated, permissions = {} } = useAppSelector((state: any) => state.auth);

    // UI State Management
    const [searchTerm, setSearchTerm] = useState('');
    const [viewMode, setViewMode] = useState<'table' | 'grid'>('table');
    const [groupTab, setGroupTab] = useState<'groups' | 'gc' | 'gu' | 'att' | 'att_logs'>('groups');

    const lastPathnameRef = useRef(location.pathname);

    const setTab = useCallback((id: string) => {
        const tabToRoute: Record<string, string> = {
            'dash': '/dashboard', 'users': '/users', 'orgs': '/organizations',
            'cat': '/categories', 'curr': '/courses', 'cm': '/media',
            'group': '/groups', 'sec': '/security', 'enroll': '/enrollments',
            'reports': '/reports',
            'att': '/groups',
            'student-dash': '/student/dashboard', 'student-discover': '/student/discover',
            'student-my-courses': '/student/my-courses', 'student-peers': '/student/peers', 'student-reports': '/student/reports'
        };
        const target = tabToRoute[id];
        if (target && location.pathname !== target) {
            navigate(target);
        }

        // Ensure sub-tab sync when clicking from Sidebar
        if (id === 'att') setGroupTab('att_logs');
        if (id === 'group') setGroupTab('groups');

        setTabState(id);
    }, [navigate, location.pathname]);

    useEffect(() => {
        const path = location.pathname.substring(1);
        const routeToTab: Record<string, string> = {
            'dashboard': 'dash', 'users': 'users', 'organizations': 'orgs',
            'categories': 'cat', 'courses': 'curr', 'groups': 'group',
            'media': 'cm', 'security': 'sec', 'enrollments': 'enroll', 'reports': 'reports',
            'student/dashboard': 'student-dash', 'student/discover': 'student-discover',
            'student/my-courses': 'student-my-courses', 'student/peers': 'student-peers', 'student/reports': 'student-reports'
        };
        const pathChanged = lastPathnameRef.current !== location.pathname;
        lastPathnameRef.current = location.pathname;

        if (routeToTab[path]) {
            let targetTab = routeToTab[path];

            // If we are on /groups and the sub-tab is 'att' or 'att_logs', keep sidebar on 'att'
            if (targetTab === 'group' && (groupTab === 'att' || groupTab === 'att_logs')) {
                targetTab = 'att';
            }

            setTabState(targetTab);

            // Reset groupTab ONLY if we are actually CHANGING the main route to /groups 
            // from a different section (not switching sub-tabs or coming from att/att_logs)
            if (pathChanged && targetTab === 'group' && groupTab !== 'att' && groupTab !== 'att_logs') {
                setGroupTab('groups');
            }
        }
    }, [location.pathname, groupTab]);

    const [tab, setTabState] = useState<string>(() => {
        const path = window.location.pathname.replace('/', '');
        return path === 'dashboard' || path === '' ? 'dash' : path;
    });

    // Multi-tenant Context
    const isSuperAdmin = useMemo(() => {
        const roleCode = (user?.roleCode || user?.RoleCode || user?.userRole || user?.UserRole || '').toUpperCase();
        const hasAdminRole = roleCode === 'SUPER_ADMIN' || roleCode === 'SUPERADMIN' || user?.email === 'admin@gmail.com' || user?.email === 'admin@lms.com';

        // A true System SuperAdmin must have NO tenantId (or tenantId 0)
        const utid = user?.tenantId ?? user?.TenantId;
        const isGlobalContext = utid === null || utid === undefined || Number(utid) === 0;

        return hasAdminRole && isGlobalContext;
    }, [user]);

    const isStudent = useMemo(() => {
        // 1. URL Override (for testing)
        const params = new URLSearchParams(window.location.search);
        if (params.get('role') === 'student') return true;

        // 2. Role Name Check
        const r = (user?.roleCode || user?.RoleCode || user?.userRole || user?.UserRole || user?.roleName || user?.RoleName || "").toUpperCase();
        if (r.includes('STUDENT') || r.includes('LEARNER')) return true;

        // 3. Permission Check
        const hasSubscribe = Object.values(permissions || {}).some((pArr: any) =>
            Array.isArray(pArr) && pArr.some(p => {
                const ps = String(p).toUpperCase();
                return ps.includes('SUBSCRIBE') || ps.includes('COURSE_VIEW') === false; // If they can't view standard courses, maybe they are students
            })
        );

        // 4. Exclusion logic
        const isAdmin = r.includes('ADMIN') || r.includes('SUPER') || r.includes('ORG') || r.includes('TENANT');
        const hasGroupId = !!(user?.groupId || user?.GroupId);

        // If they have "SUBSCRIBE" permission specifically, they are likely a student
        const hasExplicitSubscribe = Object.values(permissions || {}).some((pArr: any) =>
            Array.isArray(pArr) && pArr.some(p => String(p).toUpperCase().includes('SUBSCRIBE'))
        );

        // A user is a Student if:
        // - They have ?role=student override
        const hasNoPermissions = !permissions || Object.keys(permissions).length === 0;
        return !isAdmin && (r.includes('STUDENT') || r.includes('LEARNER') || hasExplicitSubscribe || hasGroupId);
    }, [user, permissions]);

    const isUnauthorized = useMemo(() => {
        const r = (user?.roleCode || user?.RoleCode || user?.userRole || user?.UserRole || user?.roleName || user?.RoleName || "");
        const hasNoRole = !r || r.trim() === '';
        const hasNoPermissions = !permissions || Object.keys(permissions).length === 0;
        return hasNoRole && hasNoPermissions;
    }, [user, permissions]);

    const [pagination, setPagination] = useState<any>({
        users: { page: 1, size: 10, total: 0 },
        orgs: { page: 1, size: 10, total: 0 },
        cat: { page: 1, size: 10, total: 0 },
        group: { page: 1, size: 10, total: 0 },
        curr: { page: 1, size: 10, total: 0 },
        cm: { page: 1, size: 10, total: 0 },
        roles: { page: 1, size: 10, total: 0 },
        mods: { page: 1, size: 10, total: 0 },
        perms: { page: 1, size: 10, total: 0 },
        user_roles: { page: 1, size: 10, total: 0 },
        mod_perms: { page: 1, size: 10, total: 0 },
        role_modules: { page: 1, size: 10, total: 0 },
        role_mod_perms: { page: 1, size: 10, total: 0 },
        reports: { page: 1, size: 5, total: 0 },
        enroll: { page: 1, size: 50, total: 0 }
    });

    interface LmsData {
        users: any[]; orgs: any[]; cat: any[]; cats: any[];
        courses: any[]; group: any[]; groups: any[]; sec: any[];
        perms: any[]; modules: any[]; roles: any[];
        enrollments?: any[]; reports?: any[];
    }

    const [db, setDb] = useState<LmsData>({
        users: [], orgs: [], cat: [], cats: [], courses: [], group: [], groups: [], sec: [],
        perms: [], modules: [], roles: [], enrollments: [], reports: []
    });

    const [counts, setCounts] = useState({ orgs: 0, users: 0, cats: 0, groups: 0, courses: 0, roles: 0, enrollments: 0, reports: 0 });

    const [ui, setUi] = useState<any>({
        modal: null, target: null, loading: false, statusFilter: 'all', secTab: 'sec'
    });

    const [subscriptions, setSubscriptions] = useState<number[]>(() => {
        const stored = localStorage.getItem('lms_subscriptions');
        return stored ? JSON.parse(stored) : [];
    });

    const [activeCourse, setActiveCourse] = useState<any>(null);
    const [activeOrg, setActiveOrg] = useState<any>(null);
    const [filters, setFilters] = useState<any>({
        tenantId: '',
        groupId: '',
        dateFrom: '',
        dateTo: ''
    });

    const playCourse = useCallback((course: any) => {
        console.log("[LMS] Launching Player for Course:", course?.title);
        setActiveCourse(course);
    }, []);

    const toggleSubscription = useCallback(async (courseId: number) => {
        if (!courseId) {
            console.error("[LMS] Cannot subscribe: Invalid Course ID", courseId);
            return;
        }

        console.log("[LMS] Toggling Subscription for Course ID:", courseId);
        const isSubbed = subscriptions.includes(courseId);
        setUi((prev: any) => ({ ...prev, loading: true }));
        try {
            if (isSubbed) {
                await subscriptionApi.unsubscribe(courseId);
                const next = subscriptions.filter(id => id !== courseId);
                setSubscriptions(next);
                localStorage.setItem('lms_subscriptions', JSON.stringify(next));
                toast.success("Unsubscribed from course");
            } else {
                await subscriptionApi.subscribe(courseId);
                const next = [...subscriptions, courseId];
                setSubscriptions(next);
                localStorage.setItem('lms_subscriptions', JSON.stringify(next));
                toast.success("Subscribed to course!");
            }
        } catch (e: any) {
            console.error("[LMS] Subscription Error:", e);
            toast.error(e.message || "Subscription update failed");
        } finally {
            setUi((prev: any) => ({ ...prev, loading: false }));
        }
    }, [subscriptions]);

    // Organization Specific Modal State (Studio Integration)
    const [orgEditTab, setOrgEditTab] = useState<'org' | 'admin'>('org');
    const [orgAdminData, setOrgAdminData] = useState<any>(null);

    const fetchOrgAdmin = useCallback(async (org: any) => {
        if (!org) return;
        const id = org.id || org.Id;
        setUi((prev: any) => ({ ...prev, loading: true }));
        try {
            const res = await userApi.getAdminByTenant(id);
            if (res.data.success && res.data.data) {
                setOrgAdminData(res.data.data);
            } else {
                toast.error("Administrator profile fallback node not found.");
            }
        } catch (err: any) {
            toast.error(err.message || "Authority sync failure.");
        } finally {
            setUi((prev: any) => ({ ...prev, loading: false }));
        }
    }, []);

    const hasPermission = useCallback((module: string, action?: string) => {
        if (isSuperAdmin) return true;
        if (!action) return true;

        let currentPerms = permissions || {};
        if (Object.keys(currentPerms).length === 0) {
            const storedPermsStr = localStorage.getItem('permissions');
            if (storedPermsStr) currentPerms = JSON.parse(storedPermsStr);
        }

        const mKey = module.toUpperCase();
        const aKey = action.toUpperCase();

        return currentPerms?.[mKey]?.includes(aKey) || false;
    }, [permissions, isSuperAdmin]);

    const activePageKey = useMemo(() => {
        if (tab !== 'sec') return tab;
        const st = ui.secTab || 'sec';
        const sMap: any = { 'sec': 'roles', 'mod': 'mods', 'perm': 'perms' };
        return sMap[st] || st;
    }, [tab, ui.secTab]);

    // Unified Synchronization Orchestrator
    const fetchData = useCallback(async () => {
        const token = localStorage.getItem('token');
        if (!token) return;

        setUi((prev: any) => ({ ...prev, loading: true }));
        const tid = isSuperAdmin ? undefined : (user?.tenantId ?? user?.TenantId ?? activeOrg?.id ?? activeOrg?.Id ?? 0);

        const applyIsolation = (raw: any[]) => {
            if (isSuperAdmin) return raw;
            const utid = Number(tid);
            return (raw || []).filter((it: any) => {
                const itid = it.tenantId ?? it.TenantId ?? it.organizationId;
                const code = it.code || it.roleCode || '';

                // Allow global items (tenant 0 or null)
                if (!itid || Number(itid) === 0) {
                    // But specifically hide the Super Admin role from the list
                    if (code === 'SUPER_ADMIN') return false;
                    return true;
                }

                // Otherwise only allow if it belongs to the current tenant
                return Number(itid) === utid;
            });
        };

        const extract = (res: any) => {
            const d = res.data?.data || res.data?.Data || (Array.isArray(res.data) ? res.data : (res.data || res));
            return Array.isArray(d) ? d : (d?.items || d?.Items || d?.data || d?.Data || []);
        };

        try {
            if (tab === 'dash') {
                if (isStudent) {
                    // Student Dashboard Fetch
                    const gid = user?.groupId || user?.GroupId;
                    if (gid) {
                        const [gcRes, guRes] = await Promise.all([
                            groupApi.getGroupCourses(gid, 1, 10),
                            groupApi.getGroupUsers(gid)
                        ]);

                        const rawCourses = extract(gcRes);
                        const items = rawCourses.filter(Boolean).map((it: any) => {
                            if (it.course || it.Course) return { ...(it.course || it.Course), ...it };
                            return it;
                        });

                        const rawUsersRes = (guRes.data as any)?.data || guRes.data || [];
                        const rawUsers = Array.isArray(rawUsersRes) ? rawUsersRes : (rawUsersRes.items || rawUsersRes.Items || []);
                        const peerCount = rawUsers.length;

                        setDb((prev: any) => ({ ...prev, courses: items, 'student-dash': items, users: rawUsers }));
                        setCounts(prev => ({
                            ...prev,
                            courses: (gcRes.data as any)?.totalCount || items.length,
                            users: peerCount
                        }));
                    }
                } else {
                    // Admin/SuperAdmin Dashboard Fetch
                    const [o, u, c, g, cs, r, enr, rep] = await Promise.all([
                        isSuperAdmin ? organizationApi.list('', 1, 1) : Promise.resolve({ data: { totalCount: 0 } }),
                        (isSuperAdmin || hasPermission('USER', 'USER_VIEW')) ? userApi.list({ PageNumber: 1, PageSize: 100, TenantId: tid }) : Promise.resolve({ data: { data: [] } }),
                        (isSuperAdmin || hasPermission('CATEGORY', 'CATEGORY_VIEW')) ? categoryApi.list('', 1, 100, tid) : Promise.resolve({ data: { data: [] } }),
                        (isSuperAdmin || hasPermission('GROUP', 'GROUP_VIEW')) ? groupApi.list('', 1, 100, tid) : Promise.resolve({ data: { data: [] } }),
                        (isSuperAdmin || hasPermission('COURSE', 'COURSE_VIEW')) ? courseApi.list('', 1, 100, tid) : Promise.resolve({ data: { data: [] } }),
                        (isSuperAdmin || hasPermission('ROLE', 'ROLE_VIEW')) ? securityApi.getRoles('', 1, 100, undefined, tid) : Promise.resolve({ data: { data: [] } }),
                        (isSuperAdmin || hasPermission('SUBSCRIPTION', 'SUBSCRIPTION_VIEW')) ? subscriptionApi.getList('', 1, 1, tid) : Promise.resolve({ data: { totalCount: 0 } }),
                        (isSuperAdmin || hasPermission('REPORT', 'REPORT_VIEW')) ? reportApi.list('', 1, 1, tid) : Promise.resolve({ data: { totalCount: 0 } })
                    ]);

                    const u_iso = applyIsolation(extract(u));
                    const c_iso = applyIsolation(extract(c));
                    const g_iso = applyIsolation(extract(g));
                    const cs_iso = applyIsolation(extract(cs));
                    const r_iso = applyIsolation(extract(r));

                    setCounts({
                        orgs: isSuperAdmin ? ((o.data as any)?.totalCount || 0) : 0,
                        users: u_iso.length,
                        cats: c_iso.length,
                        groups: g_iso.length,
                        courses: cs_iso.length,
                        roles: r_iso.length,
                        enrollments: (enr.data as any)?.totalCount || 0,
                        reports: (rep.data as any)?.totalCount || 0
                    });
                }
            } else {
                const pageKey = activePageKey;
                const ctx = pagination[pageKey] || { page: 1, size: 10 };
                const page = ctx.page;
                const size = ctx.size;

                if (['users', 'cat', 'group', 'curr', 'cm', 'sec', 'reports'].includes(tab)) {
                    if (isSuperAdmin) {
                        organizationApi.list('', 1, 1000).then(meta => {
                            setDb((prev: any) => ({ ...prev, orgs: extract(meta) }));
                        }).catch(e => console.error("Org Meta Fetch Error", e));
                    } else if (activeOrg) {
                        // For tenant admins, ensure their own org is in the db.orgs for lookup logic
                        setDb((prev: any) => ({ ...prev, orgs: [activeOrg] }));
                    }
                }

                if (['reports', 'users'].includes(tab)) {
                    groupApi.list('', 1, 1000, tid).then(meta => {
                        setDb((prev: any) => ({ ...prev, groups: applyIsolation(extract(meta)) }));
                    }).catch(e => console.error("Group Meta Fetch Error", e));
                }

                let res;
                if (tab === 'orgs') res = await organizationApi.list(searchTerm, page, size);
                else if (tab === 'enroll') {
                    res = await subscriptionApi.getList(searchTerm, page, size, tid);
                    console.log('ENROLL_FETCH_RESULT:', res.data);
                }
                else if (tab === 'reports') {
                    res = await reportApi.list(searchTerm, page, size, filters.tenantId || tid, filters.groupId, filters.dateFrom);
                }
                else if (tab === 'users') {
                    const [uRes, rRes, gRes] = await Promise.all([
                        userApi.list({ SearchTerm: searchTerm, PageNumber: page, PageSize: size, TenantId: tid }),
                        securityApi.getRoles('', 1, 1000, undefined, tid),
                        groupApi.list('', 1, 1000, tid)
                    ]);
                    res = uRes;
                    setDb((prev: any) => ({
                        ...prev,
                        roles: applyIsolation(extract(rRes)),
                        groups: applyIsolation(extract(gRes))
                    }));
                }
                else if (tab === 'sec') {
                    const st = ui.secTab || 'sec';
                    const activeParam = ui.statusFilter === 'all' ? undefined : ui.statusFilter === 'active';

                    if (st === 'sec') res = await securityApi.getRoles(searchTerm, page, size, activeParam, tid);
                    else if (st === 'mod') res = await securityApi.getModules(searchTerm, page, size, activeParam);
                    else if (st === 'perm') res = await securityApi.getPermissions(searchTerm, page, size, activeParam);
                    else if (st === 'user_roles') res = await securityApi.getUserRolesList(searchTerm, page, size, tid, activeParam);
                    else if (st === 'mod_perms') res = await securityApi.getModulePermissionsList(searchTerm, page, size, undefined, undefined, tid);
                    else if (st === 'role_modules') res = await securityApi.getRoleModulesList(searchTerm, page, size, undefined, tid);
                    else if (st === 'role_mod_perms') res = await securityApi.getRoleModulePermissionsList(searchTerm, page, size, tid);

                    const [uMeta, rMeta, mMeta, pMeta] = await Promise.all([
                        userApi.list({ PageNumber: 1, PageSize: 1000, TenantId: tid }),
                        securityApi.getRoles('', 1, 1000, undefined, tid),
                        securityApi.getModules('', 1, 1000, true),
                        securityApi.getPermissions('', 1, 1000, true)
                    ]);
                    setDb((prev: any) => ({
                        ...prev,
                        users: applyIsolation(extract(uMeta)),
                        roles: applyIsolation(extract(rMeta)),
                        modules: extract(mMeta), // Modules are global
                        perms: extract(pMeta) // Permissions are global
                    }));
                }
                else if (tab === 'cat') res = await categoryApi.list(searchTerm, page, size, tid);
                else if (tab === 'group') res = await groupApi.list(searchTerm, page, size, tid);
                else if (tab === 'curr' || tab === 'cm' || tab === 'media') {
                    const [cRes, catMeta] = await Promise.all([
                        courseApi.list(searchTerm, page, size, tid),
                        (isSuperAdmin || hasPermission('CATEGORY', 'CATEGORY_VIEW'))
                            ? categoryApi.list('', 1, 1000, tid).catch(e => { console.warn('Failed to fetch categories list for courses', e); return { data: { data: [] } }; })
                            : Promise.resolve({ data: { data: [] } })
                    ]);
                    res = cRes;
                    setDb((prev: any) => ({ ...prev, cats: applyIsolation(extract(catMeta)) }));
                }

                else if (tab === 'student-discover' || tab === 'student-dash') {
                    if (user?.groupId) {
                        const gcRes = await groupApi.getGroupCourses(user.groupId, page, size);
                        const raw = extract(gcRes);
                        const items = raw.filter(Boolean).map((it: any) => {
                            if (it.course || it.Course) return { ...(it.course || it.Course), ...it };
                            return it;
                        });
                        res = { data: { data: items, totalCount: (gcRes.data as any).totalCount || items.length } };
                    }
                }
                else if (tab === 'student-my-courses') {
                    const mRes = await subscriptionApi.getMyCourses();
                    const raw = extract(mRes);
                    // Flatten if returning subscription objects with nested course data
                    const items = raw.filter(Boolean).map((it: any) => {
                        if (it.course || it.Course) return { ...(it.course || it.Course), ...it };
                        return it;
                    });
                    res = { data: { data: items, totalCount: items.length } };
                }
                else if (tab === 'student-peers') {
                    if (user?.groupId) {
                        res = await groupApi.getGroupUsers(user.groupId);
                    }
                }

                if (res) {
                    const resData = res.data as any;
                    const items = applyIsolation(extract(res));
                    // Smart totalCount extraction: check top-level AND nested data (for PagedApiResponse)
                    const totalCount = resData?.totalCount || resData?.TotalCount ||
                        resData?.data?.totalCount || resData?.data?.TotalCount ||
                        resData?.totalRecords || resData?.TotalRecords ||
                        resData?.data?.totalRecords || resData?.data?.TotalRecords ||
                        items.length;

                    if (tab === 'sec') {
                        const st = ui.secTab || 'sec';
                        const keyMap: any = {
                            'sec': 'roles', 'mod': 'modules', 'perm': 'perms',
                            'user_roles': 'userRoles', 'mod_perms': 'modPerms',
                            'role_modules': 'roleModules', 'role_mod_perms': 'roleModPerms'
                        };
                        const key = keyMap[st] || 'sec';
                        setDb((prev: any) => ({ ...prev, [key]: items, sec: items }));
                    } else {
                        const dbKeyMap: any = {
                            'curr': 'courses', 'cat': 'cats', 'group': 'groups',
                            'cm': 'courses', 'media': 'courses',
                            'student-discover': 'courses', 'student-my-courses': 'courses',
                            'student-peers': 'users', 'enroll': 'enrollments', 'reports': 'reports'
                        };
                        const dbKey = dbKeyMap[tab] || tab;
                        setDb((prev: any) => ({ ...prev, [dbKey]: items, [tab]: items }));
                    }

                    if (pagination[pageKey]?.total !== totalCount) {
                        setPagination((prev: any) => ({
                            ...prev,
                            [pageKey]: { ...(prev[pageKey] || { page: 1, size: 10 }), total: totalCount }
                        }));
                    }
                }
            }
        } catch (err: any) {
            toast.error(`Sync Failure: ${err.message}`);
        } finally {
            setUi((prev: any) => ({ ...prev, loading: false }));
        }
    }, [tab, user, searchTerm, isSuperAdmin, isStudent, ui.secTab, ui.statusFilter, groupTab, activePageKey, pagination, hasPermission, subscriptions, activeOrg, filters]);

    const changePage = useCallback((entity: string, newPage: number) => {
        setPagination((prev: any) => ({
            ...prev,
            [entity]: { ...(prev[entity] || { page: 1, size: 10 }), page: newPage }
        }));
    }, []);

    const changePageSize = useCallback((entity: string, newSize: number) => {
        setPagination((prev: any) => ({
            ...prev,
            [entity]: { page: 1, size: newSize }
        }));
    }, []);


    const [courseTab, setCourseTab] = useState<'details' | 'media'>('details');
    const [formTenantId, setFormTenantId] = useState<number | null>(null);
    const [courseMedia, setCourseMedia] = useState<any>({ vids: [], docs: [], loading: false });
    const [mediaStudioTab, setMediaStudioTab] = useState<'videos' | 'docs'>('videos');
    const [mediaViewMode, setMediaViewMode] = useState<'table' | 'grid'>('table');
    const [editTarget, setEditTarget] = useState<any>(null);
    const [previewMedia, setPreviewMedia] = useState<any>(null);

    const [gcModal, setGcModal] = useState<any>(null);
    const [guModal, setGuModal] = useState<any>(null);

    const [pm, setPm] = useState<any>({ open: false, role: null, module: null, mPerms: [], rPerms: [], loading: false, tenantId: null });
    const [pmSearch, setPmSearch] = useState('');

    const togglePermission = (id: number) => {
        setPm((prev: any) => {
            const has = prev.rPerms.includes(id);
            return {
                ...prev,
                rPerms: has ? prev.rPerms.filter((pid: any) => pid !== id) : [...prev.rPerms, id]
            };
        });
    };

    const openModPM = async (m: any, r: any = null, tId: number | null = null) => {
        setUi((prev: any) => ({ ...prev, modal: null }));
        setPm({ open: true, role: r, module: m, mPerms: [], rPerms: [], loading: true, tenantId: tId });
        try {
            const [mP_Assigned, rM_P] = await Promise.all([
                securityApi.getModulePermissions(m.id || m.Id),
                r ? securityApi.getRolePermissions(r.id || r.Id, m.id || m.Id, tId) : Promise.resolve({ data: [] })
            ]);

            const extract = (res: any) => res.data?.data || res.data || res;
            const assignedToModule = extract(mP_Assigned);

            let mPerms = [];
            let rPerms = [];

            if (!r) {
                mPerms = db.perms && db.perms.length > 0 ? db.perms : [];
                rPerms = assignedToModule.map((p: any) => Number(p.id || p.Id || p.permissionId || p.PermissionId)).filter((id: any) => !isNaN(id));
            } else {
                mPerms = assignedToModule;
                const rawRP = extract(rM_P);
                rPerms = rawRP.map((p: any) => Number(p.id || p.Id || p.permissionId || p.PermissionId || p.IdPermission || p.idPermission)).filter((id: any) => !isNaN(id) && id > 0);
            }

            setPm((prev: any) => ({ ...prev, mPerms, rPerms, loading: false }));
        } catch (err) {
            toast.error("Module context alignment failed.");
            setPm((prev: any) => ({ ...prev, loading: false }));
        }
    };

    const savePermissions = async () => {
        setPm((prev: any) => ({ ...prev, loading: true }));
        try {
            if (pm.role) {
                await securityApi.assignPermissionsToRole(pm.role.id || pm.role.Id, pm.module.id || pm.module.Id, pm.rPerms, pm.tenantId);
                toast.success("Security Policy Synchronized!");
            } else {
                await securityApi.assignModulePermissions(pm.module.id || pm.module.Id, pm.rPerms);
                toast.success("Module Blueprints Updated!");
            }
            setPm((prev: any) => ({ ...prev, open: false, loading: false }));
            fetchData();
        } catch (err: any) {
            toast.error(err.message || "Convergence Failed");
            setPm((prev: any) => ({ ...prev, loading: false }));
        }
    };

    const openGroupUsers = async (group: any) => {
        setGuModal({ open: true, groupName: group.groupName || group.GroupName, groupId: group.id || group.Id, users: [], loading: true, search: "" });
        try {
            const tid = group.tenantId ?? group.TenantId ?? null;
            const [allU, myU] = await Promise.all([
                userApi.list({ PageNumber: 1, PageSize: 1000, TenantId: tid }),
                groupApi.getGroupUsers(group.id || group.Id)
            ]);

            const rawMaster = allU.data?.data || allU.data || [];
            const masterUsersResource = Array.isArray(rawMaster) ? rawMaster : ((rawMaster as any).items || (rawMaster as any).Items || []);

            const rawMy = myU.data?.data || myU.data || [];
            const myUsersGroupResource = Array.isArray(rawMy) ? rawMy : ((rawMy as any).items || (rawMy as any).Items || []);

            const targetGroupId = Number(group.id || group.Id);
            const myIds = new Set(
                myUsersGroupResource
                    .filter((u: any) => Number(u.groupId || u.GroupId) === targetGroupId && u.isActive !== false && u.IsActive !== false)
                    .map((u: any) => Number(u.userId || u.Id || u.id || u.UserId))
                    .filter((id: any) => id > 0 && !isNaN(id))
            );

            const normalized = masterUsersResource.map((u: any) => {
                const uid = Number(u.id || u.Id || u.userId || u.UserId);
                return {
                    userId: uid,
                    firstName: u.firstName || u.FirstName || 'User',
                    lastName: u.lastName || u.LastName || '',
                    email: u.email || u.Email || 'No Email',
                    isAssigned: uid > 0 && myIds.has(uid)
                };
            });
            setGuModal((prev: any) => ({ ...prev, users: normalized, loading: false }));
        } catch (err) {
            console.error("Group Users Sync Error:", err);
            toast.error("Group user synchronization failed.");
            setGuModal(null);
        }
    };

    const toggleUserInGroup = (userId: any) => {
        const targetId = Number(userId);
        setGuModal((prev: any) => {
            if (!prev) return prev;
            return {
                ...prev,
                users: prev.users.map((u: any) => {
                    const uid = Number(u.userId || u.id || u.Id);
                    return uid === targetId ? { ...u, isAssigned: !u.isAssigned } : u;
                })
            };
        });
    };

    const toggleAllVisibleUsers = (select: boolean) => {
        setGuModal((prev: any) => {
            const search = (prev.search || "").toLowerCase();
            return {
                ...prev,
                users: prev.users.map((u: any) => {
                    const nameMatch = (String(u.firstName || "") + " " + String(u.lastName || "")).toLowerCase().includes(search) || String(u.email || "").toLowerCase().includes(search);
                    return nameMatch ? { ...u, isAssigned: select } : u;
                })
            };
        });
    };

    const saveGroupUsers = async () => {
        setGuModal((p: any) => ({ ...p, saving: true }));
        try {
            const userIds = guModal.users.filter((u: any) => u.isAssigned).map((u: any) => u.userId);
            await groupApi.assignUsers({ groupId: guModal.groupId, userIds });
            toast.success("Cohort Membership Updated");
            setGuModal(null);
            fetchData();
        } catch (err: any) {
            toast.error(err.message || "Membership update failed");
            setGuModal((p: any) => ({ ...p, saving: false }));
        }
    };

    const openGroupCourses = async (group: any) => {
        setGcModal({ open: true, groupName: group.groupName || group.GroupName, groupId: group.id || group.Id, courses: [], loading: true, search: "" });
        try {
            const tid = group.tenantId ?? group.TenantId ?? null;
            const [allC, myC] = await Promise.all([
                courseApi.list('', 1, 1000, tid),
                groupApi.getGroupCourses(group.id || group.Id)
            ]);

            const rawMaster = allC.data?.data || allC.data || [];
            const masterCourses = Array.isArray(rawMaster) ? rawMaster : ((rawMaster as any).items || (rawMaster as any).Items || []);

            const rawMy = myC.data?.data || myC.data || [];
            const myCourses = Array.isArray(rawMy) ? rawMy : ((rawMy as any).items || (rawMy as any).Items || []);

            const targetGroupId = Number(group.id || group.Id);
            const myIds = new Set(
                myCourses
                    .filter((c: any) => Number(c.groupId || c.GroupId) === targetGroupId && c.isEnable !== false && c.IsEnable !== false)
                    .map((c: any) => Number(c.courseId || c.Id || c.id || c.CourseId))
                    .filter((id: any) => id > 0 && !isNaN(id))
            );

            const normalized = masterCourses.map((c: any) => {
                const cid = Number(c.id || c.Id || c.courseId || c.CourseId);
                return {
                    courseId: cid,
                    title: c.title || c.courseName || c.Title || c.CourseName,
                    courseName: c.courseName || c.title || c.CourseName || c.Title,
                    categoryName: c.categoryName || c.CategoryName || 'Curriculum',
                    courseTenantId: c.tenantId || c.TenantId,
                    isEnable: cid > 0 && myIds.has(cid)
                };
            });
            setGcModal((prev: any) => ({ ...prev, courses: normalized, loading: false }));
        } catch (err) {
            console.error("Group Courses Sync Error:", err);
            toast.error("Curriculum synchronization failed.");
            setGcModal(null);
        }
    };

    const toggleCourse = (courseId: any) => {
        const targetId = Number(courseId);
        setGcModal((prev: any) => {
            if (!prev) return prev;
            return {
                ...prev,
                courses: prev.courses.map((c: any) => {
                    const cid = Number(c.courseId || c.id || c.Id);
                    return cid === targetId ? { ...c, isEnable: !c.isEnable } : c;
                })
            };
        });
    };

    const toggleAllVisibleCourses = (select: boolean) => {
        setGcModal((prev: any) => {
            const search = (prev.search || "").toLowerCase();
            return {
                ...prev,
                courses: prev.courses.map((c: any) => {
                    const nameMatch = String(c.courseName || c.title || "").toLowerCase().includes(search);
                    return nameMatch ? { ...c, isEnable: select } : c;
                })
            };
        });
    };

    const saveGroupCourses = async () => {
        setGcModal((p: any) => ({ ...p, saving: true }));
        try {
            const courses = gcModal.courses.map((c: any) => ({
                courseId: Number(c.courseId),
                isEnable: c.isEnable
            }));
            await groupApi.bulkUpdateCourses({
                groupId: Number(gcModal.groupId),
                courses
            });
            toast.success("Group Curriculum Updated");
            setGcModal(null);
            fetchData();
        } catch (err: any) {
            toast.error(err.message || "Curriculum update failed");
            setGcModal((p: any) => ({ ...p, saving: false }));
        }
    };

    const syncCourseAssets = useCallback(async (courseId: number) => {
        setCourseMedia((prev: any) => ({ ...prev, loading: true }));
        try {
            const hasVideoView = isSuperAdmin || isStudent || hasPermission('VIDEO', 'VIDEO_VIEW') || (!isStudent && !!user?.tenantId);
            const hasCourseView = isSuperAdmin || isStudent || hasPermission('COURSE', 'COURSE_VIEW') || hasPermission('COURSE', 'COURSE_EDIT') || (!isStudent && !!user?.tenantId);

            const [v, d] = await Promise.all([
                hasVideoView ? courseApi.getVideos(courseId).catch(() => ({ data: { data: [] } })) : Promise.resolve({ data: { data: [] } }),
                hasCourseView ? courseApi.getDocuments(courseId).catch(() => ({ data: { data: [] } })) : Promise.resolve({ data: { data: [] } })
            ]);
            setCourseMedia({
                vids: v.data?.data || v.data || [],
                docs: d.data?.data || d.data || [],
                loading: false
            });
        } catch (err) {
            toast.error("Asset synchronization failure.");
            setCourseMedia((prev: any) => ({ ...prev, loading: false }));
        }
    }, [hasPermission, isSuperAdmin]);

    useEffect(() => {
        const tid = ui.target?.id || ui.target?.Id || ui.target?.courseId || ui.target?.CourseId || ui.target?.tid || ui.target?.tenantId;
        const activeId = activeCourse?.courseId || activeCourse?.CourseId || activeCourse?.id || activeCourse?.Id;

        if ((ui.modal === 'course_edit' || ui.modal === 'cm_studio') && tid) {
            syncCourseAssets(tid);
        } else if (activeId) {
            syncCourseAssets(activeId);
        } else if (ui.modal === 'course_create') {
            setCourseTab('details');
            setCourseMedia({ vids: [], docs: [], loading: false });
        }
    }, [ui.modal, ui.target, activeCourse, syncCourseAssets]);

    const handleMediaUpload = async (e: React.FormEvent<HTMLFormElement>, type: 'vid' | 'doc', forcedId?: number) => {
        e.preventDefault();
        const form = e.currentTarget;
        const tid = forcedId || ui.target?.id || ui.target?.Id || ui.target?.courseId || ui.target?.CourseId || ui.target?.tid || ui.target?.tenantId;

        if (!tid) {
            toast.error("Process aborted: Course context lost.");
            return;
        }

        const fd = new FormData(form);
        setCourseMedia((prev: any) => ({ ...prev, loading: true }));
        try {
            const res = type === 'vid' ? await courseApi.uploadVideo(tid, fd) : await courseApi.uploadDocument(tid, fd);
            const rData = res.data as any;
            if (rData.success || rData.Success) {
                toast.success("Asset successfully deployed to studio");
                syncCourseAssets(tid);
                if (form) form.reset();
            } else {
                toast.error(rData.message || rData.Message || "Deployment failed: Server rejected the asset");
            }
        } catch (err: any) {
            toast.error(err.response?.data?.message || err.message || "Nexus transfer failure");
        } finally {
            setCourseMedia((prev: any) => ({ ...prev, loading: false }));
        }
    };

    const handleMediaEdit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        const form = e.currentTarget;
        const tid = ui.target?.id || ui.target?.Id || ui.target?.courseId || ui.target?.CourseId;
        if (!editTarget || !tid) return;

        const fd = new FormData(form);
        const data = Object.fromEntries(fd.entries());
        setCourseMedia((prev: any) => ({ ...prev, loading: true }));
        try {
            const res = editTarget.type === 'vid'
                ? await courseApi.updateVideo(editTarget.item.id || editTarget.item.Id, data.title as string, data.description as string)
                : await courseApi.updateDocument(editTarget.item.id || editTarget.item.Id, data.docName as string, data.description as string);

            if (res.data.success) {
                toast.success("Asset Updated");
                setEditTarget(null);
                syncCourseAssets(tid);
            }
        } catch (err: any) {
            toast.error(err.message || "Update Failed");
        } finally {
            setCourseMedia((prev: any) => ({ ...prev, loading: false }));
        }
    };

    const handleMediaDelete = async (id: number, type: 'vid' | 'doc') => {
        const tid = ui.target?.id || ui.target?.Id || ui.target?.courseId || ui.target?.CourseId;
        if (!tid) {
            toast.error("Process aborted: Course reference context lost.");
            return;
        }
        if (!window.confirm("Delete this asset permanently?")) {
            return;
        }
        setCourseMedia((prev: any) => ({ ...prev, loading: true }));
        try {
            const res = type === 'vid' ? await courseApi.deleteVideo(id) : await courseApi.deleteDocument(id);
            if (res.data.success) {
                toast.success("Asset Removed");
                syncCourseAssets(tid);
            }
        } catch (err: any) {
            toast.error(err.message || "Deletion Failed");
        } finally {
            setCourseMedia((prev: any) => ({ ...prev, loading: false }));
        }
    };

    // Reset pagination to page 1 on filter or search change to prevent out-of-bounds page requests
    useEffect(() => {
        setPagination((prev: any) => {
            const next = { ...prev };
            Object.keys(next).forEach(key => {
                next[key] = { ...next[key], page: 1 };
            });
            return next;
        });
    }, [searchTerm, ui.statusFilter, ui.secTab, filters]);

    useEffect(() => {
        if (isAuthenticated && isStudent) {
            subscriptionApi.getMyCourses().then(res => {
                const subIds = (res.data.data || []).map((s: any) => Number(s.courseId || s.CourseId || s.id || s.Id));
                setSubscriptions(subIds);
                localStorage.setItem('lms_subscriptions', JSON.stringify(subIds));
            }).catch(err => console.error("Subscription Sync Error", err));
        }
    }, [isAuthenticated, isStudent]);

    // Standardized Trigger system
    useEffect(() => {
        // For students missing a direct tenantId, wait for activeOrg to resolve via group
        const needsTenantResolution = isStudent && !(user?.tenantId ?? user?.TenantId) && !activeOrg;
        if (needsTenantResolution) return;

        fetchData();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [tab, user, searchTerm, ui.secTab, ui.statusFilter, groupTab, pagination[activePageKey]?.page, pagination[activePageKey]?.size, activeOrg, subscriptions, isStudent, filters]);

    // Proactive Media Sync for Course Player
    useEffect(() => {
        const id = Number(activeCourse?.id || activeCourse?.Id || activeCourse?.courseId || activeCourse?.CourseId);
        if (id) {
            console.log("[LMS NEXUS] Syncing Lesson Assets for Hub:", id);
            syncCourseAssets(id);
        }
    }, [activeCourse]);

    // Branding Orchestrator
    useEffect(() => {
        // Determine tenant ID for branding
        const directTid = user?.tenantId ?? user?.TenantId;
        if (isAuthenticated && directTid && Number(directTid) > 0) {
            // Direct tenant association
            organizationApi.getById(Number(directTid)).then(res => {
                const raw = res.data?.data || res.data;
                let org = Array.isArray(raw) ? raw[0] : raw;
                if (org) {
                    if (org.logoUrl && !org.logoUrl.startsWith('http') && !org.logoUrl.includes('/api/')) {
                        // Let SecureImage handle routing
                        org.logoUrl = org.logoUrl;
                    }
                    setActiveOrg(org);
                }
            }).catch(e => console.error("Branding sync failed", e));
        } else if (isStudent && user?.groupId) {
            // Student without direct tenant, derive from group
            const gid = user.groupId ?? user.GroupId;
            groupApi.getById(gid).then(gRes => {
                const grp = gRes.data?.data || gRes.data;
                const tid = grp?.tenantId ?? grp?.TenantId;
                if (tid) {
                    organizationApi.getById(Number(tid)).then(oRes => {
                        const raw = oRes.data?.data || oRes.data;
                        let org = Array.isArray(raw) ? raw[0] : raw;
                        if (org) {
                            if (org.logoUrl && !org.logoUrl.startsWith('http') && !org.logoUrl.includes('/api/')) {
                                // Let SecureImage handle routing
                                org.logoUrl = org.logoUrl;
                            }
                            setActiveOrg(org);
                        }
                    }).catch(e => console.error("Branding sync (org) failed", e));
                } else {
                    setActiveOrg(null);
                }
            }).catch(e => console.error("Branding sync (group) failed", e));
        } else {
            setActiveOrg(null);
        }
    }, [isAuthenticated, user, isStudent]);


    useEffect(() => {
        const root = document.documentElement;
        if (activeOrg && theme !== 'dark') {
            const p = activeOrg.primaryColor || activeOrg.PrimaryColor || '#763121';
            const s = activeOrg.secondaryColor || activeOrg.SecondaryColor || '#4a2118';
            const rgb = hexToRgb(p);

            root.style.setProperty('--color-primary', p);
            root.style.setProperty('--color-primary-rgb', rgb);
            root.style.setProperty('--accent-gradient', `linear-gradient(135deg, ${p} 0%, ${s} 100%)`);
            root.style.setProperty('--color-primary-soft', `rgba(${rgb}, 0.1)`);

            // Apply Dynamic Background only for Tenants
            root.style.setProperty('--color-bg', `rgba(${rgb}, 0.02)`);
            root.style.setProperty('--color-bg-gradient', `linear-gradient(180deg, #ffffff 0%, rgba(${rgb}, 0.04) 100%)`);
            root.style.setProperty('--mesh-bg', `radial-gradient(at 0% 0%, rgba(${rgb}, 0.03) 0px, transparent 50%), radial-gradient(at 100% 100%, rgba(${rgb}, 0.02) 0px, transparent 50%)`);
        } else {
            // Reset to defaults if no org (super admin) or if in DARK MODE (keep common aesthetic)
            root.style.removeProperty('--color-primary');
            root.style.removeProperty('--color-primary-rgb');
            root.style.removeProperty('--accent-gradient');
            root.style.removeProperty('--color-primary-soft');
            root.style.removeProperty('--color-bg');
            root.style.removeProperty('--color-bg-gradient');
            root.style.removeProperty('--mesh-bg');
        }

    }, [activeOrg, theme]);


    const handleCrud = async (action: string, type: string, data: any) => {
        if (action === 'delete') {
            if (!window.confirm(`Are you sure you want to delete this ${type}?`)) {
                return;
            }
        }
        setUi((prev: any) => ({ ...prev, loading: true }));
        try {
            const getId = (d: any) => {
                if (typeof d === 'number' || typeof d === 'string') return d;
                // If it's FormData, we must use .get()
                if (d instanceof FormData) {
                    return d.get('Id') || d.get('id') || d.get('courseId') || d.get('CourseId');
                }
                // Try all common ID variants
                return d?.id || d?.Id || d?.courseId || d?.CourseId || d?.tid || d?.tenantId || d?.organizationId || d?.groupId;
            };

            const id = getId(data) || getId(ui.target);

            const services: any = {
                org: organizationApi, user: userApi, cat: categoryApi, group: groupApi, sec: securityApi,
                course: courseApi, role: securityApi, perm: securityApi, module: securityApi,
                roleModule: securityApi, roleModPerm: securityApi
            };
            const service = services[type];
            let res;

            if (action === 'create') {
                if (type === 'role') res = await securityApi.createRole(data);
                else if (type === 'perm') res = await securityApi.createPermission(data);
                else if (type === 'module') res = await securityApi.createModule(data);
                else if (type === 'roleModule') res = await securityApi.createRoleModule(data);
                else res = await service[type === 'org' ? 'register' : 'create'](data);
            }
            else if (action === 'update') {
                if (type === 'role') res = await securityApi.updateRole(id, data);
                else if (type === 'perm') res = await securityApi.updatePermission(id, data);
                else if (type === 'module') res = await securityApi.updateModule(id, data);
                else res = await service.update(id, data);
            }
            else if (action === 'assign' && type === 'user') res = await service.assign({ userId: Number(id), RoleId: Number(data.RoleId) });
            else if (action === 'delete') {
                if (type === 'role') res = await securityApi.deleteRole(id);
                else if (type === 'perm') res = await securityApi.deletePermission(id);
                else if (type === 'module') res = await securityApi.deleteModule(id);
                else if (type === 'roleModule') res = await securityApi.deleteRoleModule(id);
                else if (type === 'roleModPerm') res = await securityApi.deleteRoleModulePermission(id);
                else res = await service.delete(id);
            }
            else if (action === 'toggle') res = await service.toggleStatus(id);

            if (res) {
                toast.success(res.message || res.data?.message || "Operation successful");
                if (type === 'user' && action === 'update' && user?.id === Number(id)) {
                    const rawData = res.data?.data || res.data;
                    const updatedUser = Array.isArray(rawData) ? rawData[0] : rawData;

                    if (updatedUser) {
                        const mergedUser = { ...user, ...updatedUser };
                        localStorage.setItem('user', JSON.stringify(mergedUser));
                        dispatch(setCredentials({
                            user: mergedUser,
                            token: localStorage.getItem('token') || '',
                            permissions
                        }));
                    }
                }
                if (type === 'org' && action === 'update' && activeOrg?.id === Number(id)) {
                    organizationApi.getById(Number(id)).then(res => {
                        const raw = res.data?.data || res.data;
                        let org = Array.isArray(raw) ? raw[0] : raw;
                        if (org) setActiveOrg(org);
                    }).catch(e => console.error("Active Org Refresh Error", e));
                }

                setUi((prev: any) => ({ ...prev, modal: null, target: null }));
                fetchData();
            }
        } catch (err: any) {
            toast.error(err.response?.data?.message || err.message || "Action Failed");
        } finally {
            setUi((prev: any) => ({ ...prev, loading: false }));
        }
    };

    const revokeEnrollment = async (userId: number, courseId: number) => {
        if (!window.confirm("Are you sure you want to revoke this student's access to this course?")) {
            return;
        }
        setUi((prev: any) => ({ ...prev, loading: true }));
        try {
            const res = await subscriptionApi.revoke(userId, courseId);
            if (res.data.success) {
                toast.success("Academic access revoked successfully");
                fetchData(); // Refresh list
            }
        } catch (err: any) {
            toast.error(err.message || "Convergence revocation failed");
        } finally {
            setUi((prev: any) => ({ ...prev, loading: false }));
        }
    };

    return {
        user, isAuthenticated, isSuperAdmin, permissions,
        tab, setTab, activeCourse, setActiveCourse, playCourse,
        searchTerm, setSearchTerm,
        viewMode, setViewMode, groupTab, setGroupTab,
        db, pagination, counts, ui, setUi,
        orgEditTab, setOrgEditTab, orgAdminData, fetchOrgAdmin,
        courseTab, setCourseTab, formTenantId, setFormTenantId,
        courseMedia, mediaStudioTab, setMediaStudioTab,
        mediaViewMode, setMediaViewMode, editTarget, setEditTarget,
        previewMedia, setPreviewMedia,
        handleMediaUpload, handleMediaEdit, handleMediaDelete,
        hasPermission, fetchData, handleCrud, changePage, changePageSize,
        gcModal, setGcModal, guModal, setGuModal,
        openGroupUsers, toggleUserInGroup, toggleAllVisibleUsers, saveGroupUsers,
        openGroupCourses, toggleCourse, toggleAllVisibleCourses, saveGroupCourses,
        setDb, pm, setPm, pmSearch, setPmSearch, togglePermission, openModPM, savePermissions,
        activeOrg, filters, setFilters,
        isStudent, isUnauthorized, subscriptions, toggleSubscription,
        revokeEnrollment
    };
};

