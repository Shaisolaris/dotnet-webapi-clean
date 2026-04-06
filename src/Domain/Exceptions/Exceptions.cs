namespace Domain.Exceptions;
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string entity, object key)
        : base($"Entity \"{entity}\" ({key}) was not found.") { }
}
