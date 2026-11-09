namespace InventoryManagement.Application.Abstractions;

public interface ICurrentUser
{
    Guid UserId { get; }
}
