namespace InventoryService.Contracts;

public sealed record LookupProductsRequest(List<Guid> ProductIds);

public sealed record ConsumeStockRequest(Guid InvoiceId, int InvoiceNumber, List<ConsumeStockItemRequest> Items);

public sealed record ConsumeStockItemRequest(Guid ProductId, int Quantity);

public sealed record FailureSimulationRequest(bool Enabled);

public sealed record FailureSimulationResponse(bool Enabled);
