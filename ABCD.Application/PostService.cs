using ABCD.Application.Exceptions;
using ABCD.Domain;

namespace ABCD.Application {
    public class PostService : IPostService {
        private readonly RequestContext _requestContext;
        private readonly IPostRepository _postRepository;
        private readonly IBlogRepository _blogRepository;

        public PostService(RequestContext requestContext, IPostRepository postRepository, IBlogRepository blogRepository) {
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
            if(_requestContext.Blog == null)
                throw new ArgumentException("RequestContext must have a Blog set.", nameof(requestContext));
            else if(_requestContext.Blog.BlogId == null)
                throw new ArgumentException("RequestContext.Blog must have a valid BlogId", nameof(requestContext));

            _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));
            _blogRepository = blogRepository ?? throw new ArgumentNullException(nameof(blogRepository));
        }

        public async Task<Post> CreatePostAsync(PostCreateRequest request) {
            var post = new Post(_requestContext.Blog.BlogId, request.Title) {
                PathSegment = new PathSegment(request.Path) 
            };

            var existingPostWithSamePath = await _postRepository.GetByBlogIdAndPathSegmentAsync(_requestContext.Blog.BlogId.Value!, post.PathSegment!);
            if(existingPostWithSamePath != null) 
                throw new DuplicatePathSegmentException($"A post with the path segment '{post.PathSegment}' already exists in this blog.");

            // Persist
            await _postRepository.AddAsync(post);
            return post;
        }

        public async Task<IEnumerable<Post>> GetAllAsync()
        {
            return await _postRepository.GetAllByBlogIdAsync(_requestContext.Blog.BlogId.Value!);
        }
    }
}
