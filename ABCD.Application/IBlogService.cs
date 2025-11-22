using ABCD.Application.Models;

namespace ABCD.Application {
    public interface IBlogService {
        Task<BlogModel> GetBlogByIdAsync(int blogId);
        Task<BlogModel> UpdateBlogAsync(BlogModel blogModel);
    }
}
