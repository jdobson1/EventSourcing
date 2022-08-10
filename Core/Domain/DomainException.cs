namespace Core.Domain
{
    public class DomainException : ApplicationException
    {
        public DomainException(string message) : base(message)
        {

        }

        public override string? StackTrace => string.Empty;
    }
}
