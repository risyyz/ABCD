using ABCD.Domain;

namespace ABCD.Application {
    public interface IBlogService {
        Task<Blog> GetBlogByIdAsync(int blogId);
        Task<Blog> UpdateBlogAsync(Blog blog);
    }
}
