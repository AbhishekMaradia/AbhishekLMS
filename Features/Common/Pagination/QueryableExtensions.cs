using Microsoft.EntityFrameworkCore;

namespace LMS_SoulCode.Features.Common.Pagination
{
    public static class QueryableExtensions
    {
        public static async Task<(IEnumerable<T> Items, int TotalCount)> ToPagedListAsync<T>(this IQueryable<T> source, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var totalCount = await source.CountAsync(cancellationToken);

            var items = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
