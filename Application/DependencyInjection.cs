namespace be_pomo_do.Application;

// Dependency Injection registration for Application layer
public static class DependencyInjection
{
    // Registers application services via Scrutor assembly scanning
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Scrutor: Tự động quét và đăng ký tất cả các class có đuôi Service (map với I...Service tương ứng)
        services.Scan(scan => scan
            .FromAssemblyOf<Program>()
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service")))
            .AsMatchingInterface()
            .WithScopedLifetime());

        return services;
    }
}
