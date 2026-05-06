import React from 'react';
import { Icons } from '../../../shared/components/lms/Icons';
import { CommonTable } from '../../../shared/components/lms/LmsComponents';
import './Course.css';

interface CourseModalProps {
    ui: any;
    user: any;
    isSuperAdmin: boolean;
    db: any;
    formTenantId: any;
    setFormTenantId: any;
    courseTab: any;
    setCourseTab: any;
    courseMedia: any;
    mediaStudioTab: any;
    setMediaStudioTab: any;
    mediaViewMode: any;
    setMediaViewMode: any;
    editTarget: any;
    setEditTarget: any;
    handleMediaUpload: any;
    handleMediaEdit: any;
    handleMediaDelete: any;
    handleCrud: (action: string, entity: string, data: any) => Promise<void>;
    setPreviewImage: any;
    setPreviewMedia: any;
    hasPermission: any;
}

export const CourseModal = ({
    ui,
    user,
    isSuperAdmin,
    db,
    formTenantId,
    setFormTenantId,
    courseTab,
    setCourseTab,
    courseMedia,
    mediaStudioTab,
    setMediaStudioTab,
    mediaViewMode,
    setMediaViewMode,
    editTarget,
    setEditTarget,
    handleMediaUpload,
    handleMediaEdit,
    handleMediaDelete,
    handleCrud,
    setPreviewImage,
    setPreviewMedia,
    hasPermission
}: any) => {

    const renderMediaStudio = () => {
        if (courseMedia.loading) {
            return (
                <div className="lms-studio-loader lms-c-modal-studio-loader">
                    <div className="lms-loader-spin lms-c-modal-spinner" />
                    <div className="lms-status-sub lms-c-modal-spinner-text">Loading...</div>
                </div>
            );
        }

        return (
            <div className="lms-col-gap-md lms-c-modal-media-scroll">
                <div className="lms-view-toggle lms-c-modal-view-toggle-wrap">
                    {(['videos', 'docs'] as const).map(t => (
                        <button key={t} onClick={(e) => { e.preventDefault(); setMediaStudioTab(t); }} className={`lms-view-btn lms-c-modal-view-btn ${mediaStudioTab === t ? 'active' : ''}`}>
                            <div className="lms-flex-row lms-c-modal-btn-content">
                                {t === 'videos' ? <Icons.Video s={14} /> : <Icons.Doc s={14} />}
                                <span>{t === 'videos' ? 'Videos' : 'Documents'}</span>
                            </div>
                        </button>
                    ))}
                </div>

                {mediaStudioTab === 'videos' ? (
                    <div className="lms-col-gap-md lms-c-modal-root">
                        <div className="lms-flex-row lms-c-modal-header-row">
                            <div className="lms-status-title lms-c-modal-header-title">Videos</div>
                            <div className="lms-flex-row lms-c-modal-header-actions">
                                <button onClick={(e) => { e.preventDefault(); setMediaViewMode('table'); }} className={`lms-icon-btn-sm lms-c-modal-icon-btn ${mediaViewMode === 'table' ? 'active' : ''}`}><Icons.Table s={14} /></button>
                                <button onClick={(e) => { e.preventDefault(); setMediaViewMode('grid'); }} className={`lms-icon-btn-sm lms-c-modal-icon-btn ${mediaViewMode === 'grid' ? 'active' : ''}`}><Icons.Grid s={14} /></button>
                            </div>
                        </div>

                        <details className="lms-details-reset" open={editTarget?.type === 'vid'}>
                            <summary className="lms-summary-pill accent lms-c-modal-summary-pill">
                                <div className="lms-flex-row lms-c-modal-summary-content">
                                    {editTarget?.type === 'vid' ? <Icons.Edit s={14} /> : <Icons.Plus s={14} />}
                                    {editTarget?.type === 'vid' ? 'Edit Video' : 'Upload Video'}
                                </div>
                            </summary>
                            <div className="lms-modal-panel-dashed lms-c-modal-panel-padded">
                                {editTarget?.type === 'vid' ? (
                                    <form onSubmit={handleMediaEdit} className="lms-col-gap-md">
                                        <div className="lms-form-grid lms-c-modal-form-grid">
                                            <div>
                                                <label className="lms-label-premium lms-c-modal-form-label">TITLE</label>
                                                <div className="lms-modal-panel-premium">
                                                    <input name="title" defaultValue={editTarget.item.title || editTarget.item.Title || ''} placeholder="Title" className="lms-input-premium" required />
                                                </div>
                                            </div>
                                            <div>
                                                <label className="lms-label-premium lms-c-modal-form-label">DESCRIPTION</label>
                                                <div className="lms-modal-panel-premium">
                                                    <input name="description" defaultValue={editTarget.item.description || editTarget.item.Description || ''} placeholder="Description" className="lms-input-premium" required />
                                                </div>
                                            </div>
                                        </div>
                                        <div className="lms-flex-row lms-c-modal-form-actions">
                                            <button type="submit" className="lms-btn-commit lms-flex-1">Update</button>
                                            <button type="button" onClick={() => setEditTarget(null)} className="lms-btn lms-flex-1 lms-c-modal-cancel-btn">Cancel</button>
                                        </div>
                                    </form>
                                ) : (
                                    <form onSubmit={e => handleMediaUpload(e, 'vid')} className="lms-col-gap-md">
                                        <div className="lms-form-grid lms-c-modal-form-grid">
                                            <div>
                                                <label className="lms-label-premium lms-c-modal-form-label">TITLE</label>
                                                <div className="lms-modal-panel-premium">
                                                    <input name="title" placeholder="Title" className="lms-input-premium" required />
                                                </div>
                                            </div>
                                            <div>
                                                <label className="lms-label-premium lms-c-modal-form-label">DESCRIPTION</label>
                                                <div className="lms-modal-panel-premium">
                                                    <input name="description" placeholder="Description" className="lms-input-premium" required />
                                                </div>
                                            </div>
                                        </div>
                                        <div>
                                            <label className="lms-label-premium lms-c-modal-form-label">SOURCE FILE</label>
                                            <div className="lms-modal-panel-premium lms-c-modal-file-panel">
                                                <input name="file" type="file" accept="video/*" className="lms-input-premium lms-c-modal-file-input" required />
                                            </div>
                                        </div>
                                        <button type="submit" className="lms-btn-commit lms-c-modal-submit-btn">Upload</button>
                                    </form>
                                )}
                            </div>
                        </details>

                        {courseMedia.vids.length === 0 ? (
                            <div className="lms-empty-state lms-c-modal-empty-state">No videos found.</div>
                        ) : mediaViewMode === 'table' ? (
                            <CommonTable
                                headers={[{ header: 'Title' }, { header: 'Description' }, { header: 'Actions', className: 'lms-text-left' }, { header: '' }]}
                                empty={courseMedia.vids.length === 0}
                            >
                                {courseMedia.vids.map((v: any, i: number) => (
                                    <tr key={v.id || i}>
                                        <td className="lms-c-modal-td-num">
                                            <div className="lms-flex-row lms-c-modal-td-num-content">
                                                <span className="lms-status-sub lms-c-modal-num-text">{i + 1}</span>
                                                <div className="lms-c-modal-thumb-wrap">
                                                    {v.thumbnailUrl ? (
                                                        <img src={v.thumbnailUrl} alt="V" className="lms-media-thumb lms-c-modal-thumb-img" />
                                                    ) : (
                                                        <div className="lms-flex-row lms-c-modal-thumb-icon"><Icons.Video s={16} opacity={0.3} /></div>
                                                    )}
                                                </div>
                                            </div>
                                        </td>
                                        <td className="lms-c-modal-td-text">
                                            <div className="lms-status-title lms-c-modal-td-title">{v.title}</div>
                                            <div className="lms-status-sub lms-c-modal-td-sub">{v.description?.substring(0, 40)}...</div>
                                        </td>
                                        <td className="lms-c-modal-td-actions">
                                            <div className="lms-cell-actions">
                                                <button onClick={(e) => { e.preventDefault(); const token = localStorage.getItem('token'); const o = `http://${window.location.hostname}:5209/api`; const streamUrl = `${o}/CourseVideo/stream/${v.id || v.Id}?access_token=${token}`; setPreviewMedia({ url: streamUrl, name: v.title, type: 'video' }); }} className="lms-icon-btn-sm info"><Icons.Play s={14} /></button>
                                                <button onClick={(e) => { e.preventDefault(); setEditTarget({ type: 'vid', item: v }); }} className="lms-icon-btn-sm accent"><Icons.Edit s={14} /></button>
                                                <button onClick={(e) => { e.preventDefault(); handleMediaDelete(v.id || v.Id, 'vid'); }} className="lms-icon-btn-sm danger"><Icons.Trash s={14} /></button>
                                            </div>
                                        </td>
                                        <td></td>
                                    </tr>
                                ))}
                            </CommonTable>
                        ) : (

                            <div className="lms-media-grid lms-c-modal-grid-media">
                                {courseMedia.vids.map((v: any, i: number) => (
                                    <div key={v.id || i} className="lms-media-card lms-c-modal-card-vid">
                                        <div className="lms-c-modal-card-vid-thumb">
                                            {v.thumbnailUrl ? <img src={v.thumbnailUrl} className="lms-media-thumb" /> : <div className="lms-c-modal-card-vid-placeholder"><Icons.Video s={32} opacity={0.2} /></div>}
                                            <div className="lms-c-modal-card-vid-overlay" />
                                            <button onClick={() => { const token = localStorage.getItem('token'); const o = `http://${window.location.hostname}:5209/api`; const streamUrl = `${o}/CourseVideo/stream/${v.id || v.Id}?access_token=${token}`; setPreviewMedia({ url: streamUrl, name: v.title, type: 'video' }); }} className="lms-c-modal-card-vid-play">
                                                <Icons.Play s={16} />
                                            </button>
                                            <div className="lms-c-modal-card-vid-num">#{i + 1}</div>
                                        </div>
                                        <h4 className="lms-c-modal-card-title">{v.title}</h4>
                                        <div className="lms-flex-row lms-c-modal-card-actions">
                                            <button onClick={() => setEditTarget({ type: 'vid', item: v })} className="lms-icon-btn-sm lms-c-modal-card-icon-btn"><Icons.Edit s={14} /></button>
                                            <button onClick={() => handleMediaDelete(v.id, 'vid')} className="lms-icon-btn-sm danger"><Icons.Trash s={14} /></button>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                ) : (
                    <div className="lms-col-gap-md lms-c-modal-root">
                        <div className="lms-flex-row lms-c-modal-header-row">
                            <div className="lms-status-title lms-c-modal-header-title">DOCUMENTS</div>
                            <div className="lms-flex-row lms-c-modal-header-actions">
                                <button onClick={(e) => { e.preventDefault(); setMediaViewMode('table'); }} className={`lms-icon-btn-sm lms-c-modal-icon-btn ${mediaViewMode === 'table' ? 'active' : ''}`}><Icons.Table s={14} /></button>
                                <button onClick={(e) => { e.preventDefault(); setMediaViewMode('grid'); }} className={`lms-icon-btn-sm lms-c-modal-icon-btn ${mediaViewMode === 'grid' ? 'active' : ''}`}><Icons.Grid s={14} /></button>
                            </div>
                        </div>

                        <details className="lms-details-reset" open={editTarget?.type === 'doc'}>
                            <summary className="lms-summary-pill success lms-c-modal-summary-pill">
                                <div className="lms-flex-row lms-c-modal-summary-content">
                                    {editTarget?.type === 'doc' ? <Icons.Edit s={14} /> : <Icons.Plus s={14} />}
                                    {editTarget?.type === 'doc' ? 'Edit Document' : 'Upload Document'}
                                </div>
                            </summary>
                            <div className="lms-modal-panel-dashed lms-c-modal-panel-padded">
                                {editTarget?.type === 'doc' ? (
                                    <form onSubmit={handleMediaEdit} className="lms-col-gap-md">
                                        <div className="lms-form-grid lms-c-modal-form-grid">
                                            <div>
                                                <label className="lms-label-premium lms-c-modal-form-label">TITLE</label>
                                                <div className="lms-modal-panel-premium">
                                                    <input name="docName" defaultValue={editTarget.item.docName || editTarget.item.DocName || ''} placeholder="Title" className="lms-input-premium" required />
                                                </div>
                                            </div>
                                            <div>
                                                <label className="lms-label-premium lms-c-modal-form-label">DESCRIPTION</label>
                                                <div className="lms-modal-panel-premium">
                                                    <input name="description" defaultValue={editTarget.item.description || editTarget.item.Description || ''} placeholder="Description" className="lms-input-premium" required />
                                                </div>
                                            </div>
                                        </div>
                                        <div className="lms-flex-row lms-c-modal-form-actions">
                                            <button type="submit" className="lms-btn-commit lms-flex-1">Update</button>
                                            <button type="button" onClick={() => setEditTarget(null)} className="lms-btn lms-flex-1 lms-c-modal-cancel-btn">Cancel</button>
                                        </div>
                                    </form>
                                ) : (
                                    <form onSubmit={e => handleMediaUpload(e, 'doc')} className="lms-col-gap-md">
                                        <div className="lms-form-grid lms-c-modal-form-grid">
                                            <div>
                                                <label className="lms-label-premium lms-c-modal-form-label">TITLE</label>
                                                <div className="lms-modal-panel-premium">
                                                    <input name="docName" placeholder="Title" className="lms-input-premium" required />
                                                </div>
                                            </div>
                                            <div>
                                                <label className="lms-label-premium lms-c-modal-form-label">DESCRIPTION</label>
                                                <div className="lms-modal-panel-premium">
                                                    <input name="description" placeholder="Description" className="lms-input-premium" required />
                                                </div>
                                            </div>
                                        </div>
                                        <div>
                                            <label className="lms-label-premium lms-c-modal-form-label">SOURCE FILE</label>
                                            <div className="lms-modal-panel-premium lms-c-modal-file-panel">
                                                <input name="file" type="file" accept=".pdf,.docx,.xlsx,.ppt,.pptx,.txt" className="lms-input-premium lms-c-modal-file-input" required />
                                            </div>
                                        </div>
                                        <button type="submit" className="lms-btn-commit lms-c-modal-submit-btn">Upload</button>
                                    </form>
                                )}
                            </div>
                        </details>

                        {courseMedia.docs.length === 0 ? (
                            <div className="lms-empty-state lms-c-modal-empty-state">No documents found.</div>
                        ) : mediaViewMode === 'table' ? (
                            <CommonTable
                                headers={[{ header: 'Title' }, { header: 'Description' }, { header: 'Actions', className: 'lms-text-left' }, { header: '' }]}
                                empty={courseMedia.docs.length === 0}
                            >
                                {courseMedia.docs.map((d: any, i: number) => (
                                    <tr key={d.id || i}>
                                        <td className="lms-c-modal-td-doc-icon"><div className="lms-status-icon success lms-c-modal-doc-icon-wrap"><Icons.Doc s={14} /></div></td>
                                        <td className="lms-c-modal-td-text">
                                            <div className="lms-status-title lms-c-modal-td-title">{d.title || d.docName || d.DocName || d.DisplayName || 'Untitled Resource'}</div>
                                            <div className="lms-status-sub lms-c-modal-td-sub">{d.description?.substring(0, 40)}...</div>
                                        </td>
                                        <td className="lms-c-modal-td-actions">
                                            <div className="lms-cell-actions lms-c-modal-td-doc-actions">
                                                <button onClick={(e) => { e.preventDefault(); const t = localStorage.getItem('token'); const o = `http://${window.location.hostname}:5209/api`; const downloadUrl = `${o}/CourseDocument/download/${d.id || d.Id}?access_token=${t}`; setPreviewMedia({ url: `${downloadUrl}#toolbar=0&navpanes=0`, name: d.docName, type: 'doc' }); }} className="lms-icon-btn-sm info"><Icons.Eye s={14} /></button>
                                                <button onClick={(e) => { e.preventDefault(); setEditTarget({ type: 'doc', item: d }); }} className="lms-icon-btn-sm accent"><Icons.Edit s={14} /></button>
                                                <button onClick={(e) => { e.preventDefault(); handleMediaDelete(d.id || d.Id, 'doc'); }} className="lms-icon-btn-sm danger"><Icons.Trash s={14} /></button>
                                            </div>
                                        </td>
                                        <td></td>
                                    </tr>
                                ))}
                            </CommonTable>
                        ) : (
                            <div className="lms-media-grid lms-c-modal-grid-doc">
                                {courseMedia.docs.map((d: any, i: number) => (
                                    <div key={d.id || i} className="lms-media-card lms-c-modal-card-doc">
                                        <div className="lms-status-icon success lms-c-modal-card-doc-icon"><Icons.Doc s={20} /></div>
                                        <h4 className="lms-c-modal-card-doc-title">{d.title || d.docName || d.DocName || d.DisplayName || 'Untitled Resource'}</h4>
                                        <div className="lms-flex-row lms-c-modal-card-doc-actions">
                                            <button onClick={() => { const t = localStorage.getItem('token'); const o = `http://${window.location.hostname}:5209/api`; const downloadUrl = `${o}/CourseDocument/download/${d.id || d.Id}?access_token=${t}`; setPreviewMedia({ url: `${downloadUrl}#toolbar=0&navpanes=0`, name: d.docName, type: 'doc' }); }} className="lms-btn success solid lms-flex-1 lms-c-modal-btn-view">VIEW</button>
                                            <button onClick={() => setEditTarget({ type: 'doc', item: d })} className="lms-icon-btn-sm lms-c-modal-card-icon-btn"><Icons.Edit s={14} /></button>
                                            <button onClick={() => handleMediaDelete(d.id || d.Id, 'doc')} className="lms-icon-btn-sm danger"><Icons.Trash s={14} /></button>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                )}
            </div>
        );
    };

    return (
        <div className="lms-fade-in">
            {ui.modal === 'course_create' || ui.modal === 'course_edit' ? (
                <div className="lms-col-gap-md">
                    {ui.modal === 'course_edit' && (
                        <div className="lms-view-toggle lms-c-modal-view-tabs">
                            <button onClick={(e) => { e.preventDefault(); setCourseTab('details'); }} className={`lms-view-btn lms-c-modal-tab-btn ${courseTab === 'details' ? 'active' : ''}`}>
                                <Icons.Edit s={16} /> Course Details
                            </button>
                            <button onClick={(e) => { e.preventDefault(); setCourseTab('media'); }} className={`lms-view-btn lms-c-modal-tab-btn ${courseTab === 'media' ? 'active' : ''}`}>
                                <Icons.Video s={16} /> Studio
                            </button>
                        </div>
                    )}

                    {courseTab === 'details' || ui.modal === 'course_create' ? (
                        <form onSubmit={(e) => {
                            e.preventDefault();
                            const fd = new FormData(e.currentTarget);
                            const isActiveCheckbox = e.currentTarget.elements.namedItem('IsActive') as HTMLInputElement;
                            if (isActiveCheckbox) fd.set('IsActive', isActiveCheckbox.checked ? 'true' : 'false');
                            if (!isSuperAdmin) {
                                fd.set('TenantId', String((user as any).tenantId ?? (user as any).TenantId ?? 0));
                            } else if (fd.get('TenantId') === '0' || !fd.get('TenantId')) {
                                fd.set('TenantId', '0');
                            }
                            const op = ui.modal.split('_')[1];
                            const action = (op === 'edit' || op === 'update') ? 'update' : 'create';
                            handleCrud(action, 'course', fd);
                        }}>
                            <div className="lms-col-gap-md">
                                {ui.modal === 'course_edit' && <input type="hidden" name="Id" value={ui.target?.courseId || ui.target?.CourseId || ui.target?.id || ui.target?.Id || 0} />}

                                {isSuperAdmin && (
                                    <>
                                        <label className="lms-label-premium required">Organization</label>
                                        <div className="lms-modal-panel-premium">
                                            <select
                                                name="TenantId"
                                                defaultValue={ui.target?.tenantId ?? 0}
                                                onChange={(e) => {
                                                    setFormTenantId(Number(e.currentTarget.value));
                                                    const catSelect = e.currentTarget.form?.elements.namedItem('CategoryId') as HTMLSelectElement;
                                                    if (catSelect) catSelect.value = "";
                                                }}
                                                className="lms-input-premium lms-c-modal-select-org"
                                                required
                                            >
                                                <option value={0}>Super Admin (Global)</option>
                                                {db.orgs.filter((o: any) => (o.isActive ?? o.IsActive) !== false).map((o: any) => <option key={o.id || o.Id} value={o.id || o.Id}>{o.orgName || o.OrgName}</option>)}
                                            </select>
                                        </div>
                                    </>
                                )}

                                <label className="lms-label-premium required">Course Title</label>
                                <div className="lms-modal-panel-premium">
                                    <input name="Title" defaultValue={ui.target?.title} placeholder="e.g. Advanced UI/UX Design" className="lms-input-premium" required />
                                </div>

                                <div className="lms-form-grid lms-c-modal-form-grid">
                                    <div>
                                        <label className="lms-label-premium required">Instructor Name</label>
                                        <div className="lms-modal-panel-premium">
                                            <input name="Instructor" defaultValue={ui.target?.instructor} placeholder="Instructor" className="lms-input-premium" required />
                                        </div>
                                    </div>
                                    <div>
                                        <label className="lms-label-premium required">Lectures</label>
                                        <div className="lms-modal-panel-premium">
                                            <input name="Lectures" type="number" defaultValue={ui.target?.lectures || 0} className="lms-input-premium" required />
                                        </div>
                                    </div>
                                </div>

                                <label className="lms-label-premium required">Course Description</label>
                                <div className="lms-modal-panel-premium">
                                    <textarea name="Description" defaultValue={ui.target?.description} placeholder="Provide details about this course..." className="lms-input-premium lms-c-modal-desc-textarea" required />
                                </div>

                                <div className="lms-form-grid lms-c-modal-form-grid">
                                    <div>
                                        <label className="lms-label-premium required">Price ($)</label>
                                        <div className="lms-modal-panel-premium">
                                            <input name="Price" type="number" step="0.01" defaultValue={ui.target?.price || 0} placeholder="0.00" className="lms-input-premium" required />
                                        </div>
                                    </div>
                                    <div>
                                        <label className="lms-label-premium required">Duration (Hrs)</label>
                                        <div className="lms-modal-panel-premium">
                                            <input name="DurationHours" type="number" step="0.5" defaultValue={ui.target?.durationHours || 0} placeholder="0.0" className="lms-input-premium" required />
                                        </div>
                                    </div>
                                </div>

                                <div className="lms-form-grid lms-c-modal-form-grid">
                                    <div>
                                        <label className="lms-label-premium">Level</label>
                                        <div className="lms-modal-panel-premium">
                                            <select name="Difficulty" defaultValue={ui.target?.difficulty || 'Beginner'} className="lms-input-premium">
                                                <option value="Beginner">Beginner</option>
                                                <option value="Intermediate">Intermediate</option>
                                                <option value="Advanced">Advanced</option>
                                            </select>
                                        </div>
                                    </div>
                                    <div>
                                        <label className="lms-label-premium required">Category</label>
                                        <div className="lms-modal-panel-premium">
                                            <select name="CategoryId" defaultValue={ui.target?.categoryId || ''} className="lms-input-premium" required>
                                                <option value="">-- Choose Category --</option>
                                                {(cats || db?.cat || []).filter((c: any) => {
                                                    const isGlobal = formTenantId === 0 || formTenantId === null;
                                                    const cTid = c.tenantId ?? (c as any).TenantId;
                                                    return isGlobal || cTid === 0 || cTid === null || Number(cTid) === Number(formTenantId);
                                                }).map((c: any) => <option key={c.categoryId || c.Id} value={c.categoryId || c.Id}>{c.categoryName || c.Name}</option>)}
                                            </select>
                                        </div>
                                    </div>
                                </div>

                                <label className="lms-label-premium">Thumbnail Image</label>
                                <div className="lms-modal-panel-premium">
                                    <input type="file" name="ImageFile" accept="image/*" className="lms-input-premium lms-c-modal-thumb-input" />
                                </div>

                                <div className="lms-switch-premium lms-c-modal-status-wrap">
                                    <span className="lms-c-modal-status-label">Status</span>
                                    <input
                                        type="checkbox"
                                        name="IsActive"
                                        defaultChecked={ui.target ? (ui.target.isActive !== false) : true}
                                        className="lms-c-modal-status-checkbox"
                                    />
                                </div>

                                <button type="submit" disabled={ui.loading} className="lms-btn-commit">
                                    {ui.loading ? 'Saving...' : 'Save'}
                                </button>
                            </div>
                        </form>
                    ) : (
                        renderMediaStudio()
                    )}
                </div>
            ) : null}
        </div>
    );
};
