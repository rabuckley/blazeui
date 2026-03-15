using BlazeStore.Client.Models;
using BlazeStore.Client.Services;

namespace BlazeStore.Services;

/// <summary>
/// Stub <see cref="ICartService"/> used during SSR prerendering. Returns an
/// empty cart so interactive WASM islands can render their initial state.
/// The real cart data loads client-side after WASM hydration.
/// </summary>
internal sealed class SsrCartService : ICartService
{
    // Never fires during SSR — required by the interface contract.
#pragma warning disable CS0067
    public event Action? OnChange;
#pragma warning restore CS0067

    public Task<Cart> GetCartAsync() => Task.FromResult(new Cart([]));
    public Task AddAsync(int productId, int quantity = 1) => Task.CompletedTask;
    public Task RemoveAsync(int productId) => Task.CompletedTask;
    public Task UpdateQuantityAsync(int productId, int quantity) => Task.CompletedTask;
    public Task ClearAsync() => Task.CompletedTask;
}
