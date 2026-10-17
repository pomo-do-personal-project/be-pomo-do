namespace be_pomo_do.Presentation.DTOs;

// Encapsulates standard API response envelopes across all endpoints.
// The type of payload data returned.
public class ApiResponse<T>
{
    // Gets or sets a value that indicates whether the requested operation succeeded.
    public bool Success { get; set; }

    // Gets or sets the response data payload.
    public T? Data { get; set; }

    // Gets or sets the descriptive message regarding operation execution.
    public string Message { get; set; } = string.Empty;

    // Gets or sets the collection of error messages when the operation fails.
    public List<string>? Errors { get; set; }

    // Creates a successful API response envelope.
    public static ApiResponse<T> Ok(T data, string message = "Thao tác thành công") =>
        new() { Success = true, Data = data, Message = message };

    // Creates a failed API response envelope.
    public static ApiResponse<T> Fail(string message, List<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}

