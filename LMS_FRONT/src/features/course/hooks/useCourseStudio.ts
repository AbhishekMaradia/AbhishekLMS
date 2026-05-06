import { useState, useCallback } from 'react';
import { toast } from 'react-toastify';
import { courseApi } from '../api/courseApi';

export const useCourseStudio = (courseId: number | null) => {
    const [courseTab, setCourseTab] = useState<'details' | 'media'>('details');
    const [mediaStudioTab, setMediaStudioTab] = useState<'videos' | 'docs'>('videos');
    const [mediaViewMode, setMediaViewMode] = useState<'table' | 'grid'>('grid');
    const [courseMedia, setCourseMedia] = useState<{ loading: boolean, vids: any[], docs: any[] }>({
        loading: false, vids: [], docs: []
    });
    const [editTarget, setEditTarget] = useState<any>(null);

    const fetchCourseMedia = useCallback(async (id: number) => {
        setCourseMedia(prev => ({ ...prev, loading: true }));
        try {
            const [v, d] = await Promise.all([
                courseApi.getVideos(id),
                courseApi.getDocuments(id)
            ]);
            setCourseMedia({
                loading: false,
                vids: v.data?.data || v.data || [],
                docs: d.data?.data || d.data || []
            });
        } catch (err: any) {
            toast.error("Asset synchronization failure");
            setCourseMedia(prev => ({ ...prev, loading: false }));
        }
    }, []);

    const handleMediaUpload = async (e: React.FormEvent, type: 'vid' | 'doc') => {
        e.preventDefault();
        if (!courseId) return;
        const fd = new FormData(e.currentTarget as HTMLFormElement);
        
        try {
            setCourseMedia(prev => ({ ...prev, loading: true }));
            const res = type === 'vid' 
                ? await courseApi.uploadVideo(courseId, fd)
                : await courseApi.uploadDocument(courseId, fd);
            
            if (res.data.success) {
                toast.success("Asset deployed successfully");
                fetchCourseMedia(courseId);
                (e.target as HTMLFormElement).reset();
            } else toast.error(res.data.message);
        } catch (err: any) {
            toast.error(err.message || "Nexus transfer failure");
        } finally {
            setCourseMedia(prev => ({ ...prev, loading: false }));
        }
    };

    const handleMediaEdit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!editTarget) return;
        const fd = new FormData(e.currentTarget as HTMLFormElement);
        const data = Object.fromEntries(fd.entries());

        try {
            setCourseMedia(prev => ({ ...prev, loading: true }));
            const res = editTarget.type === 'vid'
                ? await courseApi.updateVideo(editTarget.item.id, String(data.title), String(data.description))
                : await courseApi.updateDocument(editTarget.item.id, String(data.docName), String(data.description));

            if (res.data.success) {
                toast.success("Registry updated");
                setEditTarget(null);
                if (courseId) fetchCourseMedia(courseId);
            }
        } catch (err: any) {
            toast.error("Synchronization failed");
        } finally {
            setCourseMedia(prev => ({ ...prev, loading: false }));
        }
    };

    const handleMediaDelete = async (id: number, type: 'vid' | 'doc') => {
        if (!window.confirm("Confirm asset decomposition?")) return;
        try {
            setCourseMedia(prev => ({ ...prev, loading: true }));
            const res = type === 'vid' 
                ? await courseApi.deleteVideo(id)
                : await courseApi.deleteDocument(id);

            if (res.data.success) {
                toast.success("Asset purged");
                if (courseId) fetchCourseMedia(courseId);
            }
        } catch (err: any) {
            toast.error("Operation failed");
        } finally {
            setCourseMedia(prev => ({ ...prev, loading: false }));
        }
    };

    return {
        courseTab, setCourseTab,
        mediaStudioTab, setMediaStudioTab,
        mediaViewMode, setMediaViewMode,
        courseMedia, setCourseMedia,
        editTarget, setEditTarget,
        fetchCourseMedia,
        handleMediaUpload,
        handleMediaEdit,
        handleMediaDelete
    };
};
