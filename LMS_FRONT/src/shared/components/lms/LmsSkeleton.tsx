import React from 'react';

/**
 * Premium Loading Skeletons for LMS Studio
 * Designed for High-Fidelity UI parity across modules.
 */

export const SkeletonBox: React.FC<{ width?: string, height?: string, radius?: string, style?: any }> = ({ width = '100%', height = '20px', radius = '8px', style }) => (
    <div 
        className="lms-skeleton-pulse" 
        style={{ 
            width, 
            height, 
            borderRadius: radius, 
            background: 'var(--color-glass-bright)',
            ...style 
        }} 
    />
);

export const DashboardStatsSkeleton = () => (
    <div className="lms-stats-grid">
        {[1, 2, 3].map(i => (
            <div key={i} className="lms-stat-card lms-premium-card" style={{ marginBottom: 0 }}>
                <div className="lms-skeleton-pulse" style={{ width: '48px', height: '48px', borderRadius: '12px' }} />
                <div style={{ flex: 1 }}>
                    <SkeletonBox width="40%" height="10px" style={{ marginBottom: '8px' }} />
                    <SkeletonBox width="70%" height="24px" />
                </div>
            </div>
        ))}
    </div>
);

export const CourseGridSkeleton = () => (
    <div className="lms-grid-container">
        {[1, 2, 3, 4].map(i => (
            <div key={i} className="lms-grid-card">
                <div className="lms-grid-banner" style={{ background: 'var(--color-glass-bright)' }}>
                    <div className="lms-skeleton-pulse" style={{ width: '100%', height: '100%' }} />
                </div>
                <div className="lms-grid-body">
                    <SkeletonBox width="80%" height="20px" style={{ marginBottom: '16px' }} />
                    <SkeletonBox width="40%" height="12px" style={{ marginBottom: '8px' }} />
                    <SkeletonBox width="100%" height="40px" radius="12px" style={{ marginTop: '20px' }} />
                </div>
            </div>
        ))}
    </div>
);

export const TableSkeleton = ({ rows = 5 }: { rows?: number }) => (
    <div className="lms-table-wrapper">
        <div style={{ padding: '20px' }}>
            {[...Array(rows)].map((_, i) => (
                <div key={i} style={{ display: 'flex', gap: '20px', marginBottom: '20px', borderBottom: '1px solid var(--color-border)', paddingBottom: '20px' }}>
                    <SkeletonBox width="50px" height="50px" radius="12px" />
                    <div style={{ flex: 1 }}>
                        <SkeletonBox width="30%" height="18px" style={{ marginBottom: '10px' }} />
                        <SkeletonBox width="15%" height="12px" />
                    </div>
                    <SkeletonBox width="150px" height="40px" radius="100px" />
                </div>
            ))}
        </div>
    </div>
);

export const PeerListSkeleton = () => (
    <div className="lms-grid-container grid-3">
        {[1, 2, 3, 4, 5, 6].map(i => (
            <div key={i} className="lms-premium-card" style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
                <SkeletonBox width="50px" height="50px" radius="50%" />
                <div style={{ flex: 1 }}>
                    <SkeletonBox width="60%" height="16px" style={{ marginBottom: '8px' }} />
                    <SkeletonBox width="80%" height="12px" />
                </div>
            </div>
        ))}
    </div>
);
