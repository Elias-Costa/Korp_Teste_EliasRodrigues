namespace InventoryService.Contracts;

public sealed record CreateProductRequest(string Code, string Description, int Stock);

public sealed record UpdateProductRequest(string Code, string Description, int Stock);

public sealed record ProductResponse(
    Guid Id,
    string Code,
    string Description,
    int Stock,
    DateTime CreatedAtUtc);

public sealed record ProductSnapshotResponse(
    Guid Id,
    string Code,
    string Description,
    int Stock);
