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
            var post = new Post(new BlogId(record.BlogId), new PostId(record.PostId), 
                                record.Title, (PostStatus)record.Status, null, fragments) { 
                PathSegment = record.PathSegment != null ? new PathSegment(record.PathSegment) : null,
                Version = new VersionToken(record.Version)
            };            
            return post;
        }

        private Fragment MapToDomain(FragmentRecord fragmentRecord) {
            return new Fragment(
                new FragmentId(fragmentRecord.FragmentId),
                new PostId(fragmentRecord.PostId),
                (FragmentType)fragmentRecord.FragmentType,
                fragmentRecord.Position
            ) { 
                Content = fragmentRecord.Content
            };
        }

        // Map domain model to EF record
        private PostRecord MapToRecord(Post post) {
            return new PostRecord {
                PostId = post.PostId?.Value ?? 0,
                BlogId = post.BlogId.Value,
                Title = post.Title,
                Status = post.Status,
                PathSegment = post.PathSegment?.Value,
                Version = post.Version?.Value ?? Array.Empty<byte>(),
                Fragments = post.Fragments.Select(MapToRecord).ToList()
            };
        }

        private FragmentRecord MapToRecord(Fragment fragment) {
            return new FragmentRecord {
                FragmentId = fragment.FragmentId?.Value ?? 0,
                PostId = fragment.PostId.Value,
                Position = fragment.Position,
                Content = fragment.Content ?? string.Empty,
                FragmentType = fragment.FragmentType
            };
        }

        public async Task<Post> UpdateFragmentPositionAsync(Post post, IEnumerable<Fragment> fragments) {
            var utcNow = DateTime.UtcNow;
            using var transaction = await _context.Database.BeginTransactionAsync();

            var fragmentList = fragments.ToList();
            if (fragmentList.Count == 2) {
                var first = fragmentList[0];
                var second = fragmentList[1];

                // Step 1: Set the first fragment's position to a temporary value
                var trackedFirst = await _context.Fragments.FindAsync(first.FragmentId!.Value);
                if (trackedFirst != null) {
                    trackedFirst.Position = -1;
                    trackedFirst.UpdatedDate = utcNow;
                    _context.Entry(trackedFirst).Property(f => f.Position).IsModified = true;
                    _context.Entry(trackedFirst).Property(f => f.UpdatedDate).IsModified = true;
                }
                await _context.SaveChangesAsync();

                // Step 2: Set the second fragment's position to the first's original position
                var trackedSecond = await _context.Fragments.FindAsync(second.FragmentId!.Value);
                if (trackedSecond != null) {
                    trackedSecond.Position = second.Position;
                    trackedSecond.UpdatedDate = utcNow;
                    _context.Entry(trackedSecond).Property(f => f.Position).IsModified = true;
                    _context.Entry(trackedSecond).Property(f => f.UpdatedDate).IsModified = true;
                }
                await _context.SaveChangesAsync();

                // Step 3: Set the first fragment's position to the second's original position
                if (trackedFirst != null) {
                    trackedFirst.Position = first.Position;
                    trackedFirst.UpdatedDate = utcNow;
                    _context.Entry(trackedFirst).Property(f => f.Position).IsModified = true;
                    _context.Entry(trackedFirst).Property(f => f.UpdatedDate).IsModified = true;
                }
                await _context.SaveChangesAsync();
            } 

            // Update the post's last updated date
            var trackedPost = await _context.Posts.FindAsync(post.PostId!.Value);
            if (trackedPost != null) {
                trackedPost.UpdatedDate = utcNow;
                _context.Entry(trackedPost).Property(p => p.UpdatedDate).IsModified = true;
            }
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return await GetByPostIdAsync(post.BlogId!.Value, post.PostId!.Value);
        }

        public async Task<Post> UpdatePostAsync(Post post) {
            var trackedPost = await _context.Posts.Include(p => p.Fragments).FirstOrDefaultAsync(p => p.PostId == post.PostId!.Value);
            if (trackedPost == null) throw new ArgumentException("Post not found", nameof(post));

            trackedPost.Title = post.Title;
            trackedPost.Status = post.Status;
            trackedPost.PathSegment = post.PathSegment?.Value;
            trackedPost.UpdatedDate = DateTime.UtcNow;

            _context.Entry(trackedPost).State = EntityState.Modified;

            //remove fragments that are no longer in the post
            var fragmentsToRemove = trackedPost.Fragments.Where(f => !post.Fragments.Any(pf => pf.FragmentId?.Value == f.FragmentId)).ToList();
            foreach(var fragment in fragmentsToRemove) {
                _context.Fragments.Remove(fragment);
            }

            //add or update fragments
            foreach(var fragment in post.Fragments) {
                var trackedFragment = trackedPost.Fragments.FirstOrDefault(f => f.FragmentId == fragment.FragmentId?.Value);
                if (trackedFragment != null) {
                    //update existing fragment
                    trackedFragment.Content = fragment.Content ?? string.Empty;
                    trackedFragment.Position = fragment.Position;
                    trackedFragment.UpdatedDate = DateTime.UtcNow;
                    _context.Entry(trackedFragment).State = EntityState.Modified;
                } else {
                    //add new fragment
                    var newFragmentRecord = MapToRecord(fragment);
                    newFragmentRecord.PostId = trackedPost.PostId; // ensure the new fragment is linked to the post
                    _context.Fragments.Add(newFragmentRecord);
                }
            }

            await _context.SaveChangesAsync();
            return await GetByPostIdAsync(post.BlogId!.Value, post.PostId!.Value);
        }
    }
}
