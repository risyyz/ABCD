namespace ABCD.Domain.Exceptions
{
    public class FragmentPositionException : DomainException
    {
        public FragmentPositionException(string message) : base(message) { }
        public FragmentPositionException(string message, Exception innerException) : base(message, innerException) { }
    }
}
