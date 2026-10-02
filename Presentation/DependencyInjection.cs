namespace be_pomo_do.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Đăng ký Controller Pipeline
        services.AddControllers();

        // 2. OpenAPI / Swagger Documentation
        services.AddOpenApi();

        // 3. CORS Policy cho Frontend Next.js
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:3000")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }
}
