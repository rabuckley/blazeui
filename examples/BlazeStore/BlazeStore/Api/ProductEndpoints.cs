using BlazeStore.Client.Services;

namespace BlazeStore.Api;

internal static class ProductEndpoints
{
    internal static void MapProductApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/products");

        group.MapGet("/", async (string? category, string? q, IProductService products) =>
        {
            if (!string.IsNullOrWhiteSpace(q))
                return Results.Ok(await products.SearchAsync(q));

            if (!string.IsNullOrWhiteSpace(category))
                return Results.Ok(await products.GetByCategoryAsync(category));

            return Results.Ok(await products.GetAllAsync());
        });

        group.MapGet("/categories", async (IProductService products) =>
            Results.Ok(await products.GetCategoriesAsync()));

        group.MapGet("/{slug}", async (string slug, IProductService products) =>
        {
            var product = await products.GetBySlugAsync(slug);
            return product is not null ? Results.Ok(product) : Results.NotFound();
        });
    }
}
