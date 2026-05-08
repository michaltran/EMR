# Database Schema — EMR Liên Chiểu

> Phiên bản: 0.1 (MVP — vertical slice #1)
> Ngày: 2026-05-08
> SQL Server: 2025 Standard Developer Edition

## 1. Tổng quan 2 database

| Tên DB | Vai trò | Ghi/Đọc | Migration |
|---|---|---|---|
| `EMR_LienChieu` | DB chính của hệ thống mới | Read/Write | EF Core code-first |
| `eHospital_Demo` | Giả lập DB HIS eHospital cũ (chỉ cho dev/test) | Read-only từ EMR | EF Core code-first (chỉ trong dev) |

Production sau này: `eHospital_Demo` được thay bằng connection string thật tới DB HIS sản xuất, EMR chỉ đọc.

---

## 2. DB `EMR_LienChieu`

### 2.1. Mô hình quan hệ (high-level)

```
NguoiDung ──┬─< NguoiDung_VaiTro >─── VaiTro
            │
            ├─< HoSoBenhAn (created_by, owner_dept)
            │       │
            │       └─< TaiLieu ──< ChuKy (signer_id → NguoiDung)
            │
            └─< AuditLog (actor_id)
```

### 2.2. Bảng `NguoiDung` — Người dùng hệ thống

Bác sĩ, điều dưỡng, lãnh đạo, admin... Mỗi người 1 tài khoản đăng nhập.

| Cột | Kiểu | Constraint | Ghi chú |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK | `NEWID()` default |
| `TenDangNhap` | `nvarchar(50)` | UNIQUE, NOT NULL | username, lowercase |
| `MatKhauHash` | `nvarchar(500)` | NOT NULL | BCrypt hash |
| `HoTen` | `nvarchar(200)` | NOT NULL | |
| `CCCD` | `varchar(20)` | UNIQUE | Map sang `user_id` của SmartCA |
| `Email` | `nvarchar(200)` | | |
| `SoDienThoai` | `varchar(20)` | | |
| `KhoaId` | `uniqueidentifier` | FK → `Khoa.Id`, NULL | NULL = lãnh đạo BV / admin |
| `TrangThai` | `tinyint` | NOT NULL, default 1 | 0=khóa, 1=hoạt động |
| `NgayTao` | `datetime2` | NOT NULL | |
| `NgayCapNhat` | `datetime2` | NOT NULL | |

Index: `IX_NguoiDung_TenDangNhap` (UNIQUE), `IX_NguoiDung_CCCD` (UNIQUE), `IX_NguoiDung_KhoaId`.

### 2.3. Bảng `VaiTro` — Vai trò

Seed sẵn 8 vai trò:

| Mã | Tên | Quyền chính |
|---|---|---|
| `BACSI` | Bác sĩ | Tạo hồ sơ, ký bệnh án mình tạo |
| `DIEUDUONG` | Điều dưỡng | Hỗ trợ tạo/cập nhật hồ sơ, ký phiếu chăm sóc |
| `DUOCSI` | Dược sĩ | Quản lý đơn thuốc, ký xác nhận cấp phát |
| `TRUONGKHOA` | Trưởng khoa | Ký duyệt cấp khoa, xem hồ sơ trong khoa |
| `KHTH` | Kế hoạch nghiệp vụ | Kiểm tra hồ sơ, chuyển kho BV, báo cáo |
| `LANHDAO_BV` | Lãnh đạo BV (PGĐ/GĐ) | Ký duyệt cấp BV, xem toàn bộ |
| `ADMIN` | Quản trị | Quản lý người dùng, vai trò, audit |
| `BENHNHAN` | Bệnh nhân | Đăng nhập app, xem hồ sơ của mình |

| Cột | Kiểu | Constraint |
|---|---|---|
| `Id` | `uniqueidentifier` | PK |
| `Ma` | `varchar(50)` | UNIQUE, NOT NULL |
| `Ten` | `nvarchar(100)` | NOT NULL |
| `MoTa` | `nvarchar(500)` | |

### 2.4. Bảng `NguoiDung_VaiTro` — Mapping (many-to-many)

| Cột | Kiểu | Constraint |
|---|---|---|
| `NguoiDungId` | `uniqueidentifier` | PK, FK → NguoiDung |
| `VaiTroId` | `uniqueidentifier` | PK, FK → VaiTro |

Composite PK = (`NguoiDungId`, `VaiTroId`).

### 2.5. Bảng `Khoa` — Khoa/Phòng/TYT của TTYT Liên Chiểu

Seed 33 đơn vị thật theo danh mục TTYT Liên Chiểu, gom 4 nhóm:

| Cột | Kiểu | Constraint |
|---|---|---|
| `Id` | `uniqueidentifier` | PK |
| `Ma` | `varchar(20)` | UNIQUE, NOT NULL |
| `Ten` | `nvarchar(200)` | NOT NULL |
| `Nhom` | `varchar(20)` | NOT NULL | `BGD` / `PHONG` / `KHOA` / `TYT` |
| `KhoaChaId` | `uniqueidentifier` | FK self, NULL | Cho khoa ngoại trú (vd Liên Chuyên Khoa Ngoại trú RHM thuộc Khoa Liên Chuyên Khoa) |
| `ThuTu` | `int` | NOT NULL, default 0 | Sắp xếp hiển thị |

**Danh sách seed** (34 đơn vị, theo ảnh user gửi):

*Ban Giám đốc & Phòng chức năng:*
- `BGD` Ban Giám đốc
- `P_TCHC` Phòng Tổ chức Hành chính
- `P_KHNV` Phòng Kế hoạch Nghiệp vụ
- `P_TCKT` Phòng Tài chính kế toán
- `P_DD` Phòng Điều dưỡng
- `P_TCCB` Phòng Tổ chức cán bộ
- `P_DS` Phòng Dân số
- `P_TC` Phòng Tiêm chủng
- `P_KSBT` Phòng Khám KSBT

*Khoa lâm sàng & cận lâm sàng:*
- `K_NGOAI` Khoa Ngoại
- `K_NOI` Khoa Nội
- `K_COVID` Khoa Điều trị COVID-19
- `K_SAN` Khoa Sản
- `K_NHI` Khoa Nhi
- `K_LCK` Khoa Liên Chuyên Khoa
- `K_YHCT` Khoa YHCT - VLTL&PHCN
- `K_HSCC` Khoa Hồi sức Cấp cứu
- `K_CDHA` Khoa Chẩn đoán Hình ảnh
- `K_XN` Khoa Xét nghiệm
- `K_DUOC` Khoa Dược-TTB-VTYT
- `K_KSBT` Khoa Kiểm soát bệnh tật và HIV/AIDS
- `K_YTCC` Khoa YTCC - DD & ATTP
- `K_KSNK` Khoa Kiểm soát nhiễm khuẩn
- `K_PK` Khoa Phòng khám
- `K_LCK_NTRU_RHM` Liên Chuyên Khoa (Ngoại trú RHM) — KhoaChaId → K_LCK
- `K_YHCT_NTRU` Khoa YHCT - VLTL&PHCN (Ngoại trú) — KhoaChaId → K_YHCT

*Trạm y tế vệ tinh:*
- `TYT_HKB` TYT Hòa Khánh Bắc
- `TYT_HKN` TYT Hòa Khánh Nam
- `TYT_HHB` TYT Hòa Hiệp Bắc
- `TYT_HHN` TYT Hòa Hiệp Nam
- `TYT_HM` TYT Hòa Minh
- `TYT_HV` Trạm y tế phường Hải Vân
- `TYT_KHAC` Trạm Y tế (chung)

### 2.6. Bảng `HoSoBenhAn` — Hồ sơ bệnh án

1 lần khám / 1 đợt điều trị = 1 hồ sơ. Có thể chứa nhiều tài liệu (giấy ra viện, đơn thuốc, kết quả XN...).

| Cột | Kiểu | Constraint | Ghi chú |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK | |
| `MaHoSo` | `varchar(20)` | UNIQUE, NOT NULL | Format `YY.NNNNNN` (vd `26.004951`) — 2 số cuối năm + sequence 6 số trong năm |
| `MaBenhNhanHIS` | `varchar(50)` | NOT NULL | Map sang `BenhNhan.Ma` ở DB HIS |
| `MaLanKhamHIS` | `varchar(50)` | | Map sang `LanKham.Ma` ở DB HIS |
| `HoTenBenhNhan` | `nvarchar(200)` | NOT NULL | Snapshot tại thời điểm tạo (đề phòng HIS đổi) |
| `NgaySinh` | `date` | | |
| `GioiTinh` | `tinyint` | | 0=nữ, 1=nam, 2=khác |
| `KhoaId` | `uniqueidentifier` | FK → Khoa, NOT NULL | Khoa sở hữu |
| `BacSiTaoId` | `uniqueidentifier` | FK → NguoiDung, NOT NULL | |
| `TrangThai` | `tinyint` | NOT NULL | 0=draft, 1=cho_ky, 2=da_ky_BS, 3=da_ky_TK, 4=da_ky_LD, 9=hoan_tat, 99=huy |
| `KhoLuuTru` | `tinyint` | NOT NULL, default 0 | 0=Khoa, 1=BV (sau khi đủ chữ ký) |
| `NgayTao` | `datetime2` | NOT NULL | |
| `NgayCapNhat` | `datetime2` | NOT NULL | |

Index: `IX_HoSoBenhAn_MaBenhNhanHIS`, `IX_HoSoBenhAn_KhoaId_TrangThai`, `IX_HoSoBenhAn_NgayTao`.

### 2.7. Bảng `TaiLieu` — Tài liệu trong hồ sơ

1 hồ sơ có nhiều tài liệu. Mỗi tài liệu là 1 file PDF.

| Cột | Kiểu | Constraint | Ghi chú |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK | |
| `HoSoBenhAnId` | `uniqueidentifier` | FK, NOT NULL | |
| `LoaiTaiLieu` | `varchar(50)` | NOT NULL | `BENH_AN_TONG_HOP`, `DON_THUOC`, `KQ_XET_NGHIEM`, `GIAY_RA_VIEN`, ... |
| `TenFile` | `nvarchar(500)` | NOT NULL | Tên gốc khi user upload |
| `DuongDanLuuTru` | `nvarchar(1000)` | NOT NULL | Đường dẫn relative trong file storage |
| `KichThuoc` | `bigint` | NOT NULL | Bytes |
| `MimeType` | `varchar(100)` | NOT NULL | `application/pdf` |
| `Sha256` | `varchar(64)` | NOT NULL | Hash file gốc (chưa ký) — dùng để đối chiếu |
| `TrangThaiKy` | `tinyint` | NOT NULL | 0=chua_ky, 1=dang_ky, 2=da_ky, 9=loi |
| `NguoiUploadId` | `uniqueidentifier` | FK → NguoiDung, NOT NULL | |
| `NgayUpload` | `datetime2` | NOT NULL | |

Index: `IX_TaiLieu_HoSoBenhAnId`, `IX_TaiLieu_TrangThaiKy`.

### 2.8. Bảng `ChuKy` — Chữ ký số trên tài liệu

1 tài liệu có thể có nhiều chữ ký (BS → TK → LĐ). Mỗi lần ký = 1 record.

| Cột | Kiểu | Constraint | Ghi chú |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK | |
| `TaiLieuId` | `uniqueidentifier` | FK, NOT NULL | |
| `NguoiKyId` | `uniqueidentifier` | FK → NguoiDung, NOT NULL | |
| `VaiTroKy` | `varchar(50)` | NOT NULL | `BACSI` / `TRUONGKHOA` / `LANHDAO_BV` |
| `LoaiCa` | `varchar(20)` | NOT NULL | `SELF_SIGNED` (MVP) / `VNPT_SMARTCA` |
| `SmartCa_TransactionId` | `varchar(100)` | | UUID EMR sinh, gửi cho SmartCA |
| `SmartCa_TranCode` | `varchar(100)` | | UUID CA trả về |
| `SmartCa_CertId` | `varchar(100)` | | |
| `SmartCa_SerialNumber` | `varchar(100)` | | Serial cert |
| `CertSubject` | `nvarchar(500)` | | Subject DN của cert |
| `CertNotBefore` | `datetime2` | | Cert hiệu lực từ |
| `CertNotAfter` | `datetime2` | | Cert hiệu lực đến |
| `Sha256TruocKy` | `varchar(64)` | NOT NULL | Hash của byte range PDF placeholder |
| `SignatureValue` | `nvarchar(max)` | | Base64 chữ ký nhận từ CA |
| `TimestampSignature` | `nvarchar(max)` | | Timestamp signature từ TSA (nếu có) |
| `DuongDanFileSau` | `nvarchar(1000)` | | Đường dẫn PDF sau khi embed signature |
| `TrangThai` | `tinyint` | NOT NULL | 0=cho_xac_nhan, 1=da_ky, 2=that_bai, 3=huy, 4=qua_han |
| `NgayYeuCau` | `datetime2` | NOT NULL | |
| `NgayHoanTat` | `datetime2` | | |
| `LyDoLoi` | `nvarchar(1000)` | | Nếu trạng thái=2 |
| `WebhookPayloadRaw` | `nvarchar(max)` | | Raw JSON từ webhook CA, lưu để audit |

Index: `IX_ChuKy_TaiLieuId`, `IX_ChuKy_NguoiKyId`, `IX_ChuKy_SmartCa_TransactionId` (UNIQUE WHERE NOT NULL), `IX_ChuKy_TrangThai`.

### 2.9. Bảng `AuditLog` — Nhật ký kiểm tra

Append-only. Mọi hành động ảnh hưởng đến bệnh án/chữ ký đều log.

| Cột | Kiểu | Constraint | Ghi chú |
|---|---|---|---|
| `Id` | `bigint` | PK IDENTITY | |
| `ThoiGian` | `datetime2` | NOT NULL, default `SYSUTCDATETIME()` | UTC |
| `ActorId` | `uniqueidentifier` | FK → NguoiDung, NULL | NULL nếu hệ thống/anonymous |
| `ActorTen` | `nvarchar(200)` | | Snapshot tên (trường hợp user bị xóa) |
| `HanhDong` | `varchar(50)` | NOT NULL | `LOGIN`, `UPLOAD_TAILIEU`, `KY_SO_REQUEST`, `KY_SO_SUCCESS`, `KY_SO_FAIL`, `XEM_HOSO`, `CHUYEN_KHO`, ... |
| `LoaiDoiTuong` | `varchar(50)` | | `HoSoBenhAn` / `TaiLieu` / `ChuKy` / `NguoiDung` |
| `DoiTuongId` | `uniqueidentifier` | | |
| `IpAddress` | `varchar(50)` | | |
| `UserAgent` | `nvarchar(500)` | | |
| `Chitiet` | `nvarchar(max)` | | JSON tự do |

Index: `IX_AuditLog_ThoiGian` (DESC), `IX_AuditLog_ActorId`, `IX_AuditLog_DoiTuongId_HanhDong`.

⚠️ **Append-only ở mức app**: không có `UPDATE` / `DELETE` trong code. Production có thể thêm trigger PREVENT, hoặc bật Temporal Table.

---

## 3. DB `eHospital_Demo` (giả lập HIS)

Chỉ 2 bảng tối thiểu để vertical slice #1 demo được flow "tra bệnh nhân từ HIS rồi tạo hồ sơ EMR".

### 3.1. Bảng `BenhNhan`

| Cột | Kiểu | Constraint |
|---|---|---|
| `Ma` | `varchar(50)` | PK |
| `HoTen` | `nvarchar(200)` | NOT NULL |
| `NgaySinh` | `date` | |
| `GioiTinh` | `tinyint` | |
| `DiaChi` | `nvarchar(500)` | |
| `SoDienThoai` | `varchar(20)` | |
| `CCCD` | `varchar(20)` | |

### 3.2. Bảng `LanKham`

| Cột | Kiểu | Constraint |
|---|---|---|
| `Ma` | `varchar(50)` | PK |
| `MaBenhNhan` | `varchar(50)` | FK → BenhNhan |
| `NgayVaoVien` | `datetime2` | NOT NULL |
| `NgayRaVien` | `datetime2` | |
| `KhoaDieuTri` | `nvarchar(200)` | |
| `ChanDoanRaVien` | `nvarchar(500)` | |

Seed ~10 bệnh nhân + 20 lần khám demo.

---

## 4. Chiến lược migrations

- 2 `DbContext` riêng: `EmrDbContext` (EMR_LienChieu), `HisDemoDbContext` (eHospital_Demo)
- Migration folder: `src/EMR.Infrastructure/Migrations/Emr/` và `src/EMR.Infrastructure/Migrations/HisDemo/`
- Lệnh tạo:
  ```powershell
  dotnet ef migrations add Init -c EmrDbContext -o Migrations/Emr -p src\EMR.Infrastructure -s src\EMR.Api
  dotnet ef migrations add Init -c HisDemoDbContext -o Migrations/HisDemo -p src\EMR.Infrastructure -s src\EMR.Api
  ```
- Apply:
  ```powershell
  dotnet ef database update -c EmrDbContext -p src\EMR.Infrastructure -s src\EMR.Api
  dotnet ef database update -c HisDemoDbContext -p src\EMR.Infrastructure -s src\EMR.Api
  ```

## 5. Connection strings (dev local)

`src/EMR.Api/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "Emr": "Server=localhost;Database=EMR_LienChieu;Trusted_Connection=True;TrustServerCertificate=True",
    "HisDemo": "Server=localhost;Database=eHospital_Demo;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

## 6. Còn TODO sau MVP

- Bảng `Workflow` + `WorkflowStep` cho quy trình ký nhiều bước có SLA, ủy quyền
- Bảng `RuleKiemTra` cho rule engine kiểm tra đủ tài liệu trước khi chuyển kho
- Bảng `OtpZalo` / `PhienBenhNhan` cho cổng BN
- Partition `AuditLog` theo tháng khi data lớn
- WORM thực sự: MinIO/Blob storage với immutable mode
