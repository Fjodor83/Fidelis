namespace Fidelity.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object key)
        : base($"Entity \"{entityName}\" ({key}) was not found.")
    {
    }
}

public class ValidationException : DomainException
{
    public ValidationException(string message) : base(message)
    {
    }
    
    public Dictionary<string, string[]> Errors { get; } = new();
    
    public ValidationException(Dictionary<string, string[]> errors)
        : base("One or more validation failures occurred.")
    {
        Errors = errors;
    }
}
