namespace ABCD.Application.Exceptions
{
    public class VersionConflictException : Exception
    {
        public VersionConflictException() : base("Version conflict detected.") { }
        public VersionConflictException(string message) : base(message) { }
        public VersionConflictException(string message, Exception innerException) : base(message, innerException) { }
    }
}
