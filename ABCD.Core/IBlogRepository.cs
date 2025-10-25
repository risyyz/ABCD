namespace ABCD.Core {
    public interface IBlogRepository {
        Task<Blog?> GetByIdAsync(int blogId);
        Task<Blog> UpdateAsync(Blog blog);
    }
}
