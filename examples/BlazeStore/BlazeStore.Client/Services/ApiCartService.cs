using System.Net.Http.Json;
using BlazeStore.Client.Models;

namespace BlazeStore.Client.Services;

internal sealed class ApiCartService : ICartService
{
    private readonly HttpClient _http;

    public ApiCartService(HttpClient http)
    {
        _http = http;
    }

    public event Action? OnChange;

    public async Task<Cart> GetCartAsync() =>
        await _http.GetFromJsonAsync<Cart>("/api/cart") ?? new Cart([]);

    public async Task AddAsync(int productId, int quantity = 1)
    {
        await _http.PostAsJsonAsync("/api/cart/items", new { productId, quantity });
        OnChange?.Invoke();
    }

    public async Task RemoveAsync(int productId)
    {
        await _http.DeleteAsync($"/api/cart/items/{productId}");
        OnChange?.Invoke();
    }

    public async Task UpdateQuantityAsync(int productId, int quantity)
    {
        await _http.PutAsJsonAsync($"/api/cart/items/{productId}", new { quantity });
        OnChange?.Invoke();
    }

    public async Task ClearAsync()
    {
        await _http.DeleteAsync("/api/cart");
        OnChange?.Invoke();
    }
}
