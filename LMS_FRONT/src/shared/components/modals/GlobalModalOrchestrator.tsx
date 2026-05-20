import React from 'react';
import { UserModal } from '../../../features/auth/components/UserModal';
import { OrganizationModal } from '../../../features/organization/components/OrganizationModal';
import { CategoryModal } from '../../../features/course/components/CategoryModal';
import { CourseModal } from '../../../features/course/components/CourseModal';
import { GroupModal } from '../../../features/group/components/GroupModal';
import { GroupAssignModal } from '../../../features/group/components/GroupAssignModal';
import { CourseMediaStudio } from '../../../features/course/components/CourseMediaStudio';
import { UserRoleModal } from '../../../features/security/components/UserRoleModal';
import { SecurityModal } from '../../../features/security/components/SecurityModal';
import { PermissionMappingModal } from '../../../features/security/components/PermissionMappingModal';
import { PermissionMatrix } from '../../../features/security/components/PermissionMatrix';
import { StudentCoursePreview } from '../../../features/student/components/StudentCoursePreview';
import { SecureMediaViewer } from '../SecureMediaViewer';
import { securityApi } from '../../../features/auth/api/securityApi';
import { Icons } from '../lms/Icons';
import { toast } from 'react-toastify';

interface GlobalModalOrchestratorProps {
    ui: any;
    setUi: (u: any) => void;
    db: any;
    user: any;
    isSuperAdmin: boolean;
    handleCrud: (a: string, t: string, d: any) => Promise<void>;
    hasPermission: (m: string, a: string) => boolean;
    // Organization Specific
    orgEditTab: 'org' | 'admin';
    setOrgEditTab: React.Dispatch<React.SetStateAction<'org' | 'admin'>>;
    orgAdminData: any;
    fetchOrgAdmin: (org: any) => void;
    // Course Specific
    courseTab: 'details' | 'media';
    setCourseTab: React.Dispatch<React.SetStateAction<'details' | 'media'>>;
    formTenantId: number | null;
    setFormTenantId: React.Dispatch<React.SetStateAction<number | null>>;
    courseMedia: any;
    mediaStudioTab: 'videos' | 'docs';
    setMediaStudioTab: React.Dispatch<React.SetStateAction<'videos' | 'docs'>>;
    mediaViewMode: 'table' | 'grid';
    setMediaViewMode: React.Dispatch<React.SetStateAction<'table' | 'grid'>>;
    editTarget: any;
    setEditTarget: React.Dispatch<React.SetStateAction<any>>;
    handleMediaUpload: (e: React.FormEvent<HTMLFormElement>, type: 'vid' | 'doc') => Promise<void>;
    handleMediaEdit: (e: React.FormEvent<HTMLFormElement>) => Promise<void>;
    handleMediaDelete: (id: number, type: 'vid' | 'doc') => Promise<void>;
    previewMedia: any;
    setPreviewMedia: (m: any) => void;
    // Group Assignment Specific
    gcModal: any;
    setGcModal: (v: any) => void;
    guModal: any;
    setGuModal: (v: any) => void;
    toggleCourse: (id: number) => void;
    saveGroupCourses: () => Promise<void>;
    toggleUserInGroup: (id: number) => void;
    toggleAllVisibleUsers: (select: boolean) => void;
    saveGroupUsers: () => Promise<void>;
    toggleAllVisibleCourses: (select: boolean, category?: string) => void;
    fetchData: () => Promise<void>;
    setDb: React.Dispatch<React.SetStateAction<any>>;
    // Student Perspective
    subscriptions: number[];
    playCourse: (course: any) => void;
    toggleSubscription: (id: number) => Promise<void>;
    // Security Specific
    pm: any;
    setPm: React.Dispatch<React.SetStateAction<any>>;
    pmSearch: string;
    setPmSearch: React.Dispatch<React.SetStateAction<string>>;
    togglePermission: (id: number) => void;
    openModPM: (m: any, r?: any, tId?: number | null) => Promise<void>;
    savePermissions: () => Promise<void>;
    permissions: any;
}

