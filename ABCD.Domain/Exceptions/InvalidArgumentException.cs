namespace ABCD.Domain.Exceptions {
    public class InvalidArgumentException : ArgumentException {
        public InvalidArgumentException(string message, string paramName)
            : base(message, paramName) {
        }
    }
}
