using ABCD.Domain;
using ABCD.Lib;
using ABCD.Lib.Exceptions;

namespace ABCD.Application {
    public class BlogService : IBlogService {
        private readonly RequestContext _context;
        private readonly IBlogRepository _repository;
        public BlogService(RequestContext context, IBlogRepository repository) {
            _context = context;
            _repository = repository;
        }

        public async Task<Blog> GetBlogByIdAsync(int blogId) {
            var blog = await _repository.GetByIdAsync(blogId);
            if (blog == null)
                throw new BlogNotFoundException($"Blog with Id {blogId} does not exist.");

            return blog;
        }

        public Task<Blog> UpdateBlogAsync(Blog blog) {
            throw new NotImplementedException();
        }
    }
}
