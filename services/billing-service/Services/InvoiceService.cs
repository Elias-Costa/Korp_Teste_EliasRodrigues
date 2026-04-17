using BillingService.Contracts;
using BillingService.Data;
using BillingService.Exceptions;
using BillingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Services;

public sealed class InvoiceService(
    BillingDbContext dbContext,
    IInventoryApiClient inventoryApiClient)
{
    public async Task<IReadOnlyList<InvoiceResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var invoices = await dbContext.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.Items)
            .OrderByDescending(invoice => invoice.Number)
            .ToListAsync(cancellationToken);

        return invoices.Select(MapToResponse).ToList();
    }

    public async Task<InvoiceResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await dbContext.Invoices
            .AsNoTracking()
            .Include(current => current.Items)
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        if (invoice is null)
        {
            throw new NotFoundException($"Invoice '{id}' was not found.");
        }

        return MapToResponse(invoice);
    }

    public async Task<InvoiceResponse> CreateAsync(CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var normalizedItems = NormalizeItems(request);
        var products = await inventoryApiClient.GetProductsAsync(
            normalizedItems.Select(item => item.ProductId).ToList(),
            cancellationToken);

        if (products.Count != normalizedItems.Count)
        {
            throw new ValidationException("The invoice items do not match the products returned by inventory.");
        }

        var productsById = products.ToDictionary(product => product.Id);
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            Status = InvoiceStatus.Open,
            CreatedAtUtc = DateTime.UtcNow
        };

        foreach (var item in normalizedItems)
        {
            if (!productsById.TryGetValue(item.ProductId, out var product))
            {
                throw new NotFoundException($"Product '{item.ProductId}' was not found in inventory.");
            }

            invoice.Items.Add(new InvoiceItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                ProductCode = product.Code,
                ProductDescription = product.Description,
                Quantity = item.Quantity
            });
        }

        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(invoice);
    }

    public async Task<InvoiceResponse> PrintAsync(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await dbContext.Invoices
            .Include(current => current.Items)
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        if (invoice is null)
        {
            throw new NotFoundException($"Invoice '{id}' was not found.");
        }

        if (invoice.Status != InvoiceStatus.Open)
        {
            throw new ConflictException("Only invoices with status 'Open' can be printed.");
        }

        await inventoryApiClient.ConsumeStockAsync(invoice, cancellationToken);

        invoice.Status = InvoiceStatus.Closed;
        invoice.ClosedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(invoice);
    }

    private static List<CreateInvoiceItemRequest> NormalizeItems(CreateInvoiceRequest request)
    {
        if (request.Items.Count == 0)
        {
            throw new ValidationException("At least one product must be informed.");
        }

        if (request.Items.Any(item => item.ProductId == Guid.Empty))
        {
            throw new ValidationException("Each invoice item must have a valid product id.");
        }

        if (request.Items.Any(item => item.Quantity <= 0))
        {
            throw new ValidationException("All invoice item quantities must be greater than zero.");
        }

        return request.Items
            .GroupBy(item => item.ProductId)
            .Select(group => new CreateInvoiceItemRequest(group.Key, group.Sum(item => item.Quantity)))
            .ToList();
    }

    private static InvoiceResponse MapToResponse(Invoice invoice) =>
        new(
            invoice.Id,
            invoice.Number,
            invoice.Status,
            invoice.CreatedAtUtc,
            invoice.ClosedAtUtc,
            invoice.Items
                .OrderBy(item => item.ProductCode)
                .Select(item => new InvoiceItemResponse(
                    item.ProductId,
                    item.ProductCode,
                    item.ProductDescription,
                    item.Quantity))
                .ToList());
}
