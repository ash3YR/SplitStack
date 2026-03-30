namespace backend.Models;

public class ApiErrorResponse
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public int StatusCode { get; init; }

    public static ApiErrorResponse Create(string message, int statusCode) => new()
    {
        Success = false,
        Message = message,
        StatusCode = statusCode
    };
}
