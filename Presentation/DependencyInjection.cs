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
        var jwtSecret = configuration["JwtSettings:Secret"] ?? "pomo_do_super_secret_jwt_key_min_32_characters_long_2026!";
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
            options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (context.Request.Cookies.TryGetValue("access_token", out var cookieToken) && !string.IsNullOrEmpty(cookieToken))
                    {
                        context.Token = cookieToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();

        return services;
    }
}
