namespace be_pomo_do.Presentation;

// Dependency Injection registration for Presentation layer
public static class DependencyInjection
{
    // Registers controllers, exception handling, CORS, OpenAPI, and JWT Bearer with cookie support
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Đăng ký Controller Pipeline
        services.AddControllers();

        // 2. Exception Handling & ProblemDetails (RFC 7807)
        services.AddExceptionHandler<Middlewares.GlobalExceptionHandler>();
        services.AddProblemDetails();

        // 3. OpenAPI Documentation (.NET 10 Native)
        services.AddOpenApi();

        // 3. CORS Policy an toàn cho Frontend Next.js (chặn attacker origin, chỉ cho phép localhost, vercel preview & domain cấu hình)
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "https://localhost:3000", "http://localhost:3000", "https://localhost:3001", "http://localhost:3001" };

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.SetIsOriginAllowed(origin =>
                {
                    if (string.IsNullOrWhiteSpace(origin)) return false;
                    if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                    {
                        return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                            || uri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase)
                            || allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
                    }
                    return false;
                })
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
            });
        });

        // 4. Rate Limiting (.NET 10 Native) phòng chống Brute Force và DoS
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Bộ giới hạn toàn cục theo IP (120 requests/phút)
            options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                {
                    PermitLimit = 120,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
            });

            // Chính sách riêng cho xác thực Auth (Đăng nhập, Đăng ký, OTP): tối đa 15 requests/phút/IP
            options.AddPolicy("AuthRateLimit", context =>
            {
                var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter($"auth_{clientIp}", _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                {
                    PermitLimit = 15,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
            });
        });

        // 4. JWT Authentication
        var jwtSecret = configuration["JwtSettings:Secret"];
        if (string.IsNullOrWhiteSpace(jwtSecret) || System.Text.Encoding.UTF8.GetByteCount(jwtSecret) < 32)
        {
            throw new InvalidOperationException("JwtSettings:Secret chưa được cấu hình hoặc ngắn hơn 32 bytes (256-bit).");
        }
        var jwtIssuer = configuration["JwtSettings:Issuer"] ?? "pomo-do-backend";
        var jwtAudience = configuration["JwtSettings:Audience"] ?? "pomo-do-client";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(jwtSecret)
                ),
                ClockSkew = TimeSpan.Zero
            };

            // Hỗ trợ tự động trích xuất token từ HttpOnly Cookie (BFF architecture) hoặc từ Authorization Bearer Header
            // và thực thi kiểm tra trạng thái khóa tài khoản (IsSuspended) ngay lập tức trên từng request.
            options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (context.Request.Cookies.TryGetValue("access_token", out var cookieToken) && !string.IsNullOrEmpty(cookieToken))
                    {
                        context.Token = cookieToken;
                    }
                    return Task.CompletedTask;
                },
                OnTokenValidated = async context =>
                {
                    var userIdClaim = context.Principal?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                        ?? context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                    if (Guid.TryParse(userIdClaim, out var userId))
                    {
                        var dbContext = context.HttpContext.RequestServices.GetRequiredService<be_pomo_do.Infrastructure.Data.AppDbContext>();
                        var user = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                            dbContext.Users, u => u.Id == userId, context.HttpContext.RequestAborted);

                        if (user == null || user.IsSuspended)
                        {
                            context.Fail("Tài khoản đã bị tạm khóa hoặc không còn tồn tại trên hệ thống.");
                            return;
                        }

                        // Nếu token có claim Admin nhưng trong DB đã bị gỡ quyền Admin
                        var hasAdminClaim = context.Principal?.HasClaim(c => (c.Type == System.Security.Claims.ClaimTypes.Role || c.Type == "role") && c.Value == "Admin") ?? false;
                        if (hasAdminClaim && !user.IsAdmin)
                        {
                            context.Fail("Quyền quản trị viên của tài khoản này đã bị thu hồi.");
                        }
                    }
                }
            };
        });

        services.AddAuthorization();

        return services;
    }
}
