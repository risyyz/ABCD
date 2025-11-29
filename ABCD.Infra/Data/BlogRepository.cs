using ABCD.Domain;

using Microsoft.EntityFrameworkCore;

namespace ABCD.Infra.Data {
    public class BlogRepository : IBlogRepository {
        private readonly DataContext _context;
        public BlogRepository(DataContext context) => _context = context;

        public async Task<Blog?> GetByIdAsync(int blogId) {
            throw new NotImplementedException();
            //return await _context.Blogs
            //    .AsNoTracking()
            //    .Include(b => b.Domains)
            //    .FirstOrDefaultAsync(b => b.BlogId == blogId);
        }

        public async Task<Blog> UpdateAsync(Blog blog) {
            var tracked = _context.Blogs.Local.FirstOrDefault(b => b.BlogId == blog.BlogId);
            if (tracked == null) {
                _context.Blogs.Attach(blog);
                _context.Entry(blog).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            return blog;
        }
    }
}
