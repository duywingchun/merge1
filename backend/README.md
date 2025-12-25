# MyGarden Backend Server (C#)

Backend server đơn giản cho game MyGarden multiplayer sử dụng C# ASP.NET Core.

## Yêu cầu

- .NET 8.0 SDK
- MySQL Server
- Visual Studio 2022 hoặc VS Code

## Cài đặt

### 1. Tạo database MySQL

Chạy file `database.sql` trong MySQL để tạo database và các bảng:

```bash
mysql -u root -p < database.sql
```

### 2. Cấu hình connection string

Mở file `DatabaseHelper.cs` và sửa connection string nếu cần:

```csharp
private static string connectionString = "Server=localhost;Database=mygarden_db;User=root;Password=;";
```

### 3. Cài đặt packages

```bash
cd backend
dotnet restore
```

### 4. Chạy server

```bash
dotnet run
```

Server sẽ chạy tại: `http://localhost:5000`

## API Endpoints

### Authentication
- `POST /api/auth/register` - Đăng ký
- `POST /api/auth/login` - Đăng nhập

### Farm
- `GET /api/farm/{userId}` - Lấy thông tin farm

## SignalR Hub

Server có SignalR hub tại: `http://localhost:5000/gamehub`

## Lưu ý

- Password được hash bằng BCrypt
- Mỗi user tự động có 1 farm khi đăng ký

