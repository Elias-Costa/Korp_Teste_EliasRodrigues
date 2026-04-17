using BillingService.Contracts;
using BillingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Controllers;

[ApiController]
[Route("api/invoices")]
public sealed class InvoicesController(InvoiceService invoiceService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InvoiceResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var invoices = await invoiceService.GetAllAsync(cancellationToken);
        return Ok(invoices);
    }

    [HttpGet("{id:guid}", Name = "GetInvoiceById")]
    public async Task<ActionResult<InvoiceResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await invoiceService.GetByIdAsync(id, cancellationToken);
        return Ok(invoice);
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceResponse>> CreateAsync(
        [FromBody] CreateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        var createdInvoice = await invoiceService.CreateAsync(request, cancellationToken);
        return CreatedAtRoute("GetInvoiceById", new { id = createdInvoice.Id }, createdInvoice);
    }

    [HttpPost("{id:guid}/print")]
    public async Task<ActionResult<InvoiceResponse>> PrintAsync(Guid id, CancellationToken cancellationToken)
    {
        var printedInvoice = await invoiceService.PrintAsync(id, cancellationToken);
        return Ok(printedInvoice);
    }
}
