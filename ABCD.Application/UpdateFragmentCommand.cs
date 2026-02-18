namespace ABCD.Application {
    public record UpdateFragmentCommand(int PostId, int FragmentId, string Content, string Version);
}
