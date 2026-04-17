using BillingService.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ApiException ex)
        {
            await WriteProblemDetailsAsync(context, ex.StatusCode, ex.Title, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while processing the request.");

            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Unexpected error",
                "An unexpected error occurred while processing your request.");
        }
    }

    private static Task WriteProblemDetailsAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        return context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        });
    }
}
