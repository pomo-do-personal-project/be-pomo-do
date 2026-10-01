# Pomo-Do Backend API

RESTful API backend cho ứng dụng **Pomo-Do** (kết hợp phương pháp Pomodoro và Quản lý công việc To-Do), được xây dựng trên nền tảng **ASP.NET Core (.NET 10)** và cơ sở dữ liệu **Supabase PostgreSQL**.

---

## 🚀 Tính năng cốt lõi (Core Features)

- **Authentication & User Management**: Xác thực JWT, đăng ký, đăng nhập (Local, Google, Github OAuth).
- **Task & Subtask Management**: Quản lý công việc và danh sách việc phụ (subtasks), ma trận ưu tiên Eisenhower, nhãn phân loại (tags).
- **Focus Sessions**: Đếm giờ tập trung (Pomodoro, Stopwatch), ghi nhận thời gian làm lố (overtime).
- **Gamification & Settings**: Theo dõi chuỗi ngày hoạt động liên tục (daily streaks) và cấu hình cá nhân hóa (user settings).

---

## 🛠 Công nghệ sử dụng (Tech Stack)

- **Platform**: [.NET 10 Web API](https://dotnet.microsoft.com/)
- **Architecture**: Clean Architecture / Domain-Driven Design (DDD)
- **Database**: PostgreSQL on [Supabase](https://supabase.com/)
- **ORM**: Entity Framework Core 10 (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- **API Documentation**: Microsoft OpenAPI (`Microsoft.AspNetCore.OpenApi`)

---

## ⚡ Hướng dẫn cài đặt & Khởi chạy (Getting Started)

### Yêu cầu tiên quyết:
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Chạy dự án:
```powershell
# 1. Di chuyển vào thư mục backend
cd be-pomo-do

# 2. Khôi phục dependencies
dotnet restore

# 3. Tạo migration và cập nhật schema lên Supabase (khi cần)
dotnet ef migrations add InitialCreate --output-dir Infrastructure/Data/Migrations
dotnet ef database update

# 4. Chạy ứng dụng ở môi trường Development
dotnet run
```

Ứng dụng sẽ khởi chạy tại:
- HTTP: `http://localhost:5267`
- OpenAPI Spec: `http://localhost:5267/openapi/v1.json`

---

## 🔗 Liên kết liên quan

- Frontend Repository: `pomo-do` (Next.js Application)
