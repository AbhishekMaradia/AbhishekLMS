import React from 'react';
import { Icons } from '../../../shared/components/lms/Icons';
import './CourseMedia.css';

export const CourseMediaStudio = ({
    course,
    cmStudioTab,
    setCmStudioTab,
    cmMedia,
    setPreviewMedia,
    setCmTarget,
    setCmMedia,
    handleCmDelete,
    handleCmUpload,
    handleCmEdit,
    cmEditTarget,
    setCmEditTarget,
    mediaViewMode,
    setMediaViewMode,
    user,
    isSuperAdmin,
    orgs,
    hasPermission,
    isModalView = false
}: any) => {
    const [showUpload, setShowUpload] = React.useState(false);
    if (!course) return null;

    const Table = ({ headers, children }: any) => (
        <div className="lms-cms-table-wrap">
            <table className="lms-cms-table">
                <thead>
                    <tr className="lms-cms-thead-tr">
                        {headers.map((h: string) => (
                            <th key={h} className="lms-cms-th">{h}</th>
                        ))}
                    </tr>
                </thead>
                <tbody>{children}</tbody>
            </table>
        </div>
    );
    return (
        <div className="lms-fade-in lms-col-gap-md lms-cms-root" style={{ paddingTop: isModalView ? '0' : '0' }}>
            {/* Edit overlay */}
            {cmEditTarget && (
                <div className="lms-modal-overlay lms-cms-modal-overlay" onClick={() => setCmEditTarget(null)}>
                    <div className="lms-modal-form medium lms-cms-modal-form" onClick={e => e.stopPropagation()}>
                        <div className="lms-modal-header lms-cms-modal-header">
                            <div className="lms-flex-row lms-cms-modal-header-content">
                                <div className="lms-status-icon success lms-cms-modal-icon">
                                    {cmEditTarget.type === 'vid' ? <Icons.Video s={20} /> : <Icons.Doc s={20} />}
                                </div>
                                <h2 className="lms-modal-title lms-cms-modal-title">
                                    {cmEditTarget.type === 'vid' ? 'Video Metadata' : 'Document Metadata'}
                                </h2>
                            </div>
                            <button onClick={() => setCmEditTarget(null)} className="lms-icon-btn-sm lms-cms-modal-close">✕</button>
                        </div>
                        <form className="lms-modal-body lms-col-gap-md lms-cms-modal-body" onSubmit={handleCmEdit}>
                            <div className="lms-form-grid lms-cms-form-grid">
                                <div className="lms-col-gap-xs">
                                    <label className="lms-label-premium lms-cms-label">{cmEditTarget.type === 'vid' ? 'LESSON IDENTIFIER' : 'RESOURCE NAME'}</label>
                                    <div className="lms-modal-panel-premium">
                                        <input
                                            name={cmEditTarget.type === 'vid' ? 'title' : 'docName'}
                                            defaultValue={cmEditTarget.item.title || cmEditTarget.item.Title || cmEditTarget.item.docName || cmEditTarget.item.DocName || ''}
                                            className="lms-input-premium"
                                            placeholder="Enter asset name..."
                                            required
                                        />
                                    </div>
                                </div>
                                <div className="lms-col-gap-xs">
                                    <label className="lms-label-premium lms-cms-label">METADATA DESCRIPTION</label>
                                    <div className="lms-modal-panel-premium">
                                        <input
                                            name="description"
                                            defaultValue={cmEditTarget.item.description || cmEditTarget.item.Description || ''}
                                            className="lms-input-premium"
                                            placeholder="Brief description..."
                                            required
                                        />
                                    </div>
                                </div>
                            </div>

                            <div className="lms-flex-row lms-cms-btn-row">
                                <button type="submit" className="lms-btn-commit lms-flex-1 lms-cms-btn-commit">
                                    COMMIT UPDATES
                                </button>
                                <button type="button" onClick={() => setCmEditTarget(null)} className="lms-btn lms-flex-1 lms-cms-btn-discard">
                                    DISCARD
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            {!isModalView && (
                <div className="lms-flex-row lms-flex-row-mobile-stack lms-cms-header-row">
                    <div className="lms-flex-row lms-cms-header-left">
                        <div className="lms-cms-header-icon-wrap">
                            <Icons.Video s={24} />
                        </div>
                        <div>
                            <div className="lms-cms-header-breadcrumb">
                                WORKSPACE <span className="lms-cms-header-slash">/</span> MANAGEMENT
                            </div>
                            <div className="lms-flex-row lms-cms-header-title-row">
                                <h1 className="lms-modal-title lms-cms-page-title">Media Center</h1>
                                <span className="lms-cms-page-badge">
                                    Video and document operations
                                </span>
                            </div>
                        </div>
                    </div>
                    <div className="lms-flex-row lms-cms-header-right">
                        <Icons.Moon s={20} className="lms-cms-icon-dim" />
                        <Icons.Eye s={20} className="lms-cms-icon-dim" />
                        <div className="lms-cms-user-pill">
                            <span className="lms-cms-user-text">{(user?.firstName || user?.FirstName || 'Studio').slice(0, 8)} <br /><span className="lms-cms-user-sub">Online</span></span>
                            <div className="lms-cms-user-avatar">{(user?.firstName || user?.FirstName || 'S')[0]}</div>
                            <Icons.Prev s={10} className="lms-cms-user-arrow" />
                        </div>
                    </div>
                </div>
            )}

            {/* Asset Management Studio Card */}
            <div className="lms-premium-card lms-cms-studio-card">
                <div className="lms-flex-row lms-flex-row-mobile-stack lms-cms-studio-header">
                    <div className="lms-flex-row lms-flex-row-mobile-stack lms-cms-studio-left">
                        <div>
                            <div className="lms-cms-course-sub">COURSE STUDIO</div>
                            <h2 className="lms-cms-course-title">{course.title || course.Title || 'Resource Catalog'}</h2>
                            <div className="lms-flex-row lms-cms-course-meta">
                                <span className="lms-cms-course-meta-title">{orgs?.find((o: any) => Number(o.id || o.Id) === Number(course.tenantId || course.TenantId))?.orgName || 'Core Curriculum'}</span>
                            </div>
                        </div>
                    </div>

                    <div className="lms-view-toggle lms-cms-view-toggle">
                        <button onClick={() => setCmStudioTab('videos')} className={`lms-view-btn lms-cms-view-btn ${cmStudioTab === 'videos' ? 'active' : ''}`}>
                            <Icons.Video s={16} /> VIDEOS
                        </button>
                        <button onClick={() => setCmStudioTab('docs')} className={`lms-view-btn lms-cms-view-btn ${cmStudioTab === 'docs' ? 'active' : ''}`}>
                            <Icons.Doc s={16} /> DOCUMENTS
                        </button>
                    </div>
                </div>
            </div>

            {cmMedia.loading ? (
                <div className="lms-cms-loader-wrap">
                    <div className="lms-loader-spinner lms-cms-loader"></div>
                    <span className="lms-cms-loader-text">LOADING STUDIO...</span>
                </div>
            ) : (
                <div className="lms-fade-in">
                    {((cmStudioTab === 'videos' && (isSuperAdmin || hasPermission('VIDEO', 'VIDEO_ADD'))) ||
                        (cmStudioTab === 'docs' && (isSuperAdmin || hasPermission('DOCUMENT', 'DOCUMENT_ADD')))) && (
                            <div className="lms-cms-upload-wrap">
                                <div className="lms-cms-upload-header" onClick={() => setShowUpload(!showUpload)} style={{ cursor: 'pointer' }}>
                                    <div className="lms-status-icon accent lms-cms-upload-header-icon">
                                        {showUpload ? <Icons.Prev s={14} style={{ transform: 'rotate(-90deg)' }} /> : <Icons.Plus s={14} />}
                                    </div>
                                    <h4 className="lms-cms-upload-title">
                                        {cmStudioTab === 'videos' ? 'UPLOAD VIDEO' : 'UPLOAD DOCUMENT'}
                                    </h4>
                                </div>

                                {showUpload && (
                                    <form onSubmit={e => handleCmUpload(e, cmStudioTab === 'videos' ? 'vid' : 'doc')} className="lms-col-gap-md lms-fade-in">
                                        <div className="lms-form-grid lms-cms-upload-grid">
                                            <div>
                                                <label className="lms-label-premium required lms-cms-upload-label">{cmStudioTab === 'videos' ? 'VIDEO TITLE' : 'DOCUMENT NAME'} *</label>
                                                <div className="lms-modal-panel-premium lms-cms-upload-panel">
                                                    <div className="lms-cms-input-icon"><Icons.Video s={16} /></div>
                                                    <input name={cmStudioTab === 'videos' ? 'title' : 'docName'} placeholder={cmStudioTab === 'videos' ? 'Video Title' : 'Document Name'} className="lms-input-premium" required />
                                                </div>
                                            </div>
                                            <div>
                                                <label className="lms-label-premium required lms-cms-upload-label">DESCRIPTION *</label>
                                                <div className="lms-modal-panel-premium lms-cms-upload-panel">
                                                    <div className="lms-cms-input-icon"><Icons.Doc s={16} /></div>
                                                    <input name="description" placeholder="Description" className="lms-input-premium" required />
                                                </div>
                                            </div>
                                        </div>
                                        <div>
                                            <label className="lms-label-premium required lms-cms-upload-label">{cmStudioTab === 'videos' ? 'VIDEO FILE' : 'DOCUMENT FILE'} *</label>
                                            <div className="lms-modal-panel-premium lms-cms-upload-panel lms-cms-file-panel">
                                                <div className="lms-cms-upload-zone-icon"><Icons.Plus s={24} /></div>
                                                <input name="file" type="file" required className="lms-input-premium lms-cms-file-input" accept={cmStudioTab === 'videos' ? 'video/*' : undefined} />
                                                <div className="lms-cms-file-hint">Click to select or drag assets here</div>
                                            </div>
                                        </div>
                                        <button type="submit" className="lms-btn-commit lms-cms-upload-btn">
                                            {cmStudioTab === 'videos' ? 'UPLOAD VIDEO' : 'UPLOAD DOCUMENT'}
                                        </button>
                                    </form>
                                )}
                            </div>
                        )}


                    {/* Studio Inventory Grid */}
                    <div className="lms-cms-inventory-wrap">
                        <div className="lms-cms-inventory-header">
                            <div className="lms-flex-row lms-cms-inventory-left">
                                <h4 className="lms-cms-inventory-title">MEDIA FILES</h4>
                                <span className="lms-cms-inventory-badge">
                                    {cmStudioTab === 'videos' ? cmMedia.vids.length : cmMedia.docs.length} Found
                                </span>
                            </div>
                            <div className="lms-flex-row lms-cms-inventory-right">
                                <button onClick={() => setMediaViewMode('table')} className={`lms-icon-btn-sm lms-cms-view-icon-btn ${mediaViewMode === 'table' ? 'active' : ''}`}><Icons.Table s={16} /></button>
                                <button onClick={() => setMediaViewMode('grid')} className={`lms-icon-btn-sm lms-cms-view-icon-btn ${mediaViewMode === 'grid' ? 'active' : ''}`}><Icons.Grid s={16} /></button>
                            </div>
                        </div>

                        {mediaViewMode === 'grid' ? (
                            <div className="lms-fade-in">
                                {cmStudioTab === 'videos' ? (
                                    cmMedia.vids.length === 0 ? (
                                        <div className="lms-empty-state lms-cms-empty-state">
                                            No videos found for this course.
                                        </div>
                                    ) : (
                                        <div className="lms-media-grid lms-cms-grid-vid">
                                            {cmMedia.vids.map((v: any, i: number) => (
                                                <div key={v.id || v.Id || i} className="lms-vault-card lms-cms-card-vid">
                                                    <div className="lms-cms-card-vid-thumb-wrap">
                                                        {v.thumbnailUrl ? <img src={v.thumbnailUrl} className="lms-media-thumb" /> : <div className="lms-flex-row lms-cms-card-vid-placeholder"><Icons.Video s={32} opacity={0.3} /></div>}
                                                        <button onClick={() => {
                                                            const token = localStorage.getItem('token');
                                                            const o = `http://${window.location.hostname}:5209/api`;
                                                            const streamUrl = `${o}/CourseVideo/stream/${v.id || v.Id}?access_token=${token}`;
                                                            setPreviewMedia({ url: streamUrl, name: v.title, type: 'video' });
                                                        }} className="lms-cms-card-vid-play">
                                                            <Icons.Play s={40} />
                                                        </button>
                                                    </div>
                                                    <div className="lms-cms-card-vid-body">
                                                        <h4 className="lms-cms-card-vid-title">{v.title}</h4>
                                                        <p className="lms-cms-card-vid-desc">{v.description}</p>
                                                        <div className="lms-flex-row lms-cms-card-vid-actions">
                                                            {(isSuperAdmin || hasPermission('VIDEO', 'VIDEO_EDIT')) && (
                                                                <button onClick={() => setCmEditTarget({ type: 'vid', item: v })} className="lms-view-btn lms-flex-1 lms-cms-card-vid-edit">EDIT</button>
                                                            )}
                                                            {(isSuperAdmin || hasPermission('VIDEO', 'VIDEO_DELETE')) && (
                                                                <button onClick={() => handleCmDelete(v.id || v.Id, 'vid')} className="lms-cms-card-vid-del"><Icons.Trash s={16} /></button>
                                                            )}
                                                        </div>

                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    )
                                ) : (
                                    cmMedia.docs.length === 0 ? (
                                        <div className="lms-empty-state lms-cms-empty-state">
                                            No documents found for this course.
                                        </div>
                                    ) : (
                                        <div className="lms-media-grid lms-cms-grid-doc">
                                            {cmMedia.docs.map((d: any, i: number) => (
                                                <div key={d.id || d.Id || i} className="lms-vault-card lms-cms-card-doc">
                                                    <div className="lms-cms-card-doc-icon">
                                                        <Icons.Doc s={24} />
                                                    </div>
                                                    <h4 className="lms-cms-card-doc-title">{d.docName}</h4>
                                                    <div className="lms-flex-row lms-cms-card-doc-actions">
                                                        <button onClick={() => {
                                                            const t = localStorage.getItem('token');
                                                            const o = `http://${window.location.hostname}:5209/api`;
                                                            const downloadUrl = `${o}/CourseDocument/download/${d.id || d.Id}?access_token=${t}`;
                                                            setPreviewMedia({ url: `${downloadUrl}#toolbar=0`, name: d.docName, type: 'doc' });
                                                        }} className="lms-btn-commit lms-flex-2 lms-cms-card-doc-view">VIEW</button>
                                                        {(isSuperAdmin || hasPermission('DOCUMENT', 'DOCUMENT_EDIT')) && (
                                                            <button onClick={() => setCmEditTarget({ type: 'doc', item: d })} className="lms-icon-btn-sm lms-cms-card-doc-edit"><Icons.Edit s={14} /></button>
                                                        )}

                                                        {(isSuperAdmin || hasPermission('DOCUMENT', 'DOCUMENT_DELETE')) && (
                                                            <button onClick={() => handleCmDelete(d.id || d.Id, 'doc')} className="lms-cms-card-doc-del"><Icons.Trash s={14} /></button>
                                                        )}

                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    )
                                )}
                            </div>
                        ) : (
                            <div className="lms-fade-in">
                                <Table headers={['TITLE', 'DESCRIPTION', 'TYPE', 'ACTIONS']}>
                                    {(cmStudioTab === 'videos' ? cmMedia.vids : cmMedia.docs).map((m: any, i: number) => (
                                        <tr key={m.id || m.Id || i} className="lms-cms-tr-data">
                                            <td className="lms-cms-td-title">
                                                <div className="lms-cms-td-title-text">{m.title || m.docName || m.DisplayName || 'Untitled Resource'}</div>
                                            </td>
                                            <td className="lms-cms-td-desc">
                                                <div className="lms-cms-td-desc-text">
                                                    {m.description || 'No metadata description provided.'}
                                                </div>
                                            </td>
                                            <td className="lms-cms-td-type">
                                                <span className={`lms-tag ${cmStudioTab === 'videos' ? 'accent' : 'info'} lms-cms-td-type-badge`}>
                                                    {cmStudioTab === 'videos' ? 'VIDEO' : 'DOCUMENT'}
                                                </span>
                                            </td>
                                            <td className="lms-cms-td-actions">
                                                <div className="lms-flex-row lms-cms-td-actions-wrap">
                                                    <button onClick={() => {
                                                        const token = localStorage.getItem('token');
                                                        const o = `http://${window.location.hostname}:5209/api`;
                                                        if (cmStudioTab === 'videos') {
                                                            const streamUrl = `${o}/CourseVideo/stream/${m.id || m.Id}?access_token=${token}`;
                                                            setPreviewMedia({ url: streamUrl, name: m.title, type: 'video' });
                                                        } else {
                                                            const downloadUrl = `${o}/CourseDocument/download/${m.id || m.Id}?access_token=${token}`;
                                                            setPreviewMedia({ url: `${downloadUrl}#toolbar=0`, name: m.docName, type: 'doc' });
                                                        }
                                                    }} className="lms-icon-btn-sm lms-cms-td-btn-view" title="View Source"><Icons.Play s={14} /></button>
                                                    {((cmStudioTab === 'videos' && (isSuperAdmin || hasPermission('VIDEO', 'VIDEO_EDIT'))) ||
                                                        (cmStudioTab === 'docs' && (isSuperAdmin || hasPermission('DOCUMENT', 'DOCUMENT_EDIT')))) && (
                                                            <button onClick={() => setCmEditTarget({ type: cmStudioTab === 'videos' ? 'vid' : 'doc', item: m })} className="lms-icon-btn-sm info lms-cms-td-btn-edit" title="Edit Metadata"><Icons.Edit s={14} /></button>
                                                        )}
                                                    {((cmStudioTab === 'videos' && (isSuperAdmin || hasPermission('VIDEO', 'VIDEO_DELETE'))) ||
                                                        (cmStudioTab === 'docs' && (isSuperAdmin || hasPermission('DOCUMENT', 'DOCUMENT_DELETE')))) && (
                                                            <button onClick={() => handleCmDelete(m.id || m.Id, cmStudioTab === 'videos' ? 'vid' : 'doc')} className="lms-icon-btn-sm danger lms-cms-td-btn-del" title="Purge Asset"><Icons.Trash s={14} /></button>
                                                        )}
                                                </div>

                                            </td>
                                        </tr>
                                    ))}
                                </Table>
                            </div>
                        )}
                    </div>
                </div>
            )}
        </div>
    );
};
