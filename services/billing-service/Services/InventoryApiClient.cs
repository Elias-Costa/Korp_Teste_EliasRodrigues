using System.Net;
using System.Net.Http.Json;
using BillingService.Contracts;
using BillingService.Exceptions;
using BillingService.Models;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Services;

public sealed class InventoryApiClient(
    HttpClient httpClient,
    ILogger<InventoryApiClient> logger) : IInventoryApiClient
{
    public async Task<IReadOnlyList<ProductSnapshotResponse>> GetProductsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync(
            "api/products/lookup",
            new LookupProductsRequest(productIds.ToList()),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiExceptionAsync(response, cancellationToken);
        }

        var products = await response.Content.ReadFromJsonAsync<List<ProductSnapshotResponse>>(
            cancellationToken: cancellationToken);

        return products ?? [];
    }

    public async Task ConsumeStockAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        var request = new ConsumeStockRequest(
            invoice.Id,
            invoice.Number,
            invoice.Items
                .Select(item => new ConsumeStockItemRequest(item.ProductId, item.Quantity))
                .ToList());

        var response = await httpClient.PostAsJsonAsync(
            "api/products/consume",
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    private async Task ThrowApiExceptionAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var problem = await ReadProblemDetailsAsync(response, cancellationToken);
        var message = problem?.Detail ?? $"Inventory service returned status code {(int)response.StatusCode}.";

        logger.LogWarning(
            "Inventory service returned status code {StatusCode}: {Message}",
            (int)response.StatusCode,
            message);

        throw response.StatusCode switch
        {
            HttpStatusCode.BadRequest => new ValidationException(message),
            HttpStatusCode.NotFound => new NotFoundException(message),
            HttpStatusCode.Conflict => new ConflictException(message),
            _ => new ExternalServiceException(message)
        };
    }

    private static async Task<ProblemDetails?> ReadProblemDetailsAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: cancellationToken);
        }
        catch
        {
            return null;
        }
    }
}
