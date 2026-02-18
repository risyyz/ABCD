namespace ABCD.Application.Exceptions {
    public class DuplicatePostTitleException : Exception {
        public DuplicatePostTitleException(string message) : base(message) { }
    }
}
