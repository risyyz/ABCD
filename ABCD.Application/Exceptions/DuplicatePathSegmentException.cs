namespace ABCD.Application.Exceptions {
    public class DuplicatePathSegmentException : Exception {
        public DuplicatePathSegmentException(string message) : base(message) { }
    }
}
