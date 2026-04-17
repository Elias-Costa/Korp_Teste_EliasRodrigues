using InventoryService.Contracts;
using InventoryService.Data;
using InventoryService.Exceptions;
using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Services;

public sealed class ProductService(
    InventoryDbContext dbContext,
    FailureSimulationState failureSimulationState,
    ILogger<ProductService> logger)
{
    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var products = await dbContext.Products
            .AsNoTracking()
            .OrderBy(product => product.Code)
            .ToListAsync(cancellationToken);

        return products.Select(MapToResponse).ToList();
    }

    public async Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException($"Product '{id}' was not found.");
        }

        return MapToResponse(product);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        ValidateProductPayload(request.Code, request.Description, request.Stock);

        var normalizedCode = request.Code.Trim();
        var normalizedDescription = request.Description.Trim();

        var codeAlreadyExists = await dbContext.Products
            .AnyAsync(product => product.Code == normalizedCode, cancellationToken);

        if (codeAlreadyExists)
        {
            throw new ConflictException($"A product with code '{normalizedCode}' already exists.");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Code = normalizedCode,
            Description = normalizedDescription,
            Stock = request.Stock,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(product);
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        ValidateProductPayload(request.Code, request.Description, request.Stock);

        var product = await dbContext.Products.FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException($"Product '{id}' was not found.");
        }

        var normalizedCode = request.Code.Trim();
        var normalizedDescription = request.Description.Trim();

        var codeAlreadyExists = await dbContext.Products
            .AnyAsync(
                current => current.Id != id && current.Code == normalizedCode,
                cancellationToken);

        if (codeAlreadyExists)
        {
            throw new ConflictException($"A product with code '{normalizedCode}' already exists.");
        }

        product.Code = normalizedCode;
        product.Description = normalizedDescription;
        product.Stock = request.Stock;

        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(product);
    }

    public async Task<IReadOnlyList<ProductSnapshotResponse>> LookupAsync(
        LookupProductsRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ProductIds.Count == 0)
        {
            throw new ValidationException("At least one product must be informed.");
        }

        var productIds = request.ProductIds.Distinct().ToList();
        var products = await dbContext.Products
            .AsNoTracking()
            .Where(product => productIds.Contains(product.Id))
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
        {
            var foundIds = products.Select(product => product.Id).ToHashSet();
            var missingIds = productIds.Where(id => !foundIds.Contains(id));
            var missingMessage = string.Join(", ", missingIds);
            throw new NotFoundException($"The following products were not found: {missingMessage}.");
        }

        var productsById = products.ToDictionary(product => product.Id);

        return productIds
            .Select(id => productsById[id])
            .Select(MapToSnapshot)
            .ToList();
    }

    public async Task ConsumeStockAsync(ConsumeStockRequest request, CancellationToken cancellationToken)
    {
        if (failureSimulationState.IsEnabled)
        {
            throw new ServiceUnavailableException(
                "Inventory failure simulation is enabled. Disable it and try again.");
        }

        if (request.Items.Count == 0)
        {
            throw new ValidationException("At least one item must be informed to consume stock.");
        }

        var groupedItems = request.Items
            .GroupBy(item => item.ProductId)
            .Select(group => new ConsumeStockItemRequest(group.Key, group.Sum(item => item.Quantity)))
            .ToList();

        if (groupedItems.Any(item => item.Quantity <= 0))
        {
            throw new ValidationException("All item quantities must be greater than zero.");
        }

        var productIds = groupedItems.Select(item => item.ProductId).ToList();

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var products = await dbContext.Products
            .Where(product => productIds.Contains(product.Id))
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
        {
            var foundIds = products.Select(product => product.Id).ToHashSet();
            var missingIds = productIds.Where(id => !foundIds.Contains(id));
            var missingMessage = string.Join(", ", missingIds);
            throw new NotFoundException($"The following products were not found: {missingMessage}.");
        }

        var productsById = products.ToDictionary(product => product.Id);
        var insufficientStockErrors = new List<string>();

        foreach (var item in groupedItems)
        {
            var product = productsById[item.ProductId];

            if (product.Stock < item.Quantity)
            {
                insufficientStockErrors.Add(
                    $"{product.Code} (available: {product.Stock}, requested: {item.Quantity})");
            }
        }

        if (insufficientStockErrors.Count > 0)
        {
            throw new ConflictException(
                $"Insufficient stock for: {string.Join("; ", insufficientStockErrors)}.");
        }

        foreach (var item in groupedItems)
        {
            productsById[item.ProductId].Stock -= item.Quantity;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Invoice {InvoiceNumber} consumed stock for {ItemCount} item(s).",
            request.InvoiceNumber,
            groupedItems.Count);
    }

    private static void ValidateProductPayload(string code, string description, int stock)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ValidationException("The product code is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ValidationException("The product description is required.");
        }

        if (stock < 0)
        {
            throw new ValidationException("The product stock cannot be negative.");
        }
    }

    private static ProductResponse MapToResponse(Product product) =>
        new(
            product.Id,
            product.Code,
            product.Description,
            product.Stock,
            product.CreatedAtUtc);

    private static ProductSnapshotResponse MapToSnapshot(Product product) =>
        new(
            product.Id,
            product.Code,
            product.Description,
            product.Stock);
}
