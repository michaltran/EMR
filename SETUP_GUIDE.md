# Setup Guide — Máy Dev/Server Test EMR Liên Chiểu

> Hướng dẫn cài đặt môi trường cho máy mới (Windows 11, 16GB RAM, 512GB SSD).
>
> Tổng thời gian: ~45-60 phút (tùy tốc độ mạng).

---

## A. Cài đặt theo thứ tự

### A.1. Claude Code (nếu chưa có trên máy mới)
- Tải: https://claude.com/claude-code
- Sau khi cài, đăng nhập tài khoản Anthropic
- Mở folder `D:\EMR_LienChieu\` để tiếp tục dự án

### A.2. .NET 8 SDK
- Tải: https://dotnet.microsoft.com/download/dotnet/8.0
- Chọn **SDK x64** cho Windows
- Sau khi cài, mở PowerShell mới và kiểm tra:
  ```powershell
  dotnet --version
  ```
  → Phải ra `8.0.xxx`

### A.3. SQL Server 2022 Developer Edition (FREE, full chức năng)
- Tải: https://www.microsoft.com/sql-server/sql-server-downloads
- Chọn **Developer** (không phải Express vì Express giới hạn 10GB)
- Khi cài, chọn:
  - Authentication: **Mixed Mode** (đặt password cho `sa` — ghi nhớ password này!)
  - Instance: để mặc định `MSSQLSERVER`
- Mở firewall cổng 1433 (nếu sau này cần truy cập từ máy khác):
  ```powershell
  New-NetFirewallRule -DisplayName "SQL Server" -Direction Inbound -Protocol TCP -LocalPort 1433 -Action Allow
  ```

### A.4. SQL Server Management Studio (SSMS)
- Tải: https://aka.ms/ssmsfullsetup
- Cài mặc định
- Mở SSMS, kết nối `localhost` với Windows Auth → kiểm tra SQL chạy được

### A.5. sqlcmd (thường có sẵn sau khi cài SQL, nếu không thì):
- Tải: https://learn.microsoft.com/sql/tools/sqlcmd/sqlcmd-utility
- Kiểm tra:
  ```powershell
  sqlcmd -S localhost -Q "SELECT @@VERSION" -C
  ```

### A.6. Visual Studio 2022 Community (FREE)
- Tải: https://visualstudio.microsoft.com/vs/community/
- Khi cài, chọn workloads:
  - ☑ **ASP.NET and web development**
  - ☑ **.NET Multi-platform App UI development** (cho MAUI)
  - ☑ **Data storage and processing** (cho SQL tools)
- Optional: **Mobile development with .NET** (nếu muốn build APK Android)

### A.7. Git for Windows
- Tải: https://git-scm.com/download/win
- Cài mặc định
- Sau cài, set tên/email:
  ```powershell
  git config --global user.name "Your Name"
  git config --global user.email "you@example.com"
  ```

### A.8. (Optional) VS Code + C# Dev Kit
Nếu thích editor nhẹ hơn VS:
- VS Code: https://code.visualstudio.com/
- Extension: C# Dev Kit (Microsoft)

---

## B. Sau khi cài xong — gửi kết quả 2 lệnh này cho Claude

Mở **PowerShell** mới chạy:

```powershell
dotnet --version
sqlcmd -S localhost -Q "SELECT @@VERSION" -C
```

Copy output gửi cho Claude. Nếu cả 2 đều ra kết quả → môi trường OK, Claude sẽ tạo solution.

---

## C. Khi Claude bắt đầu code, tự động sẽ làm

1. Tạo solution `D:\EMR_LienChieu\EMR.sln` với các project (xem `PROJECT_BRIEF.md` mục 3.3)
2. Viết EF Core migrations cho 2 DB:
   - `EMR_LienChieu` (DB mới của hệ thống)
   - `eHospital_Demo` (DB giả lập HIS với data mẫu)
3. Code vertical slice #1 (upload → ký → lưu → mobile xem)

---

## D. Troubleshooting thường gặp

| Vấn đề | Cách xử lý |
|---|---|
| `sqlcmd` lỗi SSL | Thêm `-C` (Trust Server Certificate) như lệnh trên |
| SQL không kết nối được | Bật service `SQL Server (MSSQLSERVER)` trong `services.msc` |
| `dotnet` không nhận | Restart PowerShell, hoặc restart máy sau khi cài SDK |
| Visual Studio cài chậm | Bỏ workload không cần, chỉ giữ 2 workload bắt buộc ở A.6 |
| Cổng 1433 bị block | Chạy lệnh New-NetFirewallRule ở A.3 |

---

## E. Cấu hình máy server test khuyến nghị

Máy hiện tại (Win 11, 16GB RAM, 512GB SSD) **đủ chạy MVP**. Lưu ý:

- SQL Server ăn ~2-4GB RAM khi chạy
- Visual Studio ăn ~2-3GB
- Để dành ~8GB cho dev + emulator Android (nếu test mobile)
- SSD 512GB: dành ~50GB cho SQL data, ~30GB cho code + tools

Nếu sau này lên production thật cần server riêng (16-32GB RAM, RAID SSD, backup).
