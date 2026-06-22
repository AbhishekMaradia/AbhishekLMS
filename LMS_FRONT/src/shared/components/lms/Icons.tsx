import React from 'react';

const BaseSvg = ({ s = 17, children, fill = 'none', strokeWidth = 2.1, viewBox = '0 0 24 24', style = {}, className }: any) => (
    <svg
        width={s} height={s} viewBox={viewBox} fill={fill}
        stroke="currentColor" strokeWidth={strokeWidth}
        strokeLinecap="round" strokeLinejoin="round"
        style={{ pointerEvents: 'none', ...style }}
        className={className}
    >
        {children}
    </svg>
);



export const Logo = ({ size = 34 }: { size?: number }) => (
    <svg width={size} height={size} viewBox="0 0 34 34">
        <defs><linearGradient id="lg" x1="0" y1="0" x2="1" y2="1"><stop offset="0%" stopColor="var(--color-accent-stop1)" /><stop offset="100%" stopColor="var(--color-accent-stop2)" /></linearGradient></defs>
        <rect width="34" height="34" rx="10" fill="url(#lg)" />
        <path d="M10 17l5 5 9-10" stroke="var(--color-text)" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round" fill="none" />
    </svg>
);

export const Menu = (p: any) => <BaseSvg {...p}><line x1="3" y1="12" x2="21" y2="12" /><line x1="3" y1="6" x2="21" y2="6" /><line x1="3" y1="18" x2="21" y2="18" /></BaseSvg>;
export const Close = (p: any) => <BaseSvg {...p}><line x1="18" y1="6" x2="6" y2="18" /><line x1="6" y1="6" x2="18" y2="18" /></BaseSvg>;
export const Dash = (p: any) => <BaseSvg {...p}><rect x="3" y="3" width="7" height="7" rx="1" /><rect x="14" y="3" width="7" height="7" rx="1" /><rect x="14" y="14" width="7" height="7" rx="1" /><rect x="3" y="14" width="7" height="7" rx="1" /></BaseSvg>;
export const Org = (p: any) => <BaseSvg {...p}><path d="M3 21h18M5 21V8l7-5 7 5v13M10 21v-6h4v6" /></BaseSvg>;
export const User = (p: any) => <BaseSvg {...p}><path d="M20 21v-2a4 4 0 00-4-4H8a4 4 0 00-4 4v2" /><circle cx="12" cy="7" r="4" /></BaseSvg>;
export const Book = (p: any) => <BaseSvg {...p}><path d="M4 19.5A2.5 2.5 0 016.5 17H20" /><path d="M6.5 2h13.5v20H6.5A2.5 2.5 0 014 19.5v-15A2.5 2.5 0 016.5 2z" /></BaseSvg>;
export const Lock = (p: any) => <BaseSvg {...p}><rect x="3" y="11" width="18" height="11" rx="2" /><path d="M7 11V7a5 5 0 0110 0v4" /></BaseSvg>;
export const Cat = (p: any) => <BaseSvg {...p}><path d="M22 19a2 2 0 01-2 2H4a2 2 0 01-2-2V5a2 2 0 012-2h5l2 3h9a2 2 0 012 2z" /></BaseSvg>;
export const Grid = (p: any) => <BaseSvg {...p}><rect x="3" y="3" width="7" height="7" rx="1" /><rect x="14" y="3" width="7" height="7" rx="1" /><rect x="14" y="14" width="7" height="7" rx="1" /><rect x="3" y="14" width="7" height="7" rx="1" /></BaseSvg>;
export const Table = (p: any) => <BaseSvg {...p}><rect x="3" y="3" width="18" height="18" rx="2" /><path d="M3 9h18M9 9v12" /></BaseSvg>;
export const Trash = (p: any) => <BaseSvg {...p}><path d="M3 6h18m-2 0v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a2 2 0 012-2h4a2 2 0 012 2v2" /></BaseSvg>;
export const Logout = (p: any) => <BaseSvg {...p}><path d="M9 21H5a2 2 0 01-2-2V5a2 2 0 012-2h4M16 17l5-5-5-5M21 12H9" /></BaseSvg>;
export const Users = (p: any) => <BaseSvg {...p}><path d="M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2" /><circle cx="9" cy="7" r="4" /><path d="M23 21v-2a4 4 0 00-3-3.87M16 3.13a4 4 0 010 7.75" /></BaseSvg>;
export const Groups = (p: any) => <BaseSvg {...p}><path d="M13 2L3 14h9l-1 8 10-12h-9l1-8z" /></BaseSvg>;
export const Plus = (p: any) => <BaseSvg {...p}><line x1="12" y1="5" x2="12" y2="19" /><line x1="5" y1="12" x2="19" y2="12" /></BaseSvg>;
export const Video = (p: any) => <BaseSvg {...p}><polygon points="23 7 16 12 23 17 23 7" /><rect x="1" y="5" width="15" height="14" rx="2" ry="2" /></BaseSvg>;
export const Doc = (p: any) => <BaseSvg {...p}><path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z" /><polyline points="14 2 14 8 20 8" /></BaseSvg>;
export const Pdf = (p: any) => <BaseSvg {...p}><path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z" /><path d="M9 15h2a1 1 0 011 1v1a1 1 0 01-1 1H9v-3z" /><path d="M15 15h1a1 1 0 011 1v1a1 1 0 01-1 1h-1v-3z" /><line x1="12" y1="15" x2="12" y2="18" /></BaseSvg>;
export const Word = (p: any) => <BaseSvg {...p}><path d="M4 4h16v16H4z" /><path d="M9 16V8l3 5 3-5v8" /></BaseSvg>;
export const Excel = (p: any) => <BaseSvg {...p}><path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z" /><path d="M8 13l4 4m0-4l-4 4" /><path d="M16 13h-2v4h2" /><line x1="14" y1="15" x2="16" y2="15" /></BaseSvg>;
export const Sun = (p: any) => <BaseSvg {...p}><circle cx="12" cy="12" r="5" /><line x1="12" y1="1" x2="12" y2="3" /><line x1="12" y1="21" x2="12" y2="23" /><line x1="4.22" y1="4.22" /><line x1="5.64" y1="5.64" /><line x1="18.36" y1="18.36" /><line x1="19.78" y1="19.78" /><line x1="1" y1="12" x2="3" y2="12" /><line x1="21" y1="12" x2="23" y2="12" /><line x1="4.22" y1="19.78" x2="5.64" y2="18.36" /><line x1="18.36" y1="5.64" x2="19.78" y2="4.22" /></BaseSvg>;
export const Moon = (p: any) => <BaseSvg {...p}><path d="M21 12.79A9 9 0 1111.21 3 7 7 0 0021 12.79z" /></BaseSvg>;
export const Play = (p: any) => <BaseSvg {...p} fill="currentColor" strokeWidth={0}><polygon points="5 3 19 12 5 21 5 3" /></BaseSvg>;
export const Shield = (p: any) => <BaseSvg {...p}><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" /></BaseSvg>;
export const Check = (p: any) => <BaseSvg {...p} strokeWidth={3}><polyline points="20 6 9 17 4 12" /></BaseSvg>;
export const Eye = (p: any) => <BaseSvg {...p}><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" /><circle cx="12" cy="12" r="3" /></BaseSvg>;
export const EyeOff = (p: any) => <BaseSvg {...p}><path d="M17.94 17.94A10.07 10.07 0 0112 20c-7 0-11-8-11-8a18.45 18.45 0 015.06-5.94M9.9 4.24A9.12 9.12 0 0112 4c7 0 11 8 11 8a18.5 18.5 0 01-2.16 3.19m-6.72-1.07a3 3 0 11-4.24-4.24" /><line x1="1" y1="1" x2="23" y2="23" /></BaseSvg>;
export const Edit = (p: any) => <BaseSvg {...p}><path d="M11 4H4a2 2 0 00-2 2v14a2 2 0 002 2h14a2 2 0 002-2v-7" /><path d="M18.5 2.5a2.121 2.121 0 013 3L12 15l-4 1 1-4 9.5-9.5z" /></BaseSvg>;
export const Mail = (p: any) => <BaseSvg {...p}><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z" /><polyline points="22,6 12,13 2,6" /></BaseSvg>;
export const Next = (p: any) => <BaseSvg {...p}><polyline points="9 18 15 12 9 6" /></BaseSvg>;
export const Prev = (p: any) => <BaseSvg {...p}><polyline points="15 18 9 12 15 6" /></BaseSvg>;
export const Clock = (p: any) => <BaseSvg {...p}><circle cx="12" cy="12" r="10" /><polyline points="12 6 12 12 16 14" /></BaseSvg>;
export const Phone = (p: any) => <BaseSvg {...p}><path d="M22 16.92v3a2 2 0 01-2.18 2 19.79 19.79 0 01-8.63-3.07 19.5 19.5 0 01-6-6 19.79 19.79 0 01-3.07-8.67A2 2 0 014.11 2h3a2 2 0 012 1.72 12.84 12.84 0 00.7 2.81 2 2 0 01-.45 2.11L8.09 9.91a16 16 0 006 6l1.27-1.27a2 2 0 012.11-.45 12.84 12.84 0 002.81.7A2 2 0 0122 16.92z" /></BaseSvg>;
export const Loader = (p: any) => <BaseSvg {...p} style={{ animation: 'spin 1.5s linear infinite', ...p.style }}><line x1="12" y1="2" x2="12" y2="6" /><line x1="12" y1="18" x2="12" y2="22" /><line x1="4.93" y1="4.93" /><line x1="16.24" y1="16.24" /><line x1="2" y1="12" x2="6" y2="12" /><line x1="18" y1="12" x2="22" y2="12" /><line x1="4.93" y1="19.07" /><line x1="16.24" y1="7.76" /></BaseSvg>;
export const Search = (p: any) => <BaseSvg {...p}><circle cx="11" cy="11" r="8" /><line x1="21" y1="21" x2="16.65" y2="16.65" /></BaseSvg>;
export const ChevronDown = (p: any) => <BaseSvg {...p}><polyline points="6 9 12 15 18 9" /></BaseSvg>;
export const ChevronLeft = (p: any) => <BaseSvg {...p}><polyline points="15 18 9 12 15 6" /></BaseSvg>;
export const ChevronRight = (p: any) => <BaseSvg {...p}><polyline points="9 18 15 12 9 6" /></BaseSvg>;
export const Settings = (p: any) => <BaseSvg {...p}><circle cx="12" cy="12" r="3" /><path d="M19.4 15a1.65 1.65 0 00.33 1.82l.06.06a2 2 0 010 2.83 2 2 0 01-2.83 0l-.06-.06a1.65 1.65 0 00-1.82-.33 1.65 1.65 0 00-1 1.51V21a2 2 0 01-2 2 2 2 0 01-2-2v-.09A1.65 1.65 0 009 19.4a1.65 1.65 0 00-1.82.33l-.06.06a2 2 0 01-2.83 0 2 2 0 010-2.83l.06-.06a1.65 1.65 0 00.33-1.82 1.65 1.65 0 00-1.51-1H3a2 2 0 01-2-2 2 2 0 012-2h.09A1.65 1.65 0 004.6 9a1.65 1.65 0 00-.33-1.82l-.06-.06a2 2 0 010-2.83 2 2 0 012.83 0l.06.06a1.65 1.65 0 001.82.33H9a1.65 1.65 0 001-1.51V3a2 2 0 012-2 2 2 0 012 2v.09a1.65 1.65 0 001 1.51 1.65 1.65 0 001.82-.33l.06-.06a2 2 0 012.83 0 2 2 0 010 2.83l-.06.06a1.65 1.65 0 00-.33 1.82V9a1.65 1.65 0 001.51 1H21a2 2 0 012 2 2 2 0 01-2 2h-.09a1.65 1.65 0 00-1.51 1z" /></BaseSvg>;
export const Globe = (p: any) => <BaseSvg {...p}><circle cx="12" cy="12" r="10" /><line x1="2" y1="12" x2="22" y2="12" /><path d="M12 2a15.3 15.3 0 014 10 15.3 15.3 0 01-4 10 15.3 15.3 0 01-4-10 15.3 15.3 0 014-10z" /></BaseSvg>;
export const UserPlus = (p: any) => <BaseSvg {...p}><path d="M16 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2" /><circle cx="8" cy="7" r="4" /><line x1="20" y1="8" x2="20" y2="14" /><line x1="23" y1="11" x2="17" y2="11" /></BaseSvg>;
export const Alert = (p: any) => <BaseSvg {...p}><circle cx="12" cy="12" r="10" /><line x1="12" y1="8" x2="12" y2="12" /><line x1="12" y1="16" x2="12.01" y2="16" /></BaseSvg>;
export const Message = (p: any) => <BaseSvg {...p}><path d="M21 11.5a8.38 8.38 0 01-.9 3.8 8.5 8.5 0 01-7.6 4.7 8.38 8.38 0 01-3.8-.9L3 21l1.9-5.7a8.38 8.38 0 01-.9-3.8 8.5 8.5 0 014.7-7.6 8.38 8.38 0 013.8-.9h.5a8.48 8.48 0 018 8v.5z" /></BaseSvg>;
export const X = Close;

