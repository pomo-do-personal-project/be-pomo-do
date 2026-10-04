namespace be_pomo_do.Core.Exceptions;

/// <summary>Represents business domain rule violation errors.</summary>
public class DomainException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="DomainException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    public DomainException(string message) : base(message)
    {
    }
}
