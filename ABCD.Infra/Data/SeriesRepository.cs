using ABCD.Domain;
using Microsoft.EntityFrameworkCore;

namespace ABCD.Infra.Data {
    public class SeriesRepository : ISeriesRepository {
        private readonly DataContext _context;
        public SeriesRepository(DataContext context) => _context = context;

        public async Task<Series?> GetByIdAsync(int blogId, int seriesId) {
            var record = await _context.Series
                .Include(s => s.Posts)
                .FirstOrDefaultAsync(s => s.SeriesId == seriesId && s.BlogId == blogId);
            if (record == null) return null;
            return MapToDomain(record);
        }

        public async Task<Series> AddAsync(Series series) {
            var record = MapToRecord(series);
            var utcNow = DateTime.UtcNow;
            record.CreatedDate = utcNow;
            record.UpdatedDate = utcNow;
            _context.Series.Add(record);
            await _context.SaveChangesAsync();
            return MapToDomain(record);
        }

        public async Task<IEnumerable<Series>> GetAllByBlogIdAsync(int blogId) {
            var records = await _context.Series
                .Include(s => s.Posts)
                .Where(s => s.BlogId == blogId)
                .ToListAsync();
            return records.Select(MapToDomain);
        }

        public async Task<IEnumerable<Series>> GetPublishedByBlogIdAsync(int blogId, int limit, int skip) {
            var records = await _context.Series
                .Include(s => s.Posts.Where(p => p.Status == PostStatus.Published))
                .Where(s => s.BlogId == blogId && s.Status == SeriesStatus.Published)
                .OrderByDescending(s => s.DateLastPublished)
                .Skip(skip)
                .Take(limit)
                .ToListAsync();
            return records.Select(MapToDomain);
        }

        public async Task<Series?> GetByBlogIdAndPathSegmentAsync(int blogId, string pathSegment) {
            var record = await _context.Series
                .Include(s => s.Posts)
                .FirstOrDefaultAsync(s => s.BlogId == blogId && s.PathSegment == pathSegment);
            if (record == null) return null;
            return MapToDomain(record);
        }

        public async Task<Series?> GetByBlogIdAndTitleAsync(int blogId, string title) {
            var record = await _context.Series
                .FirstOrDefaultAsync(s => s.BlogId == blogId && EF.Functions.Like(s.Title, title));
            if (record == null) return null;
            return MapToDomain(record);
        }

        public async Task<Series> UpdateAsync(Series series) {
            var tracked = await _context.Series.FindAsync(series.SeriesId!.Value);
            if (tracked == null) throw new ArgumentException("Series not found", nameof(series));

            tracked.Title = series.Title;
            tracked.Description = series.Description;
            tracked.PathSegment = series.PathSegment?.Value;
            tracked.UpdatedDate = DateTime.UtcNow;
            _context.Entry(tracked).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return await GetByIdAsync(series.BlogId.Value, series.SeriesId!.Value)
                ?? throw new ArgumentException("Series not found after update");
        }

        public async Task<Series> UpdateStatusAsync(Series series) {
            var tracked = await _context.Series.FindAsync(series.SeriesId!.Value);
            if (tracked == null) throw new ArgumentException("Series not found", nameof(series));

            tracked.Status = series.Status;
            tracked.DateLastPublished = series.DateLastPublished;
            tracked.UpdatedDate = DateTime.UtcNow;
            _context.Entry(tracked).Property(s => s.Status).IsModified = true;
            _context.Entry(tracked).Property(s => s.DateLastPublished).IsModified = true;
            _context.Entry(tracked).Property(s => s.UpdatedDate).IsModified = true;
            await _context.SaveChangesAsync();
            return await GetByIdAsync(series.BlogId.Value, series.SeriesId!.Value)
                ?? throw new ArgumentException("Series not found after update");
        }

        public async Task<Series> UpdatePostAssignmentsAsync(Series series) {
            var trackedPosts = await _context.Posts
                .Where(p => p.BlogId == series.BlogId.Value &&
                       (p.SeriesId == series.SeriesId!.Value || series.Posts.Select(sp => sp.PostId!.Value).Contains(p.PostId)))
                .ToListAsync();

            foreach (var trackedPost in trackedPosts) {
                var domainPost = series.Posts.FirstOrDefault(p => p.PostId!.Value == trackedPost.PostId);
                if (domainPost != null) {
                    trackedPost.SeriesId = series.SeriesId!.Value;
                    trackedPost.SeriesPosition = domainPost.SeriesPosition;
                } else {
                    // Post was removed from series
                    trackedPost.SeriesId = null;
                    trackedPost.SeriesPosition = null;
                }
                trackedPost.UpdatedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return await GetByIdAsync(series.BlogId.Value, series.SeriesId!.Value)
                ?? throw new ArgumentException("Series not found after update");
        }

        private Series MapToDomain(SeriesRecord record) {
            var posts = record.Posts?.Select(MapPostToDomain) ?? Enumerable.Empty<Post>();

            var series = new Series(new BlogId(record.BlogId), new SeriesId(record.SeriesId),
                                    record.Title, (SeriesStatus)record.Status, record.DateLastPublished, posts) {
                PathSegment = record.PathSegment != null ? new PathSegment(record.PathSegment) : null,
                Description = record.Description,
                Version = new VersionToken(record.Version)
            };
            return series;
        }

        private static Post MapPostToDomain(PostRecord record) {
            var post = new Post(new BlogId(record.BlogId), new PostId(record.PostId),
                                record.Title, record.Status, record.DateLastPublished) {
                PathSegment = record.PathSegment != null ? new PathSegment(record.PathSegment) : null,
                Version = new VersionToken(record.Version)
            };
            if (record.SeriesId != null && record.SeriesPosition != null)
                post.AssignToSeries(new SeriesId(record.SeriesId.Value), record.SeriesPosition.Value);
            return post;
        }

        private SeriesRecord MapToRecord(Series series) {
            return new SeriesRecord {
                SeriesId = series.SeriesId?.Value ?? 0,
                BlogId = series.BlogId.Value,
                Title = series.Title,
                Description = series.Description,
                PathSegment = series.PathSegment?.Value,
                Status = series.Status,
                DateLastPublished = series.DateLastPublished,
                Version = series.Version?.Value ?? Array.Empty<byte>()
            };
        }
    }
}
