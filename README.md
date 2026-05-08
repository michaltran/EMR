# EMR Liên Chiểu

Hệ thống Bệnh án điện tử & Ký số cho Trung tâm Y tế Khu vực Liên Chiểu (Đà Nẵng).

Chạy song song eHospital (HIS cũ) — không sửa HIS, chỉ đọc dữ liệu BN/lần khám và quản lý quy trình ký số bệnh án độc lập.

## Quick start

```powershell
# 1. Cài SQL Server, .NET 8 SDK (xem SETUP_GUIDE.md)
# 2. Apply migrations
dotnet ef database update -c EmrDbContext     -p src\EMR.Infrastructure -s src\EMR.Api
dotnet ef database update -c HisDemoDbContext -p src\EMR.Infrastructure -s src\EMR.Api

# 3. Run API
dotnet run --project src\EMR.Api
# → http://localhost:5099/swagger
```

## Tài liệu

- [PROJECT_BRIEF.md](PROJECT_BRIEF.md) — Bối cảnh, scope, kiến trúc, roadmap, trạng thái
- [SETUP_GUIDE.md](SETUP_GUIDE.md) — Cài môi trường dev
- [HANDOVER.md](HANDOVER.md) — Đọc đầu tiên khi mở Claude Code
- [docs/db-schema.md](docs/db-schema.md) — Thiết kế database
- [docs/smartca-api.md](docs/smartca-api.md) — Tóm tắt VNPT SmartCA API
- [docs/VNPT_SmartCA_v4.1.pdf](docs/VNPT_SmartCA_v4.1.pdf) — Tài liệu gốc của VNPT

## Stack

- ASP.NET Core 8 Web API + Entity Framework Core 8
- SQL Server 2022/2025
- Blazor Server (Web Admin)
- .NET MAUI (Mobile, sẽ thêm sau)
- VNPT SmartCA (production), self-signed cert (MVP)

## Tài khoản test (sau khi seed)

| Username | Password | Vai trò |
|---|---|---|
| `admin` | `admin@123` | ADMIN |
| `bs.an` | `bs@123` | BACSI (Khoa Nội) |
| `tk.binh` | `tk@123` | BACSI + TRUONGKHOA |
| `ld.cuong` | `ld@123` | LANHDAO_BV |
| `khth.dung` | `khth@123` | KHTH |
