import React from 'react';
import { Dashboard } from '../features/dashboard/Dashboard';
import '../features/dashboard/Dashboard.css';

interface DashboardPageProps {
    db: any;
    ui: any;
    counts: any;
    setTab: (t: string) => void;
    isSuperAdmin: boolean;
    hasPermission: (m: string, a: string) => boolean;
}

export const DashboardPage: React.FC<DashboardPageProps> = ({ 
    db, ui, counts, setTab, isSuperAdmin, hasPermission, ...rest 
}) => {
    return (
        <div className="lms-dashboard-page lms-fade-in">
            <Dashboard 
                db={db}
                ui={ui}
                counts={counts}
                setTab={setTab}
                isSuperAdmin={isSuperAdmin}
                hasPermission={hasPermission}
                tab={(rest as any).tab}
            />
        </div>
    );
};
