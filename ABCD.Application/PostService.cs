using ABCD.Application.Exceptions;
using ABCD.Domain;

namespace ABCD.Application {
    public class PostService : IPostService {
        private readonly RequestContext _requestContext;
        private readonly IPostRepository _postRepository;
        private readonly IBlogRepository _blogRepository;

        public PostService(RequestContext requestContext, IPostRepository postRepository, IBlogRepository blogRepository) {
            _requestContext = requestContext;
            _postRepository = postRepository;
            _blogRepository = blogRepository;
        }

        public async Task<Post> CreatePostAsync(PostCreateRequest request) {
            // Validate blog exists
            var blog = await _blogRepository.GetByIdAsync(request.BlogId);
            if (blog == null)
                throw new BlogNotFoundException($"Blog with ID {request.BlogId} does not exist.");

            // Create Post aggregate
            var post = new Post(blog.BlogId, request.Title);
            post.PathSegment = new PathSegment(request.Path);

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
