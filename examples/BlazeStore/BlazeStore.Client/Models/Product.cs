namespace BlazeStore.Client.Models;

public record Product(
    int Id,
    string Name,
    string Slug,
    string Description,
    decimal Price,
    string ImageUrl,
    string Category,
    double Rating,
    int ReviewCount);
