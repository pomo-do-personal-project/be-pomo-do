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
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
        );

        // 2. Cấu hình JWT Options
        services.Configure<Authentication.JwtOptions>(configuration.GetSection(Authentication.JwtOptions.SectionName));

        // 3. Đăng ký Security services (PasswordHasher, JwtTokenGenerator)
        services.AddSingleton<Application.Common.Interfaces.IPasswordHasher, Authentication.PasswordHasher>();
        services.AddSingleton<Application.Common.Interfaces.IJwtTokenGenerator, Authentication.JwtTokenGenerator>();

        // 4. Đăng ký HttpContextAccessor & CancellationTokenProvider
        services.AddHttpContextAccessor();
        services.AddScoped<Application.Common.Interfaces.ICancellationTokenProvider, Services.HttpContextCancellationTokenProvider>();

        // 4. Scrutor: Tự động quét và đăng ký tất cả các class có đuôi Repository (map với I...Repository tương ứng)
        services.Scan(scan => scan
            .FromAssemblyOf<Program>()
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")))
            .AsMatchingInterface()
            .WithScopedLifetime());

        return services;
    }
}
