using be_pomo_do.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace be_pomo_do.Infrastructure;

// Dependency Injection registration for Infrastructure layer
public static class DependencyInjection
{
    // Registers database context, JWT options, security services, and repositories
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Cấu hình EF Core DbContext với PostgreSQL Supabase
        var defaultConn = NormalizePostgresConnectionString(configuration.GetConnectionString("DefaultConnection"));
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(defaultConn)
        );

        // 2. Cấu hình JWT Options
        services.Configure<Authentication.JwtOptions>(configuration.GetSection(Authentication.JwtOptions.SectionName));

        // 3. Đăng ký Security services (PasswordHasher, JwtTokenGenerator)
        services.AddSingleton<Application.Common.Interfaces.IPasswordHasher, Authentication.PasswordHasher>();
        services.AddSingleton<Application.Common.Interfaces.IJwtTokenGenerator, Authentication.JwtTokenGenerator>();

        // 4. Đăng ký HttpContextAccessor & CancellationTokenProvider
        services.AddHttpContextAccessor();
        services.AddScoped<Application.Common.Interfaces.ICancellationTokenProvider, Services.HttpContextCancellationTokenProvider>();

        // 5. Cấu hình Redis (Upstash) & OTP Service
        var rawRedisConn = configuration.GetConnectionString("Redis")
            ?? configuration["ConnectionStrings:Redis"]
            ?? configuration["Redis"]
            ?? configuration["Redis:ConnectionString"];

        var redisConn = NormalizeRedisConnectionString(rawRedisConn);

        var isPlaceholder = string.IsNullOrWhiteSpace(redisConn) 
            || redisConn.Contains("ENDPOINT") 
            || redisConn.Contains("YOUR_UPSTASH");

        if (!isPlaceholder)
        {
            services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
            {
                var options = StackExchange.Redis.ConfigurationOptions.Parse(redisConn!);
                options.AbortOnConnectFail = false;
                options.ConnectTimeout = 5000;
                options.SyncTimeout = 5000;
                return StackExchange.Redis.ConnectionMultiplexer.Connect(options);
            });
            services.AddSingleton<Services.InMemoryOtpService>();
            services.AddScoped<Application.Common.Interfaces.IOtpService, Services.RedisOtpService>();
        }
        else
        {
            // Fallback lưu OTP trong bộ nhớ đệm (In-Memory) nếu chưa cấu hình Redis hoặc còn chứa chữ mẫu placeholder
            services.AddScoped<Application.Common.Interfaces.IOtpService, Services.InMemoryOtpService>();
        }

        // 6. Cấu hình SmtpSettings & Gmail Email Service
        services.Configure<Services.SmtpOptions>(configuration.GetSection(Services.SmtpOptions.SectionName));
        services.AddScoped<Application.Common.Interfaces.IEmailService, Services.GmailEmailService>();

        // 7. Cấu hình Redis Streams Email Report Queue & Background Workers
        services.AddSingleton<Application.Common.Interfaces.IEmailReportQueue, Services.RedisEmailReportQueue>();
        services.AddHostedService<BackgroundServices.EmailReportConsumerWorker>();
        services.AddHostedService<BackgroundServices.EmailReportSchedulerWorker>();

        // 8. Scrutor: Tự động quét và đăng ký tất cả các class có đuôi Repository (map với I...Repository tương ứng)
        services.Scan(scan => scan
            .FromAssemblyOf<Program>()
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")))
            .AsMatchingInterface()
            .WithScopedLifetime());

        return services;
    }

    /// <summary>
    /// Chuyển đổi định dạng cấu hình Redis nếu người dùng copy dạng CLI hoặc URL Upstash (redis://...)
    /// sang dạng chuẩn của StackExchange.Redis: "host:port,password=...,ssl=True,abortConnect=False"
    /// </summary>
    private static string NormalizeRedisConnectionString(string? rawConn)
    {
        if (string.IsNullOrWhiteSpace(rawConn)) return string.Empty;

        var trimmed = rawConn.Trim();

        // Xử lý nếu người dùng copy nguyên câu lệnh `redis-cli --tls -u redis://...`
        if (trimmed.StartsWith("redis-cli", StringComparison.OrdinalIgnoreCase))
        {
            var uIndex = trimmed.IndexOf("-u ", StringComparison.OrdinalIgnoreCase);
            if (uIndex >= 0)
            {
                trimmed = trimmed[(uIndex + 3)..].Trim().Trim('"', '\'');
            }
        }

        // Xử lý nếu chuỗi là dạng URI `redis://...` hoặc `rediss://...`
        if (trimmed.StartsWith("redis://", StringComparison.OrdinalIgnoreCase) || 
            trimmed.StartsWith("rediss://", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var uri = new Uri(trimmed);
                var host = uri.Host;
                var port = uri.Port > 0 ? uri.Port : 6379;
                var password = string.Empty;

                if (!string.IsNullOrEmpty(uri.UserInfo))
                {
                    var parts = uri.UserInfo.Split(':');
                    password = parts.Length > 1 ? parts[1] : parts[0];
                }

                return $"{host}:{port},password={password},ssl=True,abortConnect=False";
            }
            catch
            {
                // Bỏ qua nếu parse URI thất bại, trả về chuỗi gốc
            }
        }

        // Đảm bảo host có kèm cổng :6379 nếu chưa chỉ định cổng (tránh StackExchange.Redis tự nhảy sang port 6380 khi bật ssl=True)
        var firstComma = trimmed.IndexOf(',');
        var hostPart = firstComma >= 0 ? trimmed[..firstComma].Trim() : trimmed;
        if (!hostPart.Contains(':') && !string.IsNullOrEmpty(hostPart))
        {
            var rest = firstComma >= 0 ? trimmed[firstComma..] : string.Empty;
            trimmed = $"{hostPart}:6379{rest}";
        }

        return trimmed;
    }

    /// <summary>
    /// Chuyển đổi URI PostgreSQL (postgresql://...) từ Supabase sang định dạng chuẩn ADO.NET (Host=...;Port=...;...)
    /// </summary>
    private static string NormalizePostgresConnectionString(string? rawConn)
    {
        if (string.IsNullOrWhiteSpace(rawConn)) return string.Empty;

        var trimmed = rawConn.Trim();
        if (trimmed.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var uri = new Uri(trimmed);
                var userInfoParts = uri.UserInfo.Split(':', 2);
                var username = userInfoParts.Length > 0 ? Uri.UnescapeDataString(userInfoParts[0]) : "postgres";
                var password = userInfoParts.Length > 1 ? Uri.UnescapeDataString(userInfoParts[1]) : string.Empty;
                var database = uri.AbsolutePath.TrimStart('/');
                if (string.IsNullOrEmpty(database)) database = "postgres";
                var port = uri.Port > 0 ? uri.Port : 5432;

                return $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};Pooling=true;SSL Mode=Require;Trust Server Certificate=true";
            }
            catch
            {
                // Bỏ qua nếu parse URI thất bại, trả về chuỗi gốc
            }
        }

        return trimmed;
    }
}
