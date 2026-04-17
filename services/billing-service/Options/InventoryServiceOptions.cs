namespace BillingService.Options;

public sealed class InventoryServiceOptions
{
    public const string SectionName = "Services";

    public string InventoryServiceBaseUrl { get; set; } = string.Empty;
}
