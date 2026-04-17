using BillingService.Contracts;
using BillingService.Models;

namespace BillingService.Services;

public interface IInventoryApiClient
{
    Task<IReadOnlyList<ProductSnapshotResponse>> GetProductsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken);

    Task ConsumeStockAsync(Invoice invoice, CancellationToken cancellationToken);
}
