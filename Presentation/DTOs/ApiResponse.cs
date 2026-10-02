namespace be_pomo_do.Presentation.DTOs;

/// <summary>Encapsulates standard API response envelopes across all endpoints.</summary>
/// <typeparam name="T">The type of payload data returned.</typeparam>
public class ApiResponse<T>
{
    /// <summary>Gets or sets a value that indicates whether the requested operation succeeded.</summary>
    public bool Success { get; set; }

    /// <summary>Gets or sets the response data payload.</summary>
    public T? Data { get; set; }

    /// <summary>Gets or sets the descriptive message regarding operation execution.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets the collection of error messages when the operation fails.</summary>
    public List<string>? Errors { get; set; }

    /// <summary>Creates a successful API response envelope.</summary>
    public static ApiResponse<T> Ok(T data, string message = "Thao tác thành công") =>
        new() { Success = true, Data = data, Message = message };

    /// <summary>Creates a failed API response envelope.</summary>
    public static ApiResponse<T> Fail(string message, List<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}

