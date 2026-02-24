# High-Scale Video Progress Tracking Plan

**Objective:** Optimize `UserVideoProgress` to handle 2000+ concurrent students per organization with minimum database load and 100% data integrity.

---

## 1. The Problem: "The Heartbeat Bottleneck"
Tracking video progress by hitting the API every 10 seconds (Heartbeat) causes massive load:
- **Load @ 2000 students:** ~12,000 requests per minute per organization.
- **Risk:** Database locks, high CPU, and duplicate entries if a student opens multiple tabs/devices.
- **Data Inconsistency:** If a student rewinds, progress might incorrectly move backwards in the DB.

## 2. The Solution: "Strategic Milestones"
We shift the "intelligence" to the Frontend and keep the Backend "Smart & Lean".

### A. Frontend Strategy (Player Side)
1. **Local Tracking:** Save current playback time in `localStorage` every 2 seconds. (Cost: 0 Server Hits).
2. **First Milestone (50%):** Call API once when the user reaches 50% of the video.
3. **Completion Milestone (95%+):** Call API once when the user reaches 95%. Mark as `IsCompleted = true`.
4. **The Safety Net (Exit):** Use `navigator.sendBeacon` to send the final progress only when the user closes the tab or navigates away.

---

## 3. Backend Implementation (C#)

### Phase 1: Database Integrity (Strictness)
Prevent duplicate records at the database level using a Unique Constraint.
```csharp
// Path: Data/LmsDbContext.cs
modelBuilder.Entity<UserVideoProgress>()
    .HasIndex(p => new { p.UserId, p.VideoId })
    .IsUnique();
```

### Phase 2: Idempotent Repository Logic (Smart Update)
The repository ensures that progress only moves forward and never overwrites completion status with an accidental lower value.

```csharp
// Path: Features/CourseVideos/Repositories/CourseVideoRepository.cs

public async Task UpdateProgressAsync(UserVideoProgress progress, CancellationToken ct)
{
    // 1. Find existing record (Ignore filters to avoid missing soft-deleted ones)
    var existing = await _context.UserVideoProgresses
        .IgnoreQueryFilters() 
        .FirstOrDefaultAsync(p => p.UserId == progress.UserId && p.VideoId == progress.VideoId, ct);

    if (existing == null)
    {
        // 2. New entry for this User-Video pair
        _context.UserVideoProgresses.Add(progress);
    }
    else
    {
        // 3. Smart Update Logic:
        
        // Progress should only move FORWARD
        if (progress.WatchedPercentage > existing.WatchedPercentage)
            existing.WatchedPercentage = progress.WatchedPercentage;

        // Completion status is a "One-Way Street" (Once true, stays true)
        if (!existing.IsCompleted && progress.IsCompleted)
            existing.IsCompleted = true;

        existing.LastWatchedAt = DateTime.UtcNow;
        existing.IsDeleted = false; // Restore if it was previously soft-deleted
        existing.TenantId = progress.TenantId; // Sync tenant just in case
    }
    await _context.SaveChangesAsync(ct);
}
```

---

## 4. Why this works for 2,000+ Students?
1. **95% Load Reduction:** Hits per video drop from **~60** to just **2 or 3**.
2. **Race-Condition Proof:** The Unique Index and `FirstOrDefault` check ensure that even if 2 requests hit at the same millisecond, the data remains consistent.
3. **Accurate Reporting:** Since we only move percentage forward and lock completion, your "Course Progress Reports" will always show the maximum reached state.
4. **Existing Data Support:** This logic is backward compatible. It will find old records and "upgrade" them without creating new messy entries.

## 5. Implementation Roadmap
1. [ ] **Cleanup:** Run a SQL script to merge any existing duplicate `(UserId, VideoId)` pairs.
2. [ ] **Schema Update:** Apply the `Unique Index` via EF Migration.
3. [ ] **Repo Update:** Replace the current `UpdateProgressAsync` with the "Smart Logic" provided above.
4. [ ] **Frontend Update:** Set up the milestone triggers (50%, 95%) in the Video Player.
