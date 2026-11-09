using InventoryManagement.Domain.Exceptions;

namespace InventoryManagement.Application.Exceptions;

public sealed class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException() : base("Usuário ou senha inválidos.")
    {
    }
}
