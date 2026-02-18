namespace ABCD.Domain {
    public interface IPostRepository {
        Task<Post?> GetByPostIdAsync(int blogId, int postId);
        Task<Post> AddAsync(Post post);
        Task<IEnumerable<Post>> GetAllByBlogIdAsync(int blogId); // Returns all posts for a blog
        Task<Post?> GetByBlogIdAndPathSegmentAsync(int blogId, string path);
        Task<Post?> GetByBlogIdAndTitleAsync(int blogId, string title);
        Task<Post> MoveFragmentsAsync(Post post, IEnumerable<Fragment> fragments);
        Task<Post> UpdateFragmentAsync(Post post, Fragment fragment);
        Task<Post> UpdatePostAsync(Post post);
    }
}
