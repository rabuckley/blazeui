using BlazeStore.Client.Models;
using BlazeStore.Client.Services;

namespace BlazeStore.Data;

/// <summary>
/// In-memory product store seeded with sample data. Implements
/// <see cref="IProductService"/> so SSR pages can query products
/// directly without an HTTP round-trip.
/// </summary>
internal sealed class ProductRepository : IProductService
{
    private static readonly List<Product> Products =
    [
        new(1, "Wireless Headphones", "wireless-headphones",
            "Premium over-ear wireless headphones with active noise cancellation and 30-hour battery life.",
            149.99m, "https://picsum.photos/seed/wireless-headphones/400/400", "Audio", 4.7, 234),

        new(2, "Mechanical Keyboard", "mechanical-keyboard",
            "Compact 75% mechanical keyboard with hot-swappable switches and RGB backlighting.",
            89.99m, "https://picsum.photos/seed/mechanical-keyboard/400/400", "Peripherals", 4.5, 189),

        new(3, "USB-C Hub", "usb-c-hub",
            "7-in-1 USB-C hub with 4K HDMI, USB 3.0 ports, SD card reader, and 100W passthrough charging.",
            49.99m, "https://picsum.photos/seed/usb-c-hub/400/400", "Accessories", 4.3, 312),

        new(4, "Portable Monitor", "portable-monitor",
            "15.6\" portable IPS monitor with USB-C connectivity. Perfect for dual-screen setups on the go.",
            229.99m, "https://picsum.photos/seed/portable-monitor/400/400", "Displays", 4.6, 156),

        new(5, "Webcam HD", "webcam-hd",
            "1080p webcam with built-in ring light and noise-cancelling microphone for crystal-clear video calls.",
            69.99m, "https://picsum.photos/seed/webcam-hd/400/400", "Peripherals", 4.4, 278),

        new(6, "Desk Lamp", "desk-lamp",
            "LED desk lamp with adjustable color temperature, brightness levels, and wireless charging base.",
            59.99m, "https://picsum.photos/seed/desk-lamp/400/400", "Accessories", 4.2, 145),

        new(7, "Bluetooth Speaker", "bluetooth-speaker",
            "Waterproof portable Bluetooth speaker with 360-degree sound and 12-hour battery life.",
            79.99m, "https://picsum.photos/seed/bluetooth-speaker/400/400", "Audio", 4.5, 423),

        new(8, "Wireless Mouse", "wireless-mouse",
            "Ergonomic wireless mouse with adjustable DPI, silent clicks, and multi-device Bluetooth support.",
            39.99m, "https://picsum.photos/seed/wireless-mouse/400/400", "Peripherals", 4.6, 567),

        new(9, "Monitor Arm", "monitor-arm",
            "Gas-spring monitor arm supporting up to 32\" displays with full range of motion and cable management.",
            44.99m, "https://picsum.photos/seed/monitor-arm/400/400", "Accessories", 4.4, 198),

        new(10, "Noise Cancelling Earbuds", "noise-cancelling-earbuds",
            "True wireless earbuds with hybrid ANC, transparency mode, and customisable EQ profiles.",
            119.99m, "https://picsum.photos/seed/noise-cancelling-earbuds/400/400", "Audio", 4.8, 891),

        new(11, "Ultrawide Monitor", "ultrawide-monitor",
            "34\" curved ultrawide QHD monitor with 144Hz refresh rate and HDR400 support.",
            449.99m, "https://picsum.photos/seed/ultrawide-monitor/400/400", "Displays", 4.7, 312),

        new(12, "Laptop Stand", "laptop-stand",
            "Aluminium laptop stand with adjustable height and angle. Improves airflow and ergonomics.",
            34.99m, "https://picsum.photos/seed/laptop-stand/400/400", "Accessories", 4.3, 234),
    ];

    public Task<IReadOnlyList<Product>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<Product>>(Products);

    public Task<IReadOnlyList<Product>> GetByCategoryAsync(string category) =>
        Task.FromResult<IReadOnlyList<Product>>(
            Products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList());

    public Task<Product?> GetBySlugAsync(string slug) =>
        Task.FromResult(Products.FirstOrDefault(p => p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase)));

    public Task<IReadOnlyList<Product>> SearchAsync(string query) =>
        Task.FromResult<IReadOnlyList<Product>>(
            Products.Where(p =>
                p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList());

    public Task<IReadOnlyList<string>> GetCategoriesAsync() =>
        Task.FromResult<IReadOnlyList<string>>(Products.Select(p => p.Category).Distinct().Order().ToList());
}
