namespace ABCD.Application {
    public record CreatePostCommand(string Title, string Path);

    public record DeleteFragmentCommand(int PostId, int FragmentId, string Version);

    public record MoveFragmentCommand(int PostId, int FragmentId, int NewPosition, string Version);

    public record UpdateFragmentCommand(int PostId, int FragmentId, string Content, string Version);

    public record UpdatePostCommand(int PostId, string Title, string Synopsis, string PathSegment, string Version);
}