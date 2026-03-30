namespace backend.Models;

public class ApiResponse<T>
{
    public bool Success { get; init; }

    public T? Data { get; init; }

    public string? Message { get; init; }

    public int? StatusCode { get; init; }

    public static ApiResponse<T> FromData(T data) => new()
    {
        Success = true,
        Data = data
    };
}
