using ABCD.Services.Models;

namespace ABCD.Services {
    public interface IBlogService {
        Task<BlogModel> GetBlogByIdAsync(int blogId);
        Task<BlogModel> UpdateBlogAsync(BlogModel blogModel);
    }
}
