namespace be_pomo_do.Application.Common;

// Cung cấp TimeZoneInfo chuẩn hóa, tương thích cả Windows (SE Asia Standard Time) và Linux/Docker (Asia/Ho_Chi_Minh / Asia/Bangkok / Fallback UTC+7)
public static class TimeZoneHelper
{
    private static readonly TimeZoneInfo VietnamTimeZone = ResolveVietnamTimeZone();

    private static TimeZoneInfo ResolveVietnamTimeZone()
    {
        // 1. Windows timezone ID
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch (Exception) { }

        // 2. IANA timezone ID (Linux / Alpine / Ubuntu Docker)
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }
        catch (Exception) { }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Bangkok");
        }
        catch (Exception) { }

        // 3. Fallback an toàn tuyệt đối với offset UTC+7 cố định
        return TimeZoneInfo.CreateCustomTimeZone(
            "ICT_Custom",
            TimeSpan.FromHours(7),
            "Indochina Time (+07:00)",
            "ICT"
        );
    }

    public static TimeZoneInfo GetLocalTimeZone() => VietnamTimeZone;

    public static DateTime GetLocalNow() => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);

    public static DateTime ConvertToLocal(DateTime utcDateTime) => TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, VietnamTimeZone);

    public static DateOnly GetLocalDate(DateTime utcDateTime)
    {
        var local = ConvertToLocal(utcDateTime);
        return DateOnly.FromDateTime(local);
    }
}
