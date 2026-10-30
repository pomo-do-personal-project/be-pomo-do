namespace be_pomo_do.Application.Common.Interfaces;

/// <summary>
/// Giao diện dịch vụ lưu trữ hình ảnh và tài nguyên trên Amazon S3.
/// </summary>
public interface IS3StorageService
{
    /// <summary>
    /// Tải luồng file lên S3 bucket trong thư mục wallpapers/.
    /// </summary>
    /// <param name="stream">Luồng dữ liệu file.</param>
    /// <param name="originalFileName">Tên file gốc để lấy định dạng và đặt tên sạch.</param>
    /// <param name="contentType">MIME type của file (ví dụ image/jpeg, image/png).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Public URL có thể truy cập trực tiếp từ trình duyệt.</returns>
    Task<string> UploadFileAsync(Stream stream, string originalFileName, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tải ảnh từ URL từ xa và lưu trực tiếp vào S3 bucket.
    /// </summary>
    /// <param name="remoteUrl">Đường dẫn ảnh từ xa (ví dụ AI URL hoặc ảnh từ internet).</param>
    /// <param name="originalFileName">Tên định danh mong muốn của ảnh.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Public URL S3 của ảnh sau khi lưu trữ.</returns>
    Task<string> UploadFromUrlAsync(string remoteUrl, string originalFileName, CancellationToken cancellationToken = default);
}
