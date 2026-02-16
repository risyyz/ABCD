namespace ABCD.Application {
    public record DeleteFragmentCommand(
        int PostId,
        int FragmentId,
        string Version
    );
}
