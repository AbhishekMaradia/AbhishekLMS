import React, { createContext, useContext, useState } from 'react';

interface ModalOptions {
    title?: string;
    size?: 'sm' | 'md' | 'lg' | 'xl';
}

interface ModalContextType {
    isOpen: boolean;
    openModal: (content: React.ReactNode, options?: ModalOptions) => void;
    closeModal: () => void;
}

const ModalContext = createContext<ModalContextType | undefined>(undefined);

export const ModalProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [isOpen, setIsOpen] = useState(false);
    const [content, setContent] = useState<React.ReactNode | null>(null);
    const [options, setOptions] = useState<ModalOptions>({});

    const openModal = (node: React.ReactNode, opts: ModalOptions = {}) => {
        setOptions(opts);
        setContent(node);
        setIsOpen(true);
    };

    const closeModal = () => {
        setIsOpen(false);
        setTimeout(() => {
            setContent(null);
            setOptions({});
        }, 300);
    };

    return (
        <ModalContext.Provider value={{ isOpen, openModal, closeModal }}>
            {children}
            {isOpen && (
                <div className="lms-modal-overlay" onClick={closeModal}>
                    <div 
                        className={`lms-modal-form ${options.size || 'md'}`} 
                        onClick={e => e.stopPropagation()}
                    >
                        <div className="lms-modal-header">
                            <h2 className="lms-modal-title">{options.title || 'Studio Dialog'}</h2>
                            <button className="lms-modal-close" onClick={closeModal}>✕</button>
                        </div>
                        <div className="lms-modal-body">
                            {content}
                        </div>
                    </div>
                </div>
            )}
        </ModalContext.Provider>
    );
};

export const useModal = () => {
    const context = useContext(ModalContext);
    if (!context) throw new Error('useModal must be used within ModalProvider');
    return context;
};
