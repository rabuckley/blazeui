using BlazeStore.Data;

namespace BlazeStore.Api;

internal static class CartEndpoints
{
    private const string CartCookieName = "blazestore-cart";

    private static string GetCartId(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue(CartCookieName, out var id) && !string.IsNullOrEmpty(id))
            return id;

        id = Guid.NewGuid().ToString("N");
        context.Response.Cookies.Append(CartCookieName, id, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.FromDays(30)
        });
        return id;
    }

    internal static void MapCartApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/cart");

        group.MapGet("/", (HttpContext context, CartRepository cart) =>
            Results.Ok(cart.GetCart(GetCartId(context))));

        group.MapPost("/items", (HttpContext context, CartRepository cart, AddItemRequest request) =>
        {
            cart.AddItem(GetCartId(context), request.ProductId, request.Quantity);
            return Results.Ok(cart.GetCart(GetCartId(context)));
        });

        group.MapPut("/items/{productId:int}", (HttpContext context, CartRepository cart, int productId, UpdateQuantityRequest request) =>
        {
            cart.UpdateQuantity(GetCartId(context), productId, request.Quantity);
            return Results.Ok(cart.GetCart(GetCartId(context)));
        });

        group.MapDelete("/items/{productId:int}", (HttpContext context, CartRepository cart, int productId) =>
        {
            cart.RemoveItem(GetCartId(context), productId);
            return Results.Ok(cart.GetCart(GetCartId(context)));
        });

        group.MapDelete("/", (HttpContext context, CartRepository cart) =>
        {
            cart.Clear(GetCartId(context));
            return Results.Ok(cart.GetCart(GetCartId(context)));
        });
    }

    private record AddItemRequest(int ProductId, int Quantity = 1);
    private record UpdateQuantityRequest(int Quantity);
}
