using ABCD.Domain;

namespace ABCD.Application {
    public interface IPostService {
        Task<Post> CreatePostAsync(CreatePostCommand command);
        Task<IEnumerable<Post>> GetAllAsync();
        Task<IEnumerable<Post>> GetPublishedAsync(int limit, int skip);
        Task<Post?> GetByIdAsync(int postId);
        Task<Post?> GetPublishedByPathSegmentAsync(string pathSegment);
        Task<Post> MoveFragmentAsync(MoveFragmentCommand command);
        Task<Post> AddFragmentAsync(AddFragmentCommand command);
        Task<Post> UpdatePostAsync(UpdatePostCommand command);
        Task<Post> UpdateFragmentAsync(UpdateFragmentCommand command);
        Task<Post> DeleteFragmentAsync(DeleteFragmentCommand command);
        Task<Post> TogglePostStatusAsync(TogglePostStatusCommand command);
        Task<IEnumerable<Post>> SearchAsync(string searchTerm, int? excludePostId = null);
    }
}
