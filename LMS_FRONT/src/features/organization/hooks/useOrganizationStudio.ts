import { useState, useCallback } from 'react';
import { toast } from 'react-toastify';
import { userApi } from '../../auth/api/userApi';

export const useOrganizationStudio = (orgId: number | null) => {
    const [orgEditTab, setOrgEditTab] = useState<'org' | 'admin'>('org');
    const [orgAdminData, setOrgAdminData] = useState<any>(null);

    const fetchOrgAdmin = useCallback(async (org: any) => {
        if (!org) return;
        setOrgAdminData(null);
        
        // Comprehensive ID resolution to handle DTO casing/naming variations
        const id = org.tid || org.tenantId || org.TenantId || org.id || org.Id;
        console.log('[ORG ADMIN SYNC] Initiating fetch for Node ID:', id, 'Source:', org);
        
        if (!id || id === 0) {
            console.warn('[ORG ADMIN SYNC] Invalid ID detected. Potential SuperAdmin or uninitialized node.');
            return;
        }
        
        try {
            const res = await userApi.getAdminByTenant(Number(id));
            // res.data is the normalized object from apiClient
            if (res.data.success && res.data.data) {
                setOrgAdminData(res.data.data);
                console.log('[ORG ADMIN SYNC] Profile synchronized successfully.');
            } else {
                console.warn('[ORG ADMIN SYNC] Authority reported no active admin for this node.');
            }
        } catch (err: any) {
             console.error('[ORG ADMIN SYNC] Critical protocol failure:', err);
             // Silent fail to avoid interrupting UI flow, but logged for debugging
        }
    }, []);


    return {
        orgEditTab, setOrgEditTab,
        orgAdminData, setOrgAdminData,
        fetchOrgAdmin
    };
};
