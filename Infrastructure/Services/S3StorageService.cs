using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using be_pomo_do.Core.Exceptions;
using be_pomo_do.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace be_pomo_do.Infrastructure.Services;

/// <summary>
/// Dịch vụ tải và lưu trữ tài nguyên hình ảnh lên AWS S3 tối ưu chi phí.
/// Tự động phân giải quyền IAM (ECS Task Role hoặc Local AWS CLI Credentials).
/// </summary>
public class S3StorageService : IS3StorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _assetsBaseUrl;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<S3StorageService> _logger;

    public S3StorageService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<S3StorageService> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var awsSection = configuration.GetSection("AWS");
        var regionName = awsSection["Region"] ?? "ap-southeast-1";
        var region = RegionEndpoint.GetBySystemName(regionName);

        _bucketName = awsSection["BucketName"] ?? "pomodo-assets-023433587790";

        var explicitBaseUrl = awsSection["AssetsBaseUrl"]?.TrimEnd('/');
        _assetsBaseUrl = !string.IsNullOrWhiteSpace(explicitBaseUrl)
            ? explicitBaseUrl
            : $"https://{_bucketName}.s3.{regionName}.amazonaws.com";

        var accessKey = awsSection["AccessKey"];
        var secretKey = awsSection["SecretKey"];

        if (!string.IsNullOrWhiteSpace(accessKey) && !string.IsNullOrWhiteSpace(secretKey))
        {
            var credentials = new BasicAWSCredentials(accessKey.Trim(), secretKey.Trim());
            _s3Client = new AmazonS3Client(credentials, region);
            _logger.LogInformation("AWS S3 Client khởi tạo thành công với cấu hình tĩnh (AccessKey/SecretKey).");
        }
        else
        {
            // Tự động phân giải qua DefaultAWSCredentialsChain (ECS Task IAM Role trên AWS hoặc ~/.aws/credentials ở Local)
            _s3Client = new AmazonS3Client(region);
            _logger.LogInformation("AWS S3 Client khởi tạo thành công qua Default Credentials Chain (ECS Task Role/Environment).");
        }
    }

    public async Task<string> UploadFileAsync(
        Stream stream,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (stream == null || stream.Length == 0)
        {
            throw new DomainException("Dữ liệu tệp hình ảnh tải lên không hợp lệ hoặc rỗng.");
        }

        // 1. Kiểm tra giới hạn dung lượng: Tối đa 5MB để bảo vệ S3 không bị quá tải
        const long maxFileSize = 5 * 1024 * 1024;
        if (stream.Length > maxFileSize)
        {
            throw new DomainException("Dung lượng hình ảnh vượt quá giới hạn tối đa cho phép (5MB).");
        }

        // 2. Chuẩn hóa phần mở rộng và MIME type
        var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(ext))
        {
            ext = contentType switch
            {
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => ".jpg"
            };
        }

        var allowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowedExtensions.Contains(ext))
        {
            throw new DomainException("Định dạng tệp không được hỗ trợ. Vui lòng chỉ tải ảnh định dạng PNG, JPG, JPEG hoặc WebP.");
        }

        var rawName = Path.GetFileNameWithoutExtension(originalFileName);
        var cleanName = Regex.Replace(rawName.ToLowerInvariant(), @"[^a-z0-9]", "_");
        if (cleanName.Length > 30)
        {
            cleanName = cleanName[..30];
        }
        if (string.IsNullOrWhiteSpace(cleanName))
        {
            cleanName = "asset";
        }

        var key = $"wallpapers/{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{cleanName}{ext}";

        var resolvedContentType = string.IsNullOrWhiteSpace(contentType) || contentType == "application/octet-stream"
            ? (ext == ".png" ? "image/png" : ext == ".webp" ? "image/webp" : "image/jpeg")
            : contentType;

        try
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = stream,
                ContentType = resolvedContentType
            };

            await _s3Client.PutObjectAsync(request, cancellationToken);

            var publicUrl = $"{_assetsBaseUrl}/{key}";
            _logger.LogInformation("Tải ảnh lên S3 thành công: {Url}", publicUrl);
            return publicUrl;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Lỗi từ AWS S3 khi tải ảnh: {Message} (StatusCode: {StatusCode}, ErrorCode: {ErrorCode})",
                ex.Message, ex.StatusCode, ex.ErrorCode);
            throw new DomainException($"Không thể lưu trữ hình ảnh lên AWS S3: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi không xác định khi tải ảnh lên S3.");
            throw new DomainException($"Lỗi xử lý hình ảnh: {ex.Message}");
        }
    }

    public async Task<string> UploadFromUrlAsync(
        string remoteUrl,
        string originalFileName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(remoteUrl))
        {
            throw new DomainException("Đường dẫn URL ảnh không được để trống.");
        }

        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(20);

        try
        {
            using var response = await client.GetAsync(remoteUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new DomainException($"Không thể tải ảnh từ URL cung cấp (Mã phản hồi: {(int)response.StatusCode}).");
            }

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var memoryStream = new MemoryStream();
            await contentStream.CopyToAsync(memoryStream, cancellationToken);

            return await UploadFileAsync(memoryStream, originalFileName, contentType, cancellationToken);
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi nạp ảnh từ URL: {Url}", remoteUrl);
            throw new DomainException($"Không thể nạp dữ liệu ảnh từ URL được cung cấp: {ex.Message}");
        }
    }
}
