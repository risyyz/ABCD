
using ABCD.Domain;
using Microsoft.EntityFrameworkCore;

namespace ABCD.Infra.Data {
    public class PostRepository : IPostRepository {
        private readonly DataContext _context;
        public PostRepository(DataContext context) => _context = context;

        public async Task<Post?> GetByPostIdAsync(int blogId, int postId) {
            var record = await _context.Posts
                .Include(p => p.Fragments) // Assuming navigation property Fragments exists
                .FirstOrDefaultAsync(p => p.PostId == postId && p.BlogId == blogId);
            if (record == null) return null;
            return MapToDomain(record);
        }

        public async Task<Post> AddAsync(Post post) {
            var record = MapToRecord(post);
            _context.Posts.Add(record);
            await _context.SaveChangesAsync();
            return MapToDomain(record);
        }

        public async Task<IEnumerable<Post>> GetAllByBlogIdAsync(int blogId) {
            var records = await _context.Posts
                .Where(p => p.BlogId == blogId)
                .ToListAsync();
            return records.Select(MapToDomain);
        }

        public async Task<Post?> GetByBlogIdAndPathSegmentAsync(int blogId, string path) {
            var record = await _context.Posts
                .FirstOrDefaultAsync(p => p.BlogId == blogId && p.PathSegment == path);

            if (record == null)
                return null;

            return MapToDomain(record);
        }

        public async Task<Post?> GetByBlogIdAndTitleAsync(int blogId, string postTitle) {
            var record = await _context.Posts
                .FirstOrDefaultAsync(p =>
                    p.BlogId == blogId &&
                    EF.Functions.Like(p.Title, postTitle));

            if (record == null)
                return null;

            return MapToDomain(record);
        }

        // Map EF record to domain model
        private Post MapToDomain(PostRecord record) {
            // TODO: Map all required fields and relationships
            var fragments = new List<Fragment>();
            if (record.Fragments != null) {
                foreach (var fragmentRecord in record.Fragments) {
                    // Assuming Fragment is a domain model and you have a method to map from record to domain
                    fragments.Add(MapToDomain(fragmentRecord));
                }
            }
            var post = new Post(new BlogId(record.BlogId), new PostId(record.PostId), record.Title, (PostStatus)record.Status, null, fragments);            
            return post;
        }

        private Fragment MapToDomain(FragmentRecord fragmentRecord) {
            return new Fragment(
                new FragmentId(fragmentRecord.FragmentId),
                new PostId(fragmentRecord.PostId),
                (FragmentType)fragmentRecord.FragmentType,
                fragmentRecord.Position
            );
        }

        // Map domain model to EF record
        private PostRecord MapToRecord(Post post) {
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
