namespace ABCD.Application.Exceptions {
    public class DuplicateSeriesTitleException : Exception {
        public DuplicateSeriesTitleException(string message) : base(message) { }
    }
}
