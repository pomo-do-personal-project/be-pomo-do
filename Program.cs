using be_pomo_do.Application;
using be_pomo_do.Infrastructure;
using be_pomo_do.Presentation;

var builder = WebApplication.CreateBuilder(args);

// 1. Composition Root: Đăng ký dịch vụ theo từng tầng bằng Extension Methods
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation(builder.Configuration);

var app = builder.Build();

// 2. HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();

// 3. Map toàn bộ Controllers trong hệ thống
app.MapControllers();

app.Run();
