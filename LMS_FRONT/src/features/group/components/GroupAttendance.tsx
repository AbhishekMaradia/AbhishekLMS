import React, { useState, useEffect, useMemo } from 'react';
import { Icons } from '../../../shared/components/lms/Icons';
import { CommonTable, Button, Pagination, SearchInput } from '../../../shared/components/lms/LmsComponents';
import { groupApi } from '../api/groupApi';
import './GroupAttendance.css';
import { toast } from 'react-toastify';

interface UserAttendance {
    userId: number;
    name: string;
    email: string;
    status: 'Present' | 'Late' | 'Excused' | 'Absent';
    description: string;
    file: File | null;
    showDetails: boolean;
    startTime?: string;
    endTime?: string;
}

interface GroupAttendanceProps {
    groups: any[];
    tenantId?: number;
    filters: {
        group: number | '';
        course: number | '';
        startDate: string;
        endDate: string;
        startTime?: string;
        endTime?: string;
        search: string;
    };
    onSearchChange: (val: string) => void;
    onClose?: () => void;
}

export const GroupAttendance: React.FC<GroupAttendanceProps> = ({ groups, tenantId, filters, onSearchChange, onClose }) => {
    // --- STATE MANAGEMENT ---
    const [users, setUsers] = useState<UserAttendance[]>([]);
    const [loading, setLoading] = useState(false);
    const [saving, setSaving] = useState(false);

    // --- ANALYTICS ---
    const stats = useMemo(() => {
        const total = users.length;
        const present = users.filter(u => u.status === 'Present').length;
        const late = users.filter(u => u.status === 'Late').length;
        const excused = users.filter(u => u.status === 'Excused').length;
        const absent = users.filter(u => u.status === 'Absent').length;
        return { total, present, late, excused, absent };
    }, [users]);

    // --- DATA FETCHING ---
    useEffect(() => {
        const loadUsersAndAttendance = async () => {
            if (!filters.group) {
                setUsers([]);
                return;
            }

            setLoading(true);
            try {
                // 1. Load Student List for the Group
                const userRes = await groupApi.getGroupUsers(Number(filters.group));
                const rawUsers = userRes.data?.data || userRes.data || [];
                const usersList = Array.isArray(rawUsers) ? rawUsers : (rawUsers.items || []);

                // 2. Load Existing Attendance for Group+Course+Date
                let existingAttendance: any[] = [];
                if (filters.course && filters.startDate) {
                    try {
                        const attRes = await groupApi.getAttendanceByFilters(Number(filters.group), Number(filters.course), filters.startDate);
                        existingAttendance = attRes.data?.data || [];
                    } catch (attErr) {
                        console.error("Failed to load existing attendance, starting fresh", attErr);
                    }
                }

                const mappedUsers = usersList
                    .filter((u: any) => u.isAssigned === true || u.IsAssigned === true)
                    .map((u: any) => {
                        const userId = u.userId || u.id || u.Id;
                        const existing = existingAttendance.find(a => a.id === userId);

                        return {
                            userId: userId,
                            name: u.firstName ? `${u.firstName} ${u.lastName || ''}` : (u.name || 'User'),
                            email: u.email || 'N/A',
                            status: (existing?.status as any) || 'Present',
                            description: existing?.remarks || '',
                            file: null,
                            showDetails: !!(existing?.remarks || existing?.startTime),
                            startTime: existing?.startTime || filters.startTime,
                            endTime: existing?.endTime || filters.endTime
                        };
                    });
                setUsers(mappedUsers);
            } catch (err: any) {
                console.error("Attendance Load Error:", err);
                toast.error("Failed to load students.");
            } finally {
                setLoading(false);
            }
        };

        loadUsersAndAttendance();
    }, [filters.group, filters.course, filters.startDate]);

    // --- HANDLERS ---
    const handleStatusChange = (userId: number, status: UserAttendance['status']) => {
        setUsers(prev => prev.map(u => u.userId === userId ? { ...u, status } : u));
    };

    const toggleDetails = (userId: number) => {
        setUsers(prev => prev.map(u => u.userId === userId ? { ...u, showDetails: !u.showDetails } : u));
    };

    const markBulk = (status: UserAttendance['status']) => {
        setUsers(prev => prev.map(u => ({ ...u, status })));
        toast.info(`Marked all as ${status}`);
    };

    const resetForm = () => {
        setUsers(prev => prev.map(u => ({ ...u, status: 'Present', description: '', file: null })));
        toast.info("Selections reset");
    };

    const handleSubmit = async () => {
        if (!filters.group || !filters.course) return toast.warning("Select Group and Course first.");
        
        const formData = new FormData();
        formData.append("tenantId", String(tenantId || 0));
        formData.append("groupId", String(filters.group));
        formData.append("courseId", String(filters.course));
        formData.append("startDate", filters.startDate);
        formData.append("endDate", filters.endDate);
        formData.append("sessionStartTime", filters.startTime || "");
        formData.append("sessionEndTime", filters.endTime || "");

        // Append records as individual fields (for binding to List<Records>)
        users.forEach((u, index) => {
            formData.append(`Records[${index}].Id`, String(u.userId));
            formData.append(`Records[${index}].S`, u.status);
            formData.append(`Records[${index}].D`, u.description || "");
            formData.append(`Records[${index}].StartTime`, u.startTime || "");
            formData.append(`Records[${index}].EndTime`, u.endTime || "");
            
            if (u.file) {
                // Name it with original extension so backend can parse it: file_{userId}.{ext}
                const ext = u.file.name.split('.').pop() || '';
                const filenameWithExt = ext ? `file_${u.userId}.${ext}` : `file_${u.userId}`;
                formData.append("files", u.file, filenameWithExt);
            }
        });
        
        setSaving(true);
        try {
            // Using direct axios or fetch if groupApi doesn't support FormData easily
            // But we can just pass formData to it
            const res = await groupApi.submitAttendance(formData);
            if (res.data.success) {
                toast.success("Attendance submitted successfully!");
            } else {
                toast.error("Failed to submit attendance.");
            }
        } catch (err: any) {
            toast.error(err.message || "Attendance submission error.");
        } finally {
            setSaving(false);
        }
    };

    // --- FILTERED DATA (LOCAL SEARCH) ---
    const filteredUsers = useMemo(() => {
        return users.filter(u => 
            u.name.toLowerCase().includes(filters.search.toLowerCase()) || 
            u.email.toLowerCase().includes(filters.search.toLowerCase())
        );
    }, [users, filters.search]);

    const headers = [
        { header: 'Student Identity', key: 'user' },
        { header: 'Status Selection', key: 'status' },
        { header: 'Actions', key: 'actions', className: 'lms-text-right' }
    ];

    const statuses: UserAttendance['status'][] = ['Present', 'Late', 'Excused', 'Absent'];

    return (
        <div className="lms-attendance-listing-card lms-fade-in" style={{ marginTop: '0px' }}>

            {/* 2. Bulk Actions & Search */}
            <div className="lms-att-actions-row" style={{ marginBottom: '20px', display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: '16px', flexWrap: 'wrap' }}>
                <div style={{ flex: '1 1 300px', maxWidth: '400px' }}>
                    <SearchInput 
                        value={filters.search} 
                        onChange={onSearchChange} 
                        placeholder="Search students..." 
                    />
                </div>
                <div style={{ display: 'flex', gap: '12px' }}>
                    <Button variant="secondary" outline size="sm" onClick={() => markBulk('Present')} disabled={!filters.group}>
                        <Icons.Check s={14} /> MARK ALL PRESENT
                    </Button>
                    <Button variant="secondary" outline size="sm" onClick={() => markBulk('Absent')} disabled={!filters.group}>
                        <Icons.X s={14} /> MARK ALL ABSENT
                    </Button>
                </div>
            </div>

            <CommonTable headers={headers} loading={loading} empty={filteredUsers.length === 0 && !loading}>
                {filteredUsers.map(u => (
                    <React.Fragment key={u.userId}>
                        <tr>
                            <td>
                                <div className="lms-att-user-info">
                                    <div>
                                        <div style={{ fontWeight: 700, color: '#1e293b' }}>{u.name}</div>
                                        <div style={{ fontSize: '11px', color: '#64748b' }}>{u.email}</div>
                                    </div>
                                </div>
                            </td>
                            <td>
                                <div className="lms-att-checkbox-group">
                                    {statuses.map(s => (
                                        <label key={s} className="lms-att-checkbox-item">
                                            <input 
                                                type="checkbox" 
                                                checked={u.status === s}
                                                onChange={() => handleStatusChange(u.userId, s)}
                                            />
                                            <span className={`lms-label-${s.charAt(0).toLowerCase()}`}>{s}</span>
                                        </label>
                                    ))}
                                </div>
                            </td>
                            <td className="lms-text-right">
                                <Button variant="ghost" size="sm" onClick={() => toggleDetails(u.userId)}>
                                    {u.description ? <Icons.Message s={18} className="p-color" /> : <Icons.Plus s={18} />}
                                </Button>
                            </td>
                        </tr>
                        {u.showDetails && (
                            <tr className="lms-att-details-row">
                                <td colSpan={3}>
                                    <div className="lms-att-details-container" style={{ display: 'flex', gap: '20px', padding: '15px', background: '#f8fafc', borderRadius: '10px', marginTop: '10px', flexWrap: 'wrap' }}>
                                        <div style={{ flex: '1 1 200px' }}>
                                            <label className="lms-att-label">Individual Timing (In/Out)</label>
                                            <div style={{ display: 'flex', gap: '8px' }}>
                                                <input 
                                                    type="time" 
                                                    className="lms-att-input" 
                                                    style={{ height: '36px' }}
                                                    value={u.startTime || ''} 
                                                    onChange={(e) => setUsers(prev => prev.map(usr => usr.userId === u.userId ? { ...usr, startTime: e.target.value } : usr))}
                                                />
                                                <input 
                                                    type="time" 
                                                    className="lms-att-input" 
                                                    style={{ height: '36px' }}
                                                    value={u.endTime || ''} 
                                                    onChange={(e) => setUsers(prev => prev.map(usr => usr.userId === u.userId ? { ...usr, endTime: e.target.value } : usr))}
                                                />
                                            </div>
                                        </div>
                                        <div style={{ flex: '2 1 300px' }}>
                                            <label className="lms-att-label">Remarks & Observations</label>
                                            <input 
                                                className="lms-att-input" 
                                                style={{ background: 'white', height: '36px', border: '1px solid #e2e8f0', borderRadius: '8px', width: '100%', padding: '0 12px' }}
                                                placeholder="Add academic observations..." 
                                                value={u.description}
                                                onChange={(e) => {
                                                    const val = e.target.value;
                                                    setUsers(prev => prev.map(usr => usr.userId === u.userId ? { ...usr, description: val } : usr));
                                                }}
                                            />
                                        </div>
                                        <div style={{ flex: '1 1 200px' }}>
                                            <label className="lms-att-label">Supporting Doc</label>
                                            <div style={{ display: 'flex', gap: '10px', alignItems: 'center' }}>
                                                <input type="file" id={`f-${u.userId}`} hidden onChange={(e) => {
                                                    const f = e.target.files?.[0] || null;
                                                    setUsers(prev => prev.map(usr => usr.userId === u.userId ? { ...usr, file: f } : usr));
                                                }} />
                                                <Button size="sm" variant="secondary" outline onClick={() => document.getElementById(`f-${u.userId}`)?.click()}>CHOOSE</Button>
                                                {u.file && <span className="p-color" style={{ fontSize: '11px', fontWeight: 600 }}>✓ {u.file.name.substring(0, 10)}...</span>}
                                            </div>
                                        </div>
                                    </div>
                                </td>
                            </tr>
                        )}
                    </React.Fragment>
                ))}
            </CommonTable>

            <div style={{ marginTop: '20px', display: 'flex', justifyContent: 'center' }}>
                <Pagination 
                    current={1} 
                    total={Math.ceil(filteredUsers.length / 50) || 1} 
                    totalItems={filteredUsers.length} 
                    itemsPerPage={50} 
                    onPageChange={() => {}} 
                    onPageSizeChange={() => {}} 
                />
            </div>

            <div className="lms-att-footer">
                <div style={{ fontSize: '13px', color: '#64748b' }}>
                    Tracking <strong>{stats.total}</strong> students in this session.
                </div>
                <div style={{ display: 'flex', gap: '12px' }}>
                    <Button variant="ghost" onClick={resetForm}>Discard</Button>
                    <Button variant="primary" solid onClick={handleSubmit} disabled={users.length === 0 || loading || saving}>
                        {saving ? 'SUBMITTING...' : 'SUBMIT ATTENDANCE'}
                    </Button>
                </div>
            </div>
        </div>
    );
};
