using ABCD.Domain;

namespace ABCD.Application {
    public interface IPostService {
        Task<Post> CreatePostAsync(CreatePostCommand command);
        Task<IEnumerable<Post>> GetAllAsync();
        Task<Post?> GetByIdAsync(int postId);
        Task<Post> MoveFragmentAsync(MoveFragmentCommand command);
        Task<Post> AddFragmentAsync(AddFragmentCommand command);
        Task<Post> UpdateFragmentAsync(UpdateFragmentCommand command);
        Task<Post> DeleteFragmentAsync(DeleteFragmentCommand command);
    }

    public record CreatePostCommand(string Title, string Path);

    public record MoveFragmentCommand(int PostId, int FragmentId, int NewPosition, string Version);
}
