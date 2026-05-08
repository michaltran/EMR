# EMR Liên Chiểu — Project Brief

> **Mục đích file này**: Bàn giao toàn bộ ngữ cảnh dự án để Claude Code (hoặc người khác) có thể tiếp tục từ đúng điểm đang dở khi đổi máy/mở conversation mới.
>
> **Cách dùng**: Khi mở Claude Code lần đầu trên máy mới, mở folder `C:\Users\MAI_KHNV\Desktop\EMR_LienChieu\EMR_LienChieu\` rồi nói: *"Đọc PROJECT_BRIEF.md và SETUP_GUIDE.md để tiếp tục dự án."*

---

## 1. Bối cảnh

- **Khách hàng**: Trung tâm Y tế Khu vực Liên Chiểu (Đà Nẵng)
- **Tham chiếu nghiệp vụ**: file `Danh Muc BV LC 08052026.xlsx` — danh mục đầu tư bệnh án điện tử & bệnh án không giấy của BV (~344 dòng tính năng)
- **Hệ thống HIS hiện tại**: eHospital (WinForms .NET Framework 4, dùng DevExpress 11.1 + Janus + Spire.Pdf). Bản build có ở `D:\BIN_new\` — chỉ DLL/EXE, **không có source code**.
- **Connection string HIS** nằm ở `\\server-one\eHospital$\TestSystemConfigPublic.xml` (theo `eHospital.vshost.exe.config`).

## 2. Phạm vi MVP (4 module ưu tiên)

User chỉ quan tâm 4 module trong danh mục, KHÔNG làm full danh mục:

| # | Module | Mã trong danh mục |
|---|---|---|
| 1 | Quản lý Ký số và Kho EMR | B.I.1 |
| 2 | Quản lý Kho EMR cấp Khoa & cấp Bệnh viện | B.I.2 |
| 3 | Quản lý Quy trình Ký số (Workflow) | B.I.3 |
| 4 | Cổng thông tin Bệnh nhân & App di động | B.III.1 |

Các module khác (HIS upgrade, LIS, giám định BHYT, dashboard điều hành...) **KHÔNG nằm trong scope**.

## 3. Quyết định kiến trúc đã thống nhất

### 3.1. Chiến lược: Chạy SONG SONG eHospital, KHÔNG sửa eHospital
```
[eHospital cũ - WinForms]      [Hệ thống mới EMR + Ký số]
        ↓                              ↑
    [DB SQL HIS] ───read-only─────────┤
        ↑                              ↓
[Hồ sơ HIS gốc]                  [Web Admin Kho EMR]
                                       ↓
                                 [App iOS/Android]
