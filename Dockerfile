# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy file .csproj và restore dependencies (tối ưu cache Docker)
COPY ["be-pomo-do.csproj", "./"]
RUN dotnet restore "be-pomo-do.csproj"

# Copy toàn bộ mã nguồn và build release
COPY . .
RUN dotnet publish "be-pomo-do.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Cấu hình cổng cho Render (ASP.NET Core 8/9/10 mặc định dùng 8080)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Chạy với tài khoản không đặc quyền (non-root) để tăng cường bảo mật container
USER $APP_UID

ENTRYPOINT ["dotnet", "be-pomo-do.dll"]
