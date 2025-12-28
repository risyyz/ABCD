using ABCD.Domain;

using Microsoft.EntityFrameworkCore;

namespace ABCD.Infra.Data {
    public class PostRepository : IPostRepository {
        private readonly DataContext _context;
        public PostRepository(DataContext context) => _context = context;

        public async Task<Post?> GetByIdAsync(int postId) {
            var record = await _context.Posts
                .FirstOrDefaultAsync(p => p.PostId == postId);
            if (record == null) return null;
            return MapToDomain(record);
        }

        public async Task<Post> AddAsync(Post post) {
            var record = MapToRecord(post);
            _context.Posts.Add(record);
            await _context.SaveChangesAsync();
            return MapToDomain(record);
        }

        // Map EF record to domain model
        private static Post MapToDomain(PostRecord record) {
            // TODO: Map all required fields and relationships
            return new Post(new BlogId(record.BlogId), new PostId(record.PostId), record.Title, (PostStatus)record.Status);
        }

        // Map domain model to EF record
        private static PostRecord MapToRecord(Post post) {
            // TODO: Map all required fields and relationships
            return new PostRecord {
                PostId = post.PostId?.Value ?? 0,
                BlogId = post.BlogId.Value,
                Title = post.Title,
                Status = post.Status,
                PathSegment = post.PathSegment?.Value,
                //DateLastPublished = post.DateLastPublished.HasValue ? post.DateLastPublished.Value : null                
            };
        }
    }
}
