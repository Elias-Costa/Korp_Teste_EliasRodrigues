namespace BillingService.Models;

public sealed class Invoice
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
    public List<InvoiceItem> Items { get; set; } = [];
}
