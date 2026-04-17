using InventoryService.Contracts;
using InventoryService.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(ProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var products = await productService.GetAllAsync(cancellationToken);
        return Ok(products);
    }

    [HttpGet("{id:guid}", Name = "GetProductById")]
    public async Task<ActionResult<ProductResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> CreateAsync(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var createdProduct = await productService.CreateAsync(request, cancellationToken);

        return CreatedAtRoute("GetProductById", new { id = createdProduct.Id }, createdProduct);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductResponse>> UpdateAsync(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var updatedProduct = await productService.UpdateAsync(id, request, cancellationToken);
        return Ok(updatedProduct);
    }

    [HttpPost("lookup")]
    public async Task<ActionResult<IReadOnlyList<ProductSnapshotResponse>>> LookupAsync(
        [FromBody] LookupProductsRequest request,
        CancellationToken cancellationToken)
    {
        var products = await productService.LookupAsync(request, cancellationToken);
        return Ok(products);
    }

    [HttpPost("consume")]
    public async Task<IActionResult> ConsumeAsync(
        [FromBody] ConsumeStockRequest request,
        CancellationToken cancellationToken)
    {
        await productService.ConsumeStockAsync(request, cancellationToken);
        return NoContent();
    }
}
