using ABCD.Domain;

namespace ABCD.Application {
    public interface ISeriesService {
        Task<Series> CreateSeriesAsync(CreateSeriesCommand command);
        Task<IEnumerable<Series>> GetAllAsync();
        Task<IEnumerable<Series>> GetPublishedAsync(int limit, int skip);
        Task<Series?> GetByIdAsync(int seriesId);
        Task<Series?> GetPublishedByPathSegmentAsync(string pathSegment);
        Task<Series> UpdateSeriesAsync(UpdateSeriesCommand command);
        Task<Series> ToggleSeriesStatusAsync(ToggleSeriesStatusCommand command);
        Task<Series> AddPostToSeriesAsync(AddPostToSeriesCommand command);
        Task<Series> RemovePostFromSeriesAsync(RemovePostFromSeriesCommand command);
    }
}
