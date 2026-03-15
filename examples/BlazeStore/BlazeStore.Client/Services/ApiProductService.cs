using System.Net.Http.Json;
using BlazeStore.Client.Models;

namespace BlazeStore.Client.Services;

internal sealed class ApiProductService : IProductService
{
    private readonly HttpClient _http;

    public ApiProductService(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync() =>
        await _http.GetFromJsonAsync<List<Product>>("/api/products") ?? [];

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(string category) =>
        await _http.GetFromJsonAsync<List<Product>>($"/api/products?category={Uri.EscapeDataString(category)}") ?? [];

    public async Task<Product?> GetBySlugAsync(string slug) =>
        await _http.GetFromJsonAsync<Product>($"/api/products/{Uri.EscapeDataString(slug)}");

    public async Task<IReadOnlyList<Product>> SearchAsync(string query) =>
        await _http.GetFromJsonAsync<List<Product>>($"/api/products?q={Uri.EscapeDataString(query)}") ?? [];

    public async Task<IReadOnlyList<string>> GetCategoriesAsync() =>
        await _http.GetFromJsonAsync<List<string>>("/api/products/categories") ?? [];
}
