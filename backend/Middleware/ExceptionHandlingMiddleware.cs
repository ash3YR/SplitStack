using System.Text.Json;
using backend.Exceptions;
using backend.Models;

namespace backend.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Request failed for {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteErrorResponseAsync(context, exception);
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = exception switch
        {
            ApiException apiException => apiException.StatusCode,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.StatusCode = statusCode;

        var payload = ApiErrorResponse.Create(
            statusCode == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exception.Message,
            statusCode);

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
