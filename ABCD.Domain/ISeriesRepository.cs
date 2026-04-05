namespace ABCD.Domain {
    public interface ISeriesRepository {
        Task<Series?> GetByIdAsync(int blogId, int seriesId);
        Task<Series> AddAsync(Series series);
        Task<IEnumerable<Series>> GetAllByBlogIdAsync(int blogId);
        Task<IEnumerable<Series>> GetPublishedByBlogIdAsync(int blogId, int limit, int skip);
        Task<Series?> GetByBlogIdAndPathSegmentAsync(int blogId, string pathSegment);
        Task<Series?> GetByBlogIdAndTitleAsync(int blogId, string title);
        Task<Series> UpdateAsync(Series series);
        Task<Series> UpdateStatusAsync(Series series);
        Task<Series> UpdatePostAssignmentsAsync(Series series);
    }
}
