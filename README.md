# Pomo-Do Backend API

RESTful API backend cho ứng dụng **Pomo-Do** (kết hợp phương pháp Pomodoro và Quản lý công việc To-Do), được xây dựng trên nền tảng **ASP.NET Core (.NET 10)**.

---

## 🚀 Tính năng cốt lõi (Core Features)

- **Authentication & User Management**: Xác thực JWT, đăng ký, đăng nhập, quản lý thông tin người dùng.
- **Task Management**: CRUD danh sách công việc, phân loại thẻ (tag), ưu tiên (priority), trạng thái hoàn thành.
- **Pomodoro Session**: Theo dõi phiên làm việc, thời gian tập trung, thời gian nghỉ ngơi và liên kết với Task.
- **Analytics & Statistics**: Báo cáo tổng kết thời gian tập trung theo ngày, tuần, tháng và hiệu suất hoàn thành task.
- **Settings**: Cấu hình thời gian Pomodoro mặc định (25/5/15 mins), âm thanh thông báo.

---

## 🛠 Công nghệ sử dụng (Tech Stack)

- **Platform**: [.NET 10 Web API](https://dotnet.microsoft.com/)
- **Architecture**: Clean Architecture / Layered Architecture
- **API Documentation**: Microsoft OpenAPI (`Microsoft.AspNetCore.OpenApi`) / Swagger
- **Database**: PostgreSQL / SQL Server (hỗ trợ Entity Framework Core)
- **Authentication**: JWT (JSON Web Token)

---
## ⚡ Hướng dẫn cài đặt & Khởi chạy (Getting Started)

### Yêu cầu tiên quyết:
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Chạy dự án:
```powershell
# Di chuyển vào thư mục backend
cd be-pomo-do

# Khôi phục dependencies
dotnet restore

# Chạy ứng dụng ở môi trường Development
dotnet run
---

## 🔗 Liên kết liên quan

- Frontend Repository: `pomo-do` (Next.js Application)
