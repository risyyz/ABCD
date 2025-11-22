namespace ABCD.Domain {
    public interface IBlogRepository {
        Task<Blog?> GetByIdAsync(int blogId);
        Task<Blog> UpdateAsync(Blog blog);
    }
}