export const GlobalModalOrchestrator: React.FC<GlobalModalOrchestratorProps> = ({
    ui, setUi, db, user, isSuperAdmin, handleCrud, hasPermission,
    orgEditTab, setOrgEditTab, orgAdminData, fetchOrgAdmin,
    courseTab, setCourseTab, formTenantId, setFormTenantId,
    courseMedia, mediaStudioTab, setMediaStudioTab,
    mediaViewMode, setMediaViewMode, editTarget, setEditTarget,
    handleMediaUpload, handleMediaEdit, handleMediaDelete,
    previewMedia, setPreviewMedia,
    gcModal, setGcModal, guModal, setGuModal,
    toggleCourse, saveGroupCourses, toggleUserInGroup, saveGroupUsers,
    toggleAllVisibleCourses, toggleAllVisibleUsers,
    fetchData, setDb,
    subscriptions, playCourse, toggleSubscription,
    pm, setPm, pmSearch, setPmSearch, togglePermission, openModPM, savePermissions, permissions
}) => {
    console.log('[GMO] rendered. previewMedia=', previewMedia, 'user=', user);

    React.useEffect(() => {
        const handleEsc = (e: KeyboardEvent) => {
            if (e.key === 'Escape') closeModal();
        };
        window.addEventListener('keydown', handleEsc);
        return () => window.removeEventListener('keydown', handleEsc);
    }, [ui]);
    
    const closeModal = () => setUi({ ...ui, modal: null, target: null });

    const renderModalContent = () => {
        if (!ui.modal) return null;
        const type = ui.modal.split('_')[0];

        if (type === 'user') {
            if (ui.modal.startsWith('user_role')) return <UserRoleModal ui={ui} user={user} isSuperAdmin={isSuperAdmin} db={db} handleCrud={handleCrud} securityApi={securityApi} extractData={(res: any) => res.data?.data || res.data || res} toast={toast} syncUserRoles={fetchData} setUi={setUi} />;
            return <UserModal ui={ui} user={user} isSuperAdmin={isSuperAdmin} db={db} formTenantId={formTenantId} setFormTenantId={setFormTenantId} handleCrud={handleCrud} hasPermission={hasPermission} />;
        }
        if (type === 'org') return <OrganizationModal ui={ui} user={user} isSuperAdmin={isSuperAdmin} db={db} orgEditTab={orgEditTab} setOrgEditTab={setOrgEditTab} orgAdminData={orgAdminData} handleCrud={handleCrud} fetchOrgAdmin={fetchOrgAdmin} />;
        if (type === 'cat') return <CategoryModal ui={ui} user={user} isSuperAdmin={isSuperAdmin} db={db} handleCrud={handleCrud} />;
        if (type === 'course') return <CourseModal ui={ui} user={user} isSuperAdmin={isSuperAdmin} db={db} cats={(db.cats && db.cats.length > 0) ? db.cats : (db.cat || [])} handleCrud={handleCrud} courseTab={courseTab} setCourseTab={setCourseTab} courseMedia={courseMedia} mediaStudioTab={mediaStudioTab} setMediaStudioTab={setMediaStudioTab} mediaViewMode={mediaViewMode} setMediaViewMode={setMediaViewMode} editTarget={editTarget} setEditTarget={setEditTarget} handleMediaUpload={handleMediaUpload} handleMediaEdit={handleMediaEdit} handleMediaDelete={handleMediaDelete} setPreviewMedia={setPreviewMedia} hasPermission={hasPermission} />;
        if (type === 'group') return <GroupModal ui={ui} user={user} isSuperAdmin={isSuperAdmin} db={db} handleCrud={handleCrud} />;
        if (type === 'cm') return <CourseMediaStudio course={ui.target} cmStudioTab={mediaStudioTab} setCmStudioTab={setMediaStudioTab} cmMedia={courseMedia} setPreviewMedia={setPreviewMedia} setCmTarget={(t: any) => setUi({ ...ui, target: t })} setCmMedia={() => {}} handleCmDelete={handleMediaDelete} handleCmUpload={handleMediaUpload} handleCmEdit={handleMediaEdit} cmEditTarget={editTarget} setCmEditTarget={setEditTarget} mediaViewMode={mediaViewMode} setMediaViewMode={setMediaViewMode} user={user} isSuperAdmin={isSuperAdmin} orgs={db.orgs} isModalView={true} hasPermission={hasPermission} />;
        
        if (ui.modal === 'student_course_preview') {
            const isSubscribed = subscriptions.includes(Number(ui.target?.courseId || ui.target?.CourseId || ui.target?.id || ui.target?.Id));
            return (
                <StudentCoursePreview 
                    course={ui.target} 
                    media={courseMedia} 
                    isEnrolled={isSubscribed}
                    onSubscribe={(id) => toggleSubscription(id)}
                    onStart={(course) => { closeModal(); playCourse(course); }}
                    onClose={closeModal}
                />
            );
        }

        if (['mod', 'role', 'module', 'perm'].includes(type)) {
            if (['mod_perm_assign', 'role_mod_assign', 'role_mod_perm_assign', 'role_module_create', 'roleModPerm_view'].includes(ui.modal)) {
                return <PermissionMappingModal ui={ui} setUi={setUi} user={user} isSuperAdmin={isSuperAdmin} db={db} setDb={setDb} formTenantId={formTenantId} setFormTenantId={setFormTenantId} handleCrud={handleCrud} securityApi={securityApi} extractData={(res: any) => res.data?.data || res.data || res} setPm={setPm} openModPM={openModPM} toast={toast} permissions={permissions} />;
            }
            return <SecurityModal ui={ui} user={user} isSuperAdmin={isSuperAdmin} db={db} setUi={setUi} sync={fetchData} handleCrud={handleCrud} openModPM={openModPM} extractData={(res: any) => res.data?.data || res.data || res} />;
        }

        return null;
    };

    const getModalClass = () => {
        if (!ui.modal) return '';
        const type = ui.modal.toLowerCase();
        const isMedium = [
            'user_role_assign', 'role_mod_assign', 'role_mod_perm_assign',
            'course_create', 'course_edit', 
            'org_create', 'org_edit',
            'user_create', 'user_edit',
            'cat_create', 'cat_edit',
            'group_create', 'group_edit'
        ].includes(type);
        
        if (isMedium) return 'medium';
        if (['security_matrix', 'role_matrix', 'permission_matrix', 'group_courses', 'group_users', 'cm_studio', 'student_course_player'].includes(type)) return 'full';
        return '';
    };

    const modalContent = renderModalContent();
    const isShowingAnyModal = !!ui.modal || !!pm.open || !!gcModal || !!guModal || !!previewMedia;

    if (!isShowingAnyModal) return null;

    // Architectural Guard: Ensure gcModal and guModal (Group Assignment/Enrollment) 
    // take absolute precedence. We suppress the standard ui.modal stack if these are open 
    // to prevent dual-overlay rendering and UI "blinking".
    const showUiModal = ui.modal && modalContent && !gcModal && !guModal;

    return (
        <>
            {showUiModal && (
                <div 
                    className="lms-modal-overlay lms-modal-overlay-smooth" 
                    onClick={closeModal}
                    role="presentation"
                >
                    <div 
                        className={`lms-modal-form lms-slide-up ${getModalClass()}`} 
                        onClick={e => e.stopPropagation()}
                        role="dialog"
                        aria-modal="true"
                    >
                        <div className="lms-modal-header">
                            <div>
                                <div className="lms-modal-header-tag">WORKSPACE WIZARD</div>
                                <h2 className="lms-modal-title">
                                    {(() => {
                                        const titles: any = {
                                            'mod_perm_assign': 'Map Module Permissions',
                                            'role_mod_assign': 'Map Role Permissions',
                                            'role_mod_perm_assign': 'Role Security Wizard',
                                            'role_module_create': 'Add Module to Role',
                                            'student_course_player': 'Student Learning Studio'
                                        };
                                        return titles[ui.modal] || ui.modal.split('_').map((word: string) => word.charAt(0).toUpperCase() + word.slice(1)).join(' ');
                                    })()}
                                </h2>
                            </div>
                            <button className="lms-modal-close" onClick={closeModal} title="Close Modal">
                                <Icons.Close s={20} />
                            </button>
                        </div>
                        <div className="lms-modal-body lms-custom-scrollbar">
                            {modalContent}
                        </div>
                    </div>
                </div>
            )}

            {/* High Priority Studio Overlays (Outside UI Modal Stack) */}
            <PermissionMatrix
                pm={pm}
                setPm={setPm}
                pmSearch={pmSearch}
                setPmSearch={setPmSearch}
                isSuperAdmin={isSuperAdmin}
                togglePermission={togglePermission}
                savePermissions={savePermissions}
                permissions={permissions}
                db={db}
                onModuleChange={async (m: any) => {
                    setPm((prev: any) => ({ ...prev, module: m, loading: true }));
                    try {
                        const [mP, rM_P] = await Promise.all([
                            securityApi.getModulePermissions(m.id || m.Id),
                            securityApi.getRolePermissions(pm.role?.id || pm.role?.Id, m.id || m.Id, pm.tenantId)
                        ]);
                        const extract = (res: any) => res.data?.data || res.data || res;
                        const mPerms = extract(mP);
                        const rPerms = extract(rM_P).map((p: any) => Number(p.id || p.Id || p.permissionId || p.PermissionId)).filter((id: any) => !isNaN(id));
                        setPm((prev: any) => ({ ...prev, mPerms, rPerms, loading: false }));
                    } catch (err) {
                        toast.error("Module context switch failed");
                        setPm((prev: any) => ({ ...prev, loading: false }));
                    }
                }}
            />

            {(gcModal || guModal) && (
                <GroupAssignModal 
                    gcModal={gcModal} setGcModal={setGcModal} guModal={guModal} setGuModal={setGuModal} 
                    toggleCourse={toggleCourse} saveGroupCourses={saveGroupCourses} 
                    toggleUserInGroup={toggleUserInGroup} saveGroupUsers={saveGroupUsers} 
                    toggleAllVisibleCourses={toggleAllVisibleCourses} toggleAllVisibleUsers={toggleAllVisibleUsers} 
                />
            )}

            {previewMedia && (
                <SecureMediaViewer 
                    media={previewMedia} 
                    onClose={() => setPreviewMedia(null)} 
                    user={user}
                />
            )}
        </>
    );
};
