using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Core.Entities;
using be_pomo_do.Core.ValueObjects;
using be_pomo_do.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace be_pomo_do.Infrastructure.BackgroundServices;

/// <summary>
/// Khởi tạo và đồng bộ tài khoản Quản trị viên (Admin) tự động từ cấu hình appsettings.json.
/// Đảm bảo hệ thống luôn có tài khoản quản trị khi khởi chạy trên bất kỳ môi trường nào (AWS, Docker, Local).
/// </summary>
public class AdminAccountSeedWorker : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminAccountSeedWorker> _logger;

    public AdminAccountSeedWorker(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<AdminAccountSeedWorker> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var section = _configuration.GetSection("AdminAccount");
        var email = section["Email"]?.Trim().ToLowerInvariant();
        var password = section["Password"];
        var displayName = section["DisplayName"]?.Trim() ?? "System Administrator";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogInformation("ℹ️ [AdminSeed] Không tìm thấy cấu hình 'AdminAccount' trong appsettings. Bỏ qua bước seed admin.");
            return;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            var existingUser = await dbContext.Users
                .Include(u => u.Settings)
                .Include(u => u.Streak)
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            var passwordHash = passwordHasher.HashPassword(password);

            if (existingUser == null)
            {
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    DisplayName = displayName,
                    PasswordHash = passwordHash,
                    Provider = UserProvider.Local,
                    IsAdmin = true,
                    CreatedAt = DateTime.UtcNow,
                    Settings = new UserSettings
                    {
                        FocusDuration = 25,
                        ShortBreakDuration = 5,
                        LongBreakDuration = 15,
                        AutoStartBreaks = false,
                        ThemeMode = "dark"
                    },
                    Streak = new DailyStreak { CurrentStreak = 0, LongestStreak = 0 }
                };

                dbContext.Users.Add(adminUser);
                await dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("🚀 [AdminSeed] Đã tự động khởi tạo tài khoản Quản trị viên '{Email}' thành công.", email);
            }
            else
            {
                existingUser.PasswordHash = passwordHash;
                existingUser.IsAdmin = true;
                existingUser.IsSuspended = false;
                if (!string.IsNullOrWhiteSpace(displayName))
                {
                    existingUser.DisplayName = displayName;
                }

                await dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("🔄 [AdminSeed] Đã đồng bộ quyền Quản trị viên và mật khẩu cho '{Email}'.", email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [AdminSeed] Xảy ra lỗi khi khởi tạo tài khoản quản trị viên '{Email}'.", email);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
