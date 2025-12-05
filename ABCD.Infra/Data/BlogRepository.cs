using ABCD.Domain;
using Microsoft.EntityFrameworkCore;

namespace ABCD.Infra.Data {
    public class BlogRepository : IBlogRepository {
        private readonly DataContext _context;
        public BlogRepository(DataContext context) => _context = context;

        public async Task<Blog?> GetByIdAsync(int blogId) {
            var record = await _context.Blogs
                .Include(b => b.Domains)
                .FirstOrDefaultAsync(b => b.BlogId == blogId);
            if (record == null) return null;
            return MapToDomain(record);
        }

        public async Task<Blog> UpdateAsync(Blog blog) {
            var record = await _context.Blogs
                .Include(b => b.Domains)
                .FirstOrDefaultAsync(b => b.BlogId == blog.BlogId.Value);
            if (record == null) throw new InvalidOperationException($"Blog {blog.BlogId.Value} not found");
            // Update fields
            record.Name = blog.Name;
            record.Description = blog.Description;
            // Domains sync (simple replace for demo)
            record.Domains.Clear();
            foreach (var d in blog.Domains)
                record.Domains.Add(new DomainRecord { BlogId = record.BlogId, Domain = d.DomainName });
            // Save
            await _context.SaveChangesAsync();
            return MapToDomain(record);
        }

        private static Blog MapToDomain(BlogRecord record) {
            var blog = new Blog(new BlogId(record.BlogId)) { Name = record.Name, Description = record.Description };
            foreach (var d in record.Domains)
                blog.AddDomain(new BlogDomain(d.Domain));
            return blog;
        }
    }
}
