using System.Net;

namespace InventoryService.Exceptions;

public class ApiException(string title, string message, HttpStatusCode statusCode) : Exception(message)
{
    public string Title { get; } = title;
    public int StatusCode { get; } = (int)statusCode;
}

public sealed class ValidationException(string message)
    : ApiException("Validation error", message, HttpStatusCode.BadRequest);

public sealed class NotFoundException(string message)
    : ApiException("Resource not found", message, HttpStatusCode.NotFound);

public sealed class ConflictException(string message)
    : ApiException("Conflict", message, HttpStatusCode.Conflict);

public sealed class ServiceUnavailableException(string message)
    : ApiException("Service unavailable", message, HttpStatusCode.ServiceUnavailable);
