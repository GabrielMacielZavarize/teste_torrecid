using InventoryManagement.Domain.Exceptions;

namespace InventoryManagement.Application.Exceptions;

public sealed class ConflictException : DomainException
{
    public ConflictException(string message) : base(message)
    {
    }
}
