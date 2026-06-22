# LMS Studio: Responsive Audit & Fix Tracker

This document tracks the progress of making the LMS Studio fully responsive across all devices, ensuring a premium "Senior Designer" approved experience.

## Status Overview
- [x] Global Framework (Sidebar, Hamburger, Overlay)
- [x] Basic Table Scrolling
- [ ] Module-specific Audits
    - [ ] Dashboard: Stat grids and layout
    - [ ] Organizations: Table columns & Grid view
    - [ ] Categories: Search/Filter stacking
    - [ ] Users: Action buttons & Avatar sizing
    - [ ] Groups: Switcher & List alignment
    - [ ] Courses: Pricing & Actions display
    - [ ] Security: Complex matrix & Role management
- [ ] Modal System: Full-screen mobile transition
- [ ] Form System: Vertical stacking & Full-width inputs

---

## Detailed Audit Logs

### 1. Global Utilities & Foundation (`index.css`)
- [x] Fix sidebar breakpoint (768px for tablet/mobile).
- [x] Add Sidebar backdrop/overlay for mobile.
- [x] Ensure `lms-flex-row` stacks on mobile.
- [ ] Add `.lms-hide-mobile` utility.
- [ ] Add `.lms-stack-mobile` utility.

### 2. Header & Search
- [x] Hamburger at 1001 z-index.
- [ ] Compact User Pill on mobile (Hide text, keep avatar).
- [ ] Search bar should expand to 100% width on mobile.

### 3. Module: Dashboard
- [ ] Stat cards currently use a fixed grid? Need to ensure `grid-template-columns: 1fr` on small mobile.
- [ ] Trend charts (if any) need `overflow` handling.

### 4. Tables & Lists
- [x] Horizontal scroll container.
- [ ] Hide 'Code' or 'ID' or 'Organization' columns on mobile (under 500px).
- [ ] Ensure Action buttons don't wrap onto new lines inside cells.

### 5. Standard Modals
- [x] Set `width: 95%` globally.
- [ ] Enforce `max-height: 90vh` with internal scrolling for long forms.
