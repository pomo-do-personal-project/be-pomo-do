namespace be_pomo_do.Application.Common;

public static class SystemBroadcastState
{
    private static readonly object _lock = new();

    public static string Message { get; private set; } = "";
    public static string Type { get; private set; } = "info";
    public static bool IsActive { get; private set; } = false;
    public static DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public static void Update(string message, string type, bool isActive)
    {
        lock (_lock)
        {
            Message = message;
            Type = type;
            IsActive = isActive;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
