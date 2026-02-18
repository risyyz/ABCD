using ABCD.Domain;

namespace ABCD.Application {
    public interface IPostService {
        Task<Post> CreatePostAsync(CreatePostCommand command);
        Task<IEnumerable<Post>> GetAllAsync();
        Task<Post?> GetByIdAsync(int postId);
        Task<Post> UpdateFragmentPositionAsync(ChangeFragmentPositionCommand command);
        Task<Post> AddFragmentAsync(AddFragmentCommand command);
        Task<Post> UpdateFragmentAsync(int postId, int fragmentId, string content, string version);
        Task<Post> DeleteFragmentAsync(DeleteFragmentCommand command);
    }

    public record CreatePostCommand(string Title, string Path);

    public record ChangeFragmentPositionCommand(int PostId, int FragmentId, int NewPosition, string Version);
}
