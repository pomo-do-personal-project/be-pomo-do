namespace be_pomo_do.Core.Exceptions;

// Represents business domain rule violation errors.
public class DomainException : Exception
{
    // Initializes a new instance of the DomainException class.
    public DomainException(string message) : base(message)
    {
    }
}
