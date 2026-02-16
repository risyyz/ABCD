using ABCD.Application.Exceptions;
using ABCD.Domain;

namespace ABCD.Application {
    public class PostService : IPostService {
        private readonly RequestContext _requestContext;
        private readonly IPostRepository _postRepository;

        public PostService(RequestContext requestContext, IPostRepository postRepository) {
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
            if(_requestContext.Blog == null)
                throw new ArgumentException("RequestContext must have a Blog set.", nameof(requestContext));
            else if(_requestContext.Blog.BlogId == null)
                throw new ArgumentException("RequestContext.Blog must have a valid BlogId", nameof(requestContext));

            _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));
        }

        public async Task<Post> AddFragmentAsync(AddFragmentCommand command) {
            var post = await TryGetPostByIdAndVersion(command.PostId, command.Version);
            var fragment = post.GetFragmentById(command.AfterFragmentId);
            post.AddFragment(command.FragmentType, fragment?.Position + 1);
            return await _postRepository.UpdatePostAsync(post);
        }

        public async Task<Post> CreatePostAsync(CreatePostCommand command) {
            var blogId = _requestContext.Blog.BlogId.Value!;
            var post = new Post(_requestContext.Blog.BlogId, command.Title) { PathSegment = new PathSegment(command.Path) };

            // Check for duplicate title (case-insensitive)
            var existingPostWithSameTitle = await _postRepository.GetByBlogIdAndTitleAsync(blogId, command.Title);
            if (existingPostWithSameTitle != null)
                throw new DuplicatePostTitleException($"A post with the title '{command.Title}' already exists in this blog.");            

            var existingPostWithSamePath = await _postRepository.GetByBlogIdAndPathSegmentAsync(blogId, post.PathSegment.Value!);
            if (existingPostWithSamePath != null) 
                throw new DuplicatePathSegmentException($"A post with the path segment '{post.PathSegment}' already exists in this blog.");

            // Persist
            await _postRepository.AddAsync(post);
            return post;
        }

        public async Task<Post> DeleteFragmentAsync(DeleteFragmentCommand command) {
            var post = await TryGetPostByIdAndVersion(command.PostId, command.Version);
            post.RemoveFragment(command.FragmentId);
            return await _postRepository.UpdatePostAsync(post);
        }

        public async Task<IEnumerable<Post>> GetAllAsync()
        {
            return await _postRepository.GetAllByBlogIdAsync(_requestContext.Blog.BlogId.Value!);
        }

        public async Task<Post?> GetByIdAsync(int postId) {
            return await _postRepository.GetByPostIdAsync(_requestContext.Blog.BlogId.Value!, postId);
        }

        public async Task<Post> UpdateFragmentPositionAsync(ChangeFragmentPositionCommand command) {
            var post = await TryGetPostByIdAndVersion(command.PostId, command.Version);
            var impactedFragments = post.ChangeFragmentPosition(command.FragmentId, command.NewPosition);
            return await _postRepository.UpdateFragmentPositionAsync(post, impactedFragments);
        }

        private async Task<Post> TryGetPostByIdAndVersion(int postId, string version) {
            var post = await _postRepository.GetByPostIdAsync(_requestContext.Blog.BlogId.Value!, postId);
            if (post == null) throw new PostNotFoundException($"Post {postId} does not exist.");
            // Version check
            if (!string.IsNullOrWhiteSpace(version) && post.Version != null && !string.Equals(version, post.Version.HexString, StringComparison.Ordinal))
                throw new VersionConflictException("The resource was updated by another process. Please reload and try again.");
            
            return post;
        }
    }
}
