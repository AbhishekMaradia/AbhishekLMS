import React, { useState, useEffect } from 'react';
import { groupApi } from '../api/groupApi';
import { Icons } from '../../../shared/components/lms/Icons';
import { CommonTable } from '../../../shared/components/lms/LmsComponents';
import { toast } from 'react-toastify';

interface AttendanceLog {
    id: number;
    studentName: string;
    orgName: string;
    groupName: string;
    courseName: string;
    attendanceDate: string;
    startTime: string;
    endTime: string;
    status: string;
    remarks: string;
    documentUrl: string;
    thumbUrl: string;
}

export const AttendanceLogs: React.FC<{ 
    isSuperAdmin: boolean; 
    tenantId?: number; 
    groupId?: number; 
    courseId?: number; 
    search?: string; 
    setPreviewMedia?: (media: any) => void;
}> = ({ isSuperAdmin, tenantId, groupId, courseId, search = "", setPreviewMedia }) => {
    const [logs, setLogs] = useState<AttendanceLog[]>([]);
    const [loading, setLoading] = useState(false);
    // Local fallback preview state (used if global setPreviewMedia is not wired)
    const [localPreview, setLocalPreview] = useState<{ url: string; name: string; type: string } | null>(null);

    // Universal opener: use global modal if available, else local fallback
    const openPreview = (log: AttendanceLog | any) => {
        const rid = log.id || log.Id;
        const docUrl = log.documentUrl || log.DocumentUrl || '';
        const ext = docUrl.replace('.enc', '').split('.').pop()?.toLowerCase() || '';
        const isImg = ['jpg', 'jpeg', 'png', 'gif', 'webp', 'bmp'].includes(ext);
        const token = localStorage.getItem('token') || '';
        const media = {
            url: `/api/Attendance/view/${rid}${token ? `?access_token=${token}` : ''}`,
            name: `${log.studentName || log.StudentName || 'Student'} - Attendance Attachment`,
            type: isImg ? 'img' : 'doc'
        };
        console.log('[ATT] openPreview called, rid=', rid, 'setPreviewMedia=', typeof setPreviewMedia);
        if (setPreviewMedia) {
            setPreviewMedia(media);
        } else {
            setLocalPreview(media);
        }
    };

    const fetchLogs = async () => {
        setLoading(true);
        try {
            const res = await groupApi.getAttendanceList(tenantId, groupId, courseId);
            const list = res.data?.data || res.data || [];
            const finalData = Array.isArray(list) ? list : (list.items || list.Items || []);
            setLogs(finalData);
        } catch (err: any) {
            console.error("Failed to load attendance logs", err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchLogs();
    }, [tenantId, groupId, courseId]);

    const isImage = (url: string) => {
        if (!url) return false;
        const pathPart = url.split('?path=')[1] || url;
        const cleanUrl = pathPart.split('?')[0].replace('.enc', '');
        const ext = cleanUrl.split('.').pop()?.toLowerCase();
        return ['jpg', 'jpeg', 'png', 'gif', 'webp', 'bmp'].includes(ext || '');
    };

    const filteredLogs = logs.filter(log => {
        const s = search.toLowerCase();
        return (log.studentName || (log as any).StudentName || "").toLowerCase().includes(s) ||
            (log.groupName || (log as any).GroupName || "").toLowerCase().includes(s) ||
            (log.courseName || (log as any).CourseName || "").toLowerCase().includes(s) ||
            (log.status || (log as any).Status || "").toLowerCase().includes(s);
    });

    const getStatusColor = (status: string) => {
        switch ((status || '').toLowerCase()) {
            case 'present': return '#10b981';
            case 'absent': return '#ef4444';
            case 'late': return '#f59e0b';
            case 'excused': return '#6366f1';
            default: return '#6b7280';
        }
    };

    const handleDelete = async (id: number) => {
        if (!window.confirm("Are you sure you want to delete this attendance record? This cannot be undone.")) {
            return;
        }
        
        try {
            const res = await groupApi.deleteAttendance(id);
            if (res.data?.success) {
                // Remove the deleted log from state
                setLogs(prev => prev.filter(log => (log.id || (log as any).Id) !== id));
                toast.success(res.data?.message || "Attendance record deleted successfully.");
            } else {
                toast.error(res.data?.message || "Failed to delete attendance record.");
            }
        } catch (err: any) {
            console.error("Delete failed:", err);
            toast.error(err.message || "An error occurred while deleting the record.");
        }
    };

    const headers = [
        { header: 'Date', key: 'date' },
        ...(isSuperAdmin ? [{ header: 'Organization', key: 'org' }] : []),
        { header: 'Student Identity', key: 'student' },
        { header: 'Group / Course', key: 'context' },
        { header: 'Timing', key: 'timing' },
        { header: 'Status', key: 'status' },
        { header: 'Remarks', key: 'remarks' },
        { header: 'Actions', key: 'actions' }
    ];

    return (
        <div className="lms-attendance-logs lms-fade-in">
            <CommonTable headers={headers} loading={loading} empty={filteredLogs.length === 0 && !loading}>
                {filteredLogs.map((log) => (
                    <tr key={log.id}>
                        <td>
                            <div style={{ fontWeight: 600, fontSize: '13px' }}>
                                {new Date(log.attendanceDate).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })}
                            </div>
                        </td>
                        {isSuperAdmin && (
                            <td>
                                <span style={{ color: 'var(--color-primary)', fontWeight: 600, fontSize: '12px' }}>
                                    {log.orgName || (log as any).OrgName}
                                </span>
                            </td>
                        )}
                        <td style={{ minWidth: '150px' }}>
                            <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                                {(log.thumbUrl || (log as any).ThumbUrl) ? (
                                    <img
                                        src={log.thumbUrl || (log as any).ThumbUrl}
                                        alt="Doc"
                                        style={{ width: '32px', height: '32px', borderRadius: '4px', objectFit: 'cover', cursor: 'pointer', border: '1px solid rgba(255,255,255,0.1)', position: 'relative', zIndex: 10, pointerEvents: 'auto' }}
                                        onClick={(e) => {
                                            e.stopPropagation();
                                            openPreview(log);
                                        }}
                                    />
                                ) : (log.documentUrl || (log as any).DocumentUrl) ? (
                                    <div
                                        style={{ width: '32px', height: '32px', borderRadius: '4px', background: 'rgba(255,255,255,0.05)', display: 'flex', alignItems: 'center', justifyContent: 'center', cursor: 'pointer', position: 'relative', zIndex: 10, pointerEvents: 'auto' }}
                                        onClick={(e) => {
                                            e.stopPropagation();
                                            openPreview(log);
                                        }}
                                    >
                                        <Icons.Plus s={14} style={{ opacity: 0.5 }} />
                                    </div>
                                ) : null}
                                <div>
                                    <div style={{ fontWeight: 600, fontSize: '13px' }}>{log.studentName || (log as any).StudentName}</div>
                                    <div style={{ fontSize: '11px', opacity: 0.6 }}>{log.orgName || (log as any).OrgName}</div>
                                </div>
                            </div>
                        </td>
                        <td>
                            <div style={{ fontSize: '12px', fontWeight: 600 }}>{log.groupName || (log as any).GroupName}</div>
                            <div style={{ fontSize: '11px', opacity: 0.6 }}>{log.courseName || (log as any).CourseName}</div>
                        </td>
                        <td>
                            <div style={{ fontSize: '11px', fontWeight: 500 }}>
                                {(log.startTime || (log as any).StartTime) && (log.endTime || (log as any).EndTime) ? (
                                    <span style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                                        <Icons.Clock s={12} /> {log.startTime || (log as any).StartTime} - {log.endTime || (log as any).EndTime}
                                    </span>
                                ) : '-'}
                            </div>
                        </td>
                        <td>
                            <span className="lms-status-badge" style={{
                                backgroundColor: `${getStatusColor(log.status || (log as any).Status)}15`,
                                color: getStatusColor(log.status || (log as any).Status),
                                border: `1px solid ${getStatusColor(log.status || (log as any).Status)}30`,
                                fontSize: '10px',
                                padding: '3px 10px'
                            }}>
                                {(log.status || (log as any).Status)?.toUpperCase()}
                            </span>
                        </td>
                        <td style={{ fontSize: '11px', maxWidth: '180px' }}>
                            <div style={{ opacity: 0.8, marginBottom: '4px' }}>{log.remarks || (log as any).Remarks || '-'}</div>
                            {(log.documentUrl || (log as any).DocumentUrl) && (
                                <button
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        openPreview(log);
                                    }}
                                    style={{
                                        background: 'rgba(var(--color-primary-rgb), 0.1)',
                                        border: '1px solid rgba(var(--color-primary-rgb), 0.2)',
                                        cursor: 'pointer',
                                        color: 'var(--color-primary)',
                                        fontWeight: 700,
                                        fontSize: '9px',
                                        padding: '4px 8px',
                                        borderRadius: '4px',
                                        display: 'inline-flex',
                                        alignItems: 'center',
                                        gap: '4px',
                                        transition: 'all 0.2s ease',
                                        position: 'relative',
                                        zIndex: 10,
                                        pointerEvents: 'auto'
                                    }}
                                    onMouseOver={(e) => e.currentTarget.style.background = 'rgba(var(--color-primary-rgb), 0.2)'}
                                    onMouseOut={(e) => e.currentTarget.style.background = 'rgba(var(--color-primary-rgb), 0.1)'}
                                >
                                    <Icons.Plus s={10} /> VIEW ATTACHMENT
                                </button>
                            )}
                        </td>
                        <td>
                            <button
                                onClick={(e) => {
                                    e.stopPropagation();
                                    handleDelete(log.id || (log as any).Id);
                                }}
                                className="lms-icon-btn-sm danger"
                                title="Delete Record"
                                style={{ position: 'relative', zIndex: 10, pointerEvents: 'auto' }}
                            >
                                <Icons.Trash s={14} />
                            </button>
                        </td>
                    </tr>
                ))}
            </CommonTable>

            {/* LOCAL FALLBACK MODAL — works even if global setPreviewMedia is not wired */}
            {localPreview && (
                <div
                    style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.88)', backdropFilter: 'blur(12px)', zIndex: 9999, display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '32px' }}
                    onClick={() => setLocalPreview(null)}
                >
                    <div
                        style={{ position: 'relative', width: '100%', maxWidth: '1000px', height: '85vh', background: '#111', borderRadius: '14px', overflow: 'hidden', border: '1px solid rgba(255,255,255,0.1)', display: 'flex', flexDirection: 'column', boxShadow: '0 32px 80px rgba(0,0,0,0.6)' }}
                        onClick={e => e.stopPropagation()}
                    >
                        {/* Header */}
                        <div style={{ padding: '14px 20px', borderBottom: '1px solid rgba(255,255,255,0.07)', display: 'flex', justifyContent: 'space-between', alignItems: 'center', background: 'rgba(255,255,255,0.03)' }}>
                            <div style={{ fontWeight: 700, fontSize: '14px', color: 'rgba(255,255,255,0.9)' }}>📎 {localPreview.name}</div>
                            <button onClick={() => setLocalPreview(null)} style={{ background: 'rgba(255,255,255,0.08)', border: 'none', color: '#fff', borderRadius: '50%', width: '30px', height: '30px', cursor: 'pointer', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '18px' }}>×</button>
                        </div>
                        {/* Content */}
                        <div style={{ flex: 1, overflow: 'hidden', display: 'flex', alignItems: 'center', justifyContent: 'center', background: '#000' }}>
                            {localPreview.type === 'img' ? (
                                <img src={localPreview.url} alt={localPreview.name} style={{ maxWidth: '100%', maxHeight: '100%', objectFit: 'contain' }} onContextMenu={e => e.preventDefault()} />
                            ) : (
                                <iframe src={`${localPreview.url}#toolbar=0&navpanes=0`} title="Document Preview" style={{ width: '100%', height: '100%', border: 'none' }} />
                            )}
                        </div>
                        {/* Footer */}
                        <div style={{ padding: '10px 20px', borderTop: '1px solid rgba(255,255,255,0.07)', display: 'flex', justifyContent: 'flex-end', gap: '10px', background: 'rgba(255,255,255,0.02)' }}>
                            <a href={localPreview.url} target="_blank" rel="noreferrer" style={{ fontSize: '12px', color: 'rgba(255,255,255,0.6)', textDecoration: 'none', padding: '6px 12px', borderRadius: '4px', background: 'rgba(255,255,255,0.05)' }}>Open in New Tab</a>
                            <button onClick={() => setLocalPreview(null)} style={{ fontSize: '12px', background: 'var(--color-primary)', color: 'var(--color-btn-text)', border: 'none', padding: '6px 16px', borderRadius: '4px', fontWeight: 700, cursor: 'pointer' }}>Close</button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};
