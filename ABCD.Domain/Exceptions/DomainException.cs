namespace ABCD.Domain.Exceptions
{
    /// <summary>
    /// general exception for domain layer
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
        public DomainException(string message, Exception innerException) : base(message, innerException) { }
    }
}
