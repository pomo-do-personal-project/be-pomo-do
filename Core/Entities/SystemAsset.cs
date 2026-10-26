namespace be_pomo_do.Core.Entities;

// Đại diện cho tài nguyên hệ thống (âm thanh chuông báo, âm thanh môi trường, hình nền)
public class SystemAsset
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Loại tài nguyên: "audio_alarm" | "audio_ambient" | "wallpaper"
    public string AssetType { get; set; } = string.Empty;

    // Tên hiển thị của tài nguyên
    public string Name { get; set; } = string.Empty;

    // Đường link trực tiếp đến file âm thanh hoặc hình ảnh
    public string Url { get; set; } = string.Empty;

    // Đường link ảnh thu nhỏ xem trước (áp dụng cho hình nền hoặc ảnh bìa âm thanh)
    public string? ThumbnailUrl { get; set; }

    // Thể loại tài nguyên (ví dụ: "#Lofi", "#Nature", "#Minimal", "#Zen", "#Cyberpunk")
    public string? Category { get; set; }

    // Trạng thái khả dụng (Admin có thể bật/tắt hiển thị)
    public bool IsActive { get; set; } = true;

    // Đánh dấu tài nguyên mặc định cho người dùng mới
    public bool IsDefault { get; set; } = false;

    // Thời gian tạo
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
