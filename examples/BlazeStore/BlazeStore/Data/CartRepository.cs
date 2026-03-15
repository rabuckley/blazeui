using System.Collections.Concurrent;
using BlazeStore.Client.Models;

namespace BlazeStore.Data;

/// <summary>
/// In-memory cart store keyed by a cart ID (stored in a browser cookie).
/// Thread-safe for concurrent requests.
/// </summary>
internal sealed class CartRepository
{
    private readonly ConcurrentDictionary<string, List<CartItem>> _carts = new();
    private readonly IServiceProvider _services;

    public CartRepository(IServiceProvider services)
    {
        _services = services;
    }

    public Cart GetCart(string cartId)
    {
        var items = _carts.GetOrAdd(cartId, _ => []);
        lock (items)
        {
            return new Cart(items.ToList());
        }
    }

    public void AddItem(string cartId, int productId, int quantity)
    {
        var items = _carts.GetOrAdd(cartId, _ => []);
        var productService = _services.GetRequiredService<Client.Services.IProductService>();
        var product = productService.GetAllAsync().GetAwaiter().GetResult()
            .FirstOrDefault(p => p.Id == productId);

        if (product is null) return;

        lock (items)
        {
            var existing = items.FirstOrDefault(i => i.Product.Id == productId);
            if (existing is not null)
            {
                items.Remove(existing);
                items.Add(existing with { Quantity = existing.Quantity + quantity });
            }
            else
            {
                items.Add(new CartItem(product, quantity));
            }
        }
    }

    public void RemoveItem(string cartId, int productId)
    {
        if (!_carts.TryGetValue(cartId, out var items)) return;
        lock (items)
        {
            items.RemoveAll(i => i.Product.Id == productId);
        }
    }

    public void UpdateQuantity(string cartId, int productId, int quantity)
    {
        if (!_carts.TryGetValue(cartId, out var items)) return;
        lock (items)
        {
            var existing = items.FirstOrDefault(i => i.Product.Id == productId);
            if (existing is null) return;

            if (quantity <= 0)
            {
                items.Remove(existing);
            }
            else
            {
                items.Remove(existing);
                items.Add(existing with { Quantity = quantity });
            }
        }
    }

    public void Clear(string cartId)
    {
        if (!_carts.TryGetValue(cartId, out var items)) return;
        lock (items)
        {
            items.Clear();
        }
    }
}
