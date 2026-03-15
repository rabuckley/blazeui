using BlazeStore.Client.Models;

namespace BlazeStore.Client.Services;

public interface ICartService
{
    Task<Cart> GetCartAsync();
    Task AddAsync(int productId, int quantity = 1);
    Task RemoveAsync(int productId);
    Task UpdateQuantityAsync(int productId, int quantity);
    Task ClearAsync();

    event Action? OnChange;
}
