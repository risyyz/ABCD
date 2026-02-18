namespace ABCD.Application.Exceptions {
    public class BlogNotFoundException : Exception {
        public BlogNotFoundException(string message) : base(message) { }
    }

    public class PostNotFoundException : Exception {
        public PostNotFoundException(string message) : base(message) { }
    }

    public class FragmentNotFoundException : Exception {
        public FragmentNotFoundException(string message) : base(message) { }
    }
}
