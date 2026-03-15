using BlazeStore.Client.Models;

namespace BlazeStore.Client.Services;

public interface IProductService
{
    Task<IReadOnlyList<Product>> GetAllAsync();
    Task<IReadOnlyList<Product>> GetByCategoryAsync(string category);
    Task<Product?> GetBySlugAsync(string slug);
    Task<IReadOnlyList<Product>> SearchAsync(string query);
    Task<IReadOnlyList<string>> GetCategoriesAsync();
}
