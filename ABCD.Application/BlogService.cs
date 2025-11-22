using System.Collections.Immutable;

using ABCD.Domain;
using ABCD.Lib;
using ABCD.Lib.Exceptions;
using ABCD.Application.Models;

namespace ABCD.Application {
    public class BlogService : IBlogService {
        private readonly RequestContext _context;
        private readonly IBlogRepository _repository;
        public BlogService(RequestContext context, IBlogRepository repository) {
            _context = context;
            _repository = repository;
        }

        public async Task<BlogModel> GetBlogByIdAsync(int blogId) {
            var blog = await FindBlogByIdAsync(blogId);
            return new BlogModel(
                blog.BlogId,
                blog.Name,
                blog.Description,
                blog.Domains.Select(d => d.Domain.Name).ToImmutableList()
            );
        }

        public async Task<BlogModel> UpdateBlogAsync(BlogModel blogModel) {
            var blog = await FindBlogByIdAsync(blogModel.BlogId);
            blog.Name = blogModel.Name;
            blog.Description = blogModel.Description;
            blog.ClearDomains();

            foreach (var domain in blogModel.Domains) {
                blog.AddDomain(domain);
            }

            var updated = await _repository.UpdateAsync(blog);

            return new BlogModel(
                updated.BlogId,
                updated.Name,
                updated.Description,
                updated.Domains.Select(d => d.Domain.Name).ToImmutableList()
            );
        }

        private async Task<Blog> FindBlogByIdAsync(int blogId) {
            var blog = await _repository.GetByIdAsync(blogId);
            if (blog == null)
                throw new BlogNotFoundException($"Blog with ID {blogId} not found.");

            return blog;
        }
    }
}
