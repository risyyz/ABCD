namespace ABCD.Domain.Exceptions {
    /// <summary>
    /// Use this exception to indicate that an operation is not allowed in the current context or state. For example, trying to publish a blog post that has no title or content, or trying to delete a blog post that is already published.
    /// Prefer this custom exception over the built-in IllegalOperationException to provide more specific error messages and to allow for better error handling in the application. For example, you can catch this exception in the ExceptionHandlingMiddleware and return a 400 Bad Request status code with a clear error message to the client.
    /// .Net framework can also throw IllegalOperationException in some cases, such as when trying to access a disposed object or when trying to modify a collection while enumerating it. In those cases, you can catch the IllegalOperationException and rethrow an IllegalOperationException with a more specific message that indicates the cause of the error.
    /// </summary>
    public class IllegalOperationException : Exception {
        public IllegalOperationException(string message) : base(message) { }
    }
}
