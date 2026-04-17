namespace BillingService.Contracts;

public sealed record LookupProductsRequest(List<Guid> ProductIds);

public sealed record ProductSnapshotResponse(Guid Id, string Code, string Description, int Stock);

public sealed record ConsumeStockRequest(Guid InvoiceId, int InvoiceNumber, List<ConsumeStockItemRequest> Items);

public sealed record ConsumeStockItemRequest(Guid ProductId, int Quantity);
