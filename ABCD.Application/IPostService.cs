using ABCD.Domain;

namespace ABCD.Application {
    public interface IPostService {
        Task<Post> CreatePostAsync(CreatePostCommand command);
        Task<IEnumerable<Post>> GetAllAsync();
        Task<Post?> GetByIdAsync(int postId);
        Task<Post> MoveFragmentAsync(MoveFragmentCommand command);
        Task<Post> AddFragmentAsync(AddFragmentCommand command);
        Task<Post> UpdatePostAsync(UpdatePostCommand command);
        Task<Post> UpdateFragmentAsync(UpdateFragmentCommand command);
        Task<Post> DeleteFragmentAsync(DeleteFragmentCommand command);
        Task<Post> TogglePostStatusAsync(TogglePostStatusCommand command);
    }
}
