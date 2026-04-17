namespace InventoryService.Models;

public sealed class Product
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Stock { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
