namespace MatchIQ.API.Common;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
}

public static class ApiResponse
{
    public static ApiResponse<T> Ok<T>(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<object?> Ok(string? message = null) =>
        new() { Success = true, Message = message };

    public static ApiResponse<object?> Fail(string message) =>
        new() { Success = false, Message = message };

    public static ApiResponse<T> Fail<T>(string message, T data) =>
        new() { Success = false, Message = message, Data = data };
}
