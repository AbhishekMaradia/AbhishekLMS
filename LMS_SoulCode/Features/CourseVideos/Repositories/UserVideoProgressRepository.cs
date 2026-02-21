using LMS_SoulCode.Data;
using LMS_SoulCode.Features.CourseVideos.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS_SoulCode.Features.CourseVideos.Repositories
{
    public class UserVideoProgressRepository
    {
        private readonly LmsDbContext _context;

        public UserVideoProgressRepository(LmsDbContext context)
        {
            _context = context;
        }

        public async Task<UserVideoProgress?> GetAsync(int userId, int videoId)
        => await _context.UserVideoProgresses
                .FirstOrDefaultAsync(x => x.UserId == userId && x.VideoId == videoId);
        

        public async Task AddOrUpdateAsync(UserVideoProgress progress)
        {
            var existing = await GetAsync(progress.UserId, progress.VideoId);
            if (existing == null)
                _context.UserVideoProgresses.Add(progress);
            else
            {
                existing.WatchedPercentage = progress.WatchedPercentage;
                existing.IsCompleted = progress.IsCompleted;
                existing.LastWatchedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}