export const StatIcons: Record<string, React.FC<any>> = {
    Orgs: Org,
    Cats: Cat,
    Users: Users,
    Courses: Book,
    Groups: Groups,
    Media: Video,
    Security: Lock,
};

export const BarChart = (p: any) => <BaseSvg {...p}><line x1="18" y1="20" x2="18" y2="10" /><line x1="12" y1="20" x2="12" y2="4" /><line x1="6" y1="20" x2="6" y2="14" /></BaseSvg>;
export const Maximize = (p: any) => <BaseSvg {...p}><path d="M8 3H5a2 2 0 00-2 2v3m18 0V5a2 2 0 00-2-2h-3m0 18h3a2 2 0 002-2v-3M3 16v3a2 2 0 002 2h3" /></BaseSvg>;
export const Copy = (p: any) => <BaseSvg {...p}><rect x="9" y="9" width="13" height="13" rx="2" ry="2" /><path d="M5 15H4a2 2 0 01-2-2V4a2 2 0 012-2h9a2 2 0 012 2v1" /></BaseSvg>;
export const Link = (p: any) => <BaseSvg {...p}><path d="M10 13a5 5 0 007.54.54l3-3a5 5 0 00-7.07-7.07l-1.72 1.71" /><path d="M14 11a5 5 0 00-7.54-.54l-3 3a5 5 0 007.07 7.07l1.71-1.71" /></BaseSvg>;

// Summary object for convenience
export const Icons = {
    Logo, Menu, Close, Dash, Org, User, Book, Lock, Cat, Grid, Table, Trash, Logout, Users, Groups, Plus, Video, Doc, Pdf, Word, Excel, Sun, Moon, Play, Shield, Check, Eye, EyeOff, Edit, Mail, Next, Prev, Clock, Phone, Loader, Search, ChevronDown, ChevronLeft, ChevronRight, Settings, Globe, UserPlus, Alert, Maximize, BarChart, X, Message, Copy, Link
};
