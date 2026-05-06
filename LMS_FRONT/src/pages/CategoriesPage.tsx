import React from 'react';
import { Icons } from '../shared/components/lms/Icons';
import { CategoryList } from '../features/course/components/CategoryList';
import { Pagination, Button, SearchInput, PerspectiveSwitcher } from '../shared/components/lms/LmsComponents';
import '../features/course/components/Course.css';

interface CategoriesPageProps {
    db: any;
    ui: any;
    setUi: (u: any) => void;
    searchTerm: string;
    setSearchTerm: (s: string) => void;
    viewMode: 'table' | 'grid';
    setViewMode: (v: 'table' | 'grid') => void;
    pagination: any;
    changePage: (e: string, p: number) => void;
    changePageSize: (e: string, s: number) => void;
    hasPermission: (m: string, a?: string) => boolean;
    handleCrud: (a: string, t: string, d: any) => void;
    user: any;
    isSuperAdmin: boolean;
}

export const CategoriesPage: React.FC<CategoriesPageProps> = ({
    db, ui, setUi, searchTerm, setSearchTerm, viewMode, setViewMode,
    pagination, changePage, changePageSize, hasPermission, handleCrud, user, isSuperAdmin
}) => {
    const p = pagination['cat'] || { page: 1, size: 50, total: 0 };
    const totalPages = Math.ceil((p.total || 0) / (p.size || 50)) || 1;

    const canCreate = isSuperAdmin || hasPermission('CATEGORY', 'CATEGORY_ADD');

    const dataList = db.cats || db.cat || [];
    
    return (
        <div className="lms-categories-page lms-fade-in">
            <div className="lms-premium-card">
                <div className="lms-entity-header">
                    <div className="lms-section-heading">
                        <h1 className="lms-card-title">Categories</h1>
                        <span className="lms-section-count">{p.total} categories</span>
                    </div>

                    <div className="lms-card-actions">
                        <PerspectiveSwitcher viewMode={viewMode} setViewMode={setViewMode} />
                        {canCreate && (
                            <Button
                                variant="primary"
                                onClick={() => setUi({ ...ui, modal: 'cat_create' })}
                                className="lms-btn-primary lms-categories-add-btn"
                            >
                                <Icons.Plus s={18} /> CREATE CATEGORY
                            </Button>
                        )}
                    </div>
                </div>

                <div className="lms-entity-filters">
                    <div className="lms-entity-search">
                        <SearchInput value={searchTerm} onChange={setSearchTerm} placeholder="Filter by category name..." />
                    </div>
                </div>
            </div>

            <div className="lms-container">
                <CategoryList
                    cats={db.cats || db.cat}
                    orgs={db.orgs}
                    viewMode={viewMode}
                    hasPermission={hasPermission}
                    setUi={setUi}
                    ui={ui}
                    handleCrud={handleCrud}
                    user={user}
                    isSuperAdmin={isSuperAdmin}
                />
                <Pagination
                    current={p.page}
                    total={totalPages}
                    totalItems={p.total}
                    itemsPerPage={p.size}
                    onPageChange={(page: number) => changePage('cat', page)}
                    onPageSizeChange={(size: number) => changePageSize('cat', size)}
                />
            </div>
        </div>
    );
};
