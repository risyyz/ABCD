using ABCD.Application.Exceptions;
using ABCD.Domain;

namespace ABCD.Application {
    public class SeriesService : ISeriesService {
        private readonly RequestContext _requestContext;
        private readonly ISeriesRepository _seriesRepository;
        private readonly IPostRepository _postRepository;

        public SeriesService(RequestContext requestContext, ISeriesRepository seriesRepository, IPostRepository postRepository) {
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
            if (_requestContext.Blog == null)
                throw new ArgumentException("RequestContext must have a Blog set.", nameof(requestContext));
            else if (_requestContext.Blog.BlogId == null)
                throw new ArgumentException("RequestContext.Blog must have a valid BlogId", nameof(requestContext));

            _seriesRepository = seriesRepository ?? throw new ArgumentNullException(nameof(seriesRepository));
            _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));
        }

        public async Task<Series> CreateSeriesAsync(CreateSeriesCommand command) {
            var blogId = _requestContext.Blog.BlogId;
            var series = new Series(blogId, command.Title) {
                PathSegment = new PathSegment(command.Path),
                Description = command.Description
            };

            var existingWithSameTitle = await _seriesRepository.GetByBlogIdAndTitleAsync(blogId.Value, command.Title);
            if (existingWithSameTitle != null)
                throw new DuplicateSeriesTitleException($"A series with the title '{command.Title}' already exists in this blog.");

            var existingWithSamePath = await _seriesRepository.GetByBlogIdAndPathSegmentAsync(blogId.Value, series.PathSegment.Value);
            if (existingWithSamePath != null)
                throw new DuplicatePathSegmentException($"A series with the path segment '{series.PathSegment}' already exists in this blog.");

            return await _seriesRepository.AddAsync(series);
        }

        public async Task<IEnumerable<Series>> GetAllAsync() {
            return await _seriesRepository.GetAllByBlogIdAsync(_requestContext.Blog.BlogId.Value);
        }

        public async Task<IEnumerable<Series>> GetPublishedAsync(int limit, int skip) {
            return await _seriesRepository.GetPublishedByBlogIdAsync(_requestContext.Blog.BlogId.Value, limit, skip);
        }

        public async Task<Series?> GetByIdAsync(int seriesId) {
            return await _seriesRepository.GetByIdAsync(_requestContext.Blog.BlogId.Value, seriesId);
        }

        public async Task<Series?> GetPublishedByPathSegmentAsync(string pathSegment) {
            var series = await _seriesRepository.GetByBlogIdAndPathSegmentAsync(_requestContext.Blog.BlogId.Value, pathSegment);
            return series?.Status == SeriesStatus.Published ? series : null;
        }

        public async Task<Series> UpdateSeriesAsync(UpdateSeriesCommand command) {
            var series = await TryGetSeriesByIdAndVersion(command.SeriesId, command.Version);

            var seriesWithSamePath = await _seriesRepository.GetByBlogIdAndPathSegmentAsync(_requestContext.Blog.BlogId.Value, command.PathSegment?.Trim());
            if (seriesWithSamePath != null && seriesWithSamePath.SeriesId != series.SeriesId)
                throw new DuplicatePathSegmentException($"A series with the path segment '{command.PathSegment}' already exists in this blog.");

            series.Title = command.Title;
            series.Description = command.Description;
            series.PathSegment = new PathSegment(command.PathSegment);
            return await _seriesRepository.UpdateAsync(series);
        }

        public async Task<Series> ToggleSeriesStatusAsync(ToggleSeriesStatusCommand command) {
            var series = await TryGetSeriesByIdAndVersion(command.SeriesId, command.Version);
            if (series.Status == SeriesStatus.Published)
                series.UnPublish();
            else
                series.Publish();
            return await _seriesRepository.UpdateStatusAsync(series);
        }

        public async Task<Series> AddPostToSeriesAsync(AddPostToSeriesCommand command) {
            var series = await TryGetSeriesByIdAndVersion(command.SeriesId, command.Version);
            var post = await _postRepository.GetByPostIdAsync(_requestContext.Blog.BlogId.Value, command.PostId);
            if (post == null)
                throw new PostNotFoundException($"Post {command.PostId} does not exist.");

            series.AddPost(post, command.Position);
            return await _seriesRepository.UpdatePostAssignmentsAsync(series);
        }

        public async Task<Series> RemovePostFromSeriesAsync(RemovePostFromSeriesCommand command) {
            var series = await TryGetSeriesByIdAndVersion(command.SeriesId, command.Version);
            var post = series.Posts.FirstOrDefault(p => p.PostId?.Value == command.PostId);
            if (post == null)
                throw new PostNotFoundException($"Post {command.PostId} is not in this series.");

            series.RemovePost(post);
            return await _seriesRepository.UpdatePostAssignmentsAsync(series);
        }

        private async Task<Series> TryGetSeriesByIdAndVersion(int seriesId, string version) {
            var series = await _seriesRepository.GetByIdAsync(_requestContext.Blog.BlogId.Value, seriesId);
            if (series == null) throw new SeriesNotFoundException($"Series {seriesId} does not exist.");
            if (!string.IsNullOrWhiteSpace(version) && series.Version != null && !string.Equals(version, series.Version.HexString, StringComparison.Ordinal))
                throw new VersionConflictException("The resource was updated by another process. Please reload and try again.");
            return series;
        }
    }
}
