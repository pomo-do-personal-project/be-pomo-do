using be_pomo_do.Application;
using be_pomo_do.Infrastructure;
using be_pomo_do.Presentation;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Composition Root: Đăng ký dịch vụ theo từng tầng bằng Extension Methods
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation(builder.Configuration);

var app = builder.Build();

// 2. HTTP Request Pipeline
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Pomo-do API Reference")
            .WithTheme(Scalar.AspNetCore.ScalarTheme.Moon)
            .WithDefaultHttpClient(Scalar.AspNetCore.ScalarTarget.JavaScript, Scalar.AspNetCore.ScalarClient.Fetch);
    });

    // Mở http://localhost:5267/ sẽ tự động chuyển hướng vào giao diện API
    app.MapGet("/", () => Results.Redirect("/scalar/v1"));
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// 3. Map toàn bộ Controllers trong hệ thống
app.MapControllers();

app.Run();
