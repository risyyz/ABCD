using ABCD.Domain;

namespace ABCD.Infra.Data {
    public class BlogRepository : IBlogRepository {
        private readonly DataContext _context;
        public BlogRepository(DataContext context) => _context = context;

        public async Task<Blog?> GetByIdAsync(int blogId) {
            throw new NotImplementedException();            
        }

        public async Task<Blog> UpdateAsync(Blog blog) {
            throw new NotImplementedException();
        }
    }
}
