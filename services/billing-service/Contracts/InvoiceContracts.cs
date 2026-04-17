using BillingService.Models;

namespace BillingService.Contracts;

public sealed record CreateInvoiceRequest(List<CreateInvoiceItemRequest> Items);

public sealed record CreateInvoiceItemRequest(Guid ProductId, int Quantity);

public sealed record InvoiceResponse(
    Guid Id,
    int Number,
    InvoiceStatus Status,
    DateTime CreatedAtUtc,
    DateTime? ClosedAtUtc,
    IReadOnlyList<InvoiceItemResponse> Items);

public sealed record InvoiceItemResponse(
    Guid ProductId,
    string ProductCode,
    string ProductDescription,
    int Quantity);