```

### 3.2. Stack
- **Backend**: ASP.NET Core 8 Web API + Entity Framework Core
- **DB**: SQL Server 2022 (cùng engine với HIS để dễ liên thông)
- **File storage**: Filesystem local cho MVP, sau nâng lên MinIO/Azure Blob
- **Web Admin**: Blazor Server (chưa chốt — có thể đổi React nếu user thích)
- **Mobile**: .NET MAUI (1 codebase iOS + Android, cùng C# với backend)
- **Ký số**: VNPT-CA (user đã liên hệ, đang chờ tài liệu API). Dev dùng self-signed cert trước.
- **Định dạng ký**: PAdES cho PDF, có timestamp TSA
- **Audit log**: Bảng riêng, append-only (mô phỏng WORM ở mức app), giữ ≥5 năm

### 3.3. Cấu trúc solution dự kiến
```
C:\Users\MAI_KHNV\Desktop\EMR_LienChieu\EMR_LienChieu\
├── src\
│   ├── EMR.Api\              ASP.NET Core 8 Web API
│   ├── EMR.Domain\           Entities, business rules
│   ├── EMR.Infrastructure\   EF Core, SQL Server, Storage, Signing
│   ├── EMR.Signing\          PAdES + VNPT-CA + USB Token wrapper
│   ├── EMR.HisSync\          Đọc DB eHospital read-only
│   ├── EMR.Web\              Blazor Server admin portal
│   └── EMR.Mobile\           .NET MAUI (iOS + Android)
├── tests\
├── docs\
│   ├── architecture.md       (chưa viết)
│   ├── db-schema.md          (chưa viết)
│   └── api-spec.md           (chưa viết)
├── PROJECT_BRIEF.md          (file này)
└── SETUP_GUIDE.md            Hướng dẫn cài máy dev
```

## 4. Roadmap

### Tuần 1-3: Vertical Slice #1 (đang chuẩn bị)
**Mục tiêu**: 1 lát cắt dọc xuyên suốt 4 module ở mức tối thiểu, để chứng minh kiến trúc.

Flow demo: `Upload PDF bệnh án → Ký số (self-signed) → Lưu kho → App mobile login + xem PDF`

Chia nhỏ:
- [ ] Tạo solution skeleton (.NET 8, các project ở mục 3.3)
- [ ] DB schema kho EMR (HoSoBenhAn, TaiLieu, ChuKy, AuditLog, NguoiDung, VaiTro)
- [ ] DB giả lập eHospital (BenhNhan, LanKham) với data mẫu
- [ ] API: POST /api/hoso (upload), POST /api/hoso/{id}/sign, GET /api/hoso (list), GET /api/hoso/{id}/pdf
- [ ] Auth JWT + login bằng username/password
- [ ] MAUI app: login, list hồ sơ, view PDF
- [ ] Test ký self-signed → verify chữ ký hợp lệ

### Sau MVP — mở rộng theo thứ tự
1. Workflow ký số nhiều cấp (BS → Trưởng khoa → PGĐ → GĐ), SLA, ủy quyền
2. Kho 2 cấp Khoa/Bệnh viện + rule engine kiểm tra đủ tài liệu trước khi chuyển
3. Tích hợp VNPT-CA thật (thay self-signed)
4. Đồng bộ HIS (đọc DB eHospital read-only)
5. Cổng BN web + đầy đủ tính năng app (OTP Zalo, thanh toán QR, ký mẫu đồng ý)
6. Audit log đầy đủ + báo cáo

## 5. Trạng thái HIỆN TẠI (tính đến 2026-05-08)

- [x] Đã thống nhất scope 4 module
- [x] Đã thống nhất stack & kiến trúc (chạy song song eHospital)
- [x] Đã chọn path `C:\Users\MAI_KHNV\Desktop\EMR_LienChieu\EMR_LienChieu\` (đổi từ `D:\EMR_LienChieu\` vì máy mới không có ổ D:)
- [x] Đã viết brief này
- [x] Máy server test (Win 11, 16GB RAM, 512GB SSD): SQL Server 2025 Standard Developer + .NET SDK 10.0.203 + Visual Studio 2026 Community + Git + SSMS 22 — đã cài xong (2026-05-08)
- [x] .NET 8.0.420 SDK — đã cài, pin qua `global.json`
- [x] Solution skeleton — đã tạo: `EMR.sln` + 6 project (Api, Domain, Infrastructure, Signing, HisSync, Web), build pass 0 warning
- [x] Tài liệu VNPT SmartCA v4.0 — đã đọc, tóm tắt ở `docs/smartca-api.md`, file gốc copy vào `docs/VNPT_SmartCA_v4.1.pdf`
- [x] DB schema — đã thiết kế ở `docs/db-schema.md` (2 DB: `EMR_LienChieu` 8 bảng + `eHospital_Demo` 2 bảng giả lập)
- [ ] EMR.Mobile (.NET MAUI) — chưa tạo (chờ cài workload `maui` cho .NET 8 khi cần demo mobile)
- [x] EF Core entities + DbContext + 2 migration `Init` — đã tạo, đã apply lên SQL Server local
- [x] DB `EMR_LienChieu` (8 bảng) + DB `eHospital_Demo` (2 bảng) đã tạo trên localhost
- [x] Seed: 8 vai trò, 33 khoa/phòng/TYT thật của TTYT Liên Chiểu
- [x] Seed bệnh nhân + lần khám demo trong eHospital_Demo (10 BN, ~20 lần khám)
- [x] Seed 5 user demo: `admin/admin@123`, `bs.an/bs@123`, `tk.binh/tk@123` (BS+TK), `ld.cuong/ld@123` (LĐ BV), `khth.dung/khth@123` (KHTH)
- [x] **Vertical slice #1 backend xong**: JWT login → HIS lookup → upload PDF → tạo hồ sơ (mã `26.000002`) → ký self-signed → verify chữ ký valid (smoke test pass 2026-05-08)
- [x] **Web Admin (Blazor Server) xong (Phase A)**: cookie auth, /hoso list, /hoso/moi tạo, /hoso/{id} chi tiết với 11 biểu mẫu chuẩn, nút Import PDF + nút Ký + inline PDF viewer
- [x] Git repo: https://github.com/michaltran/EMR (đã push 2 commits)
- [ ] EMR.Mobile (.NET MAUI) — chưa tạo, demo PDF download bằng Postman/Swagger trước
- [ ] PAdES embed thật vào PDF (hiện mới logical sign — lưu signature trong DB) — sau khi có VNPT-CA
- [ ] Webhook endpoint cho VNPT SmartCA — sau khi có tài khoản UAT

**Bước tiếp theo ngay khi máy mới sẵn sàng**:
1. User cài môi trường theo `SETUP_GUIDE.md`
2. User chạy 2 lệnh kiểm tra (`dotnet --version` + `sqlcmd -S localhost -Q "SELECT @@VERSION" -C`) gửi cho Claude
3. Claude tạo solution skeleton + DB schema
4. Code vertical slice #1

## 6. Quy ước làm việc với user

- **Ngôn ngữ giao tiếp**: Tiếng Việt
- **Phong cách user**: thực tế, thích "làm thử thấy hình hài" trước khi commit lớn → đồng ý cách tiếp cận MVP / vertical slice
- **Không tự ý làm việc lớn**: mỗi bước xong báo user xem trước khi đi tiếp
- **Folder cũ `D:\BIN_new\`**: chỉ để tham khảo nghiệp vụ, KHÔNG sửa, KHÔNG decompile trừ khi user yêu cầu

## 7. Câu hỏi đang chờ user

- **VNPT-CA API**: User đã liên hệ. Khi có file PDF/Word tài liệu API → upload cho Claude ở giai đoạn cần ký thật.
- **DB HIS thật**: Chưa cần xin quyền truy cập. MVP sẽ dùng DB giả lập trên máy local.

## 8. Tham chiếu

- File danh mục: `Danh Muc BV LC 08052026.xlsx` (user gửi qua chat, không có sẵn ở folder dự án — nên xin user upload lại nếu cần)
- HIS cũ: `D:\BIN_new\` (read-only reference)
- Pháp lý: Nghị định 130/2018/NĐ-CP (chữ ký số), Thông tư 46/2018/TT-BYT (bệnh án điện tử), Nghị định 13/2023 (dữ liệu cá nhân)
