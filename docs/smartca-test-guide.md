# Hướng dẫn test VNPT SmartCA TH (Tích hợp)

> File này chỉ là hướng dẫn quy trình. **Credentials thật nằm trong `src/EMR.*/appsettings.local.json`** — file đã ignore khỏi git.

## 1. Cấu trúc tài khoản SmartCA

VNPT phân 2 cấp:

| Cấp | Vai trò | Ai có | Lưu ở đâu |
|---|---|---|---|
| **3rd Party (SP)** | Định danh ứng dụng EMR Liên Chiểu (1 lần duy nhất cho BV) | Do VNPT cấp khi BV ký hợp đồng | `VnptSmartCa.SpId` + `SpPassword` trong `appsettings.local.json` |
| **End user** | Mỗi BS / lãnh đạo có riêng 1 tài khoản | BS đăng ký với VNPT | `VnptSmartCa.Users[]` trong `appsettings.local.json` |

## 2. Hai loại flow ký

### V1 — SmartCA thường (mặc định)
- BS ấn "Ký" trong EMR
- EMR gọi `/v1/signatures/sign` với hash
- VNPT push notification tới app **VNPT SmartCA** trên điện thoại BS
- BS mở app, bấm "Đồng ý ký"
- EMR poll status hoặc nhận webhook → có signature

### TH (Tích hợp) — automation
- Yêu cầu BS có **TOTP key** (VNPT cấp khi đăng ký gói TH)
- EMR tự sinh OTP từ TOTP key (không cần app điện thoại)
- 2-step API: `/v2/signatures/sign` → `/v2/signatures/confirm`
- Phù hợp ký hàng loạt, không gián đoạn workflow BS

## 3. Bật chế độ thật

### Bước 1: Cập nhật `src/EMR.Web/appsettings.local.json` (và `src/EMR.Api/appsettings.local.json`)

```json
{
  "VnptSmartCa": {
    "Enabled": true,
    "Mode": "TH",
    "BaseUrl": "https://rmgateway.vnptit.vn/sca/sp769",
    "SpId": "<lấy từ VNPT khi BV ký hợp đồng>",
    "SpPassword": "<lấy từ VNPT khi BV ký hợp đồng>",
    "Users": [
      {
        "Cccd": "067096005163",
        "Password": "<password VNPT của BS>",
        "SerialNumber": "54010101dab00f6cc8009c3f884d04ad",
        "TotpKey": "NkY5QTM2RkQzRENGMDRDQUE1QUZGNzc1Qzk2NDdDOEE="
      }
    ]
  }
}
```

### Bước 2: Đảm bảo trong DB EMR có user với cùng CCCD

- Login `admin/admin@123` → vào **/admin/users**
- Tạo hoặc sửa 1 user có **CCCD = 067096005163**
- Gán vai trò BS / TK / LĐ phù hợp

### Bước 3: Khởi động lại Web

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' run --project src\EMR.Web
```

### Bước 4: Test

- Login với user có CCCD trên
- Mở 1 hồ sơ → nhấn "Ký (BACSI)" trên 1 biểu mẫu
- Xem console log:
  ```
  SmartCA TH: gen OTP cho CCCD=067096005163
  SmartCA TH sign: cccd=067096005163 tran=...
  SmartCA TH confirm response: ...
  ```

### Lỗi thường gặp

| Lỗi | Nguyên nhân | Fix |
|---|---|---|
| `401 SP_CREDENTIAL_INVALID` | Sai sp_id/sp_password | Lấy đúng từ email VNPT cấp |
| `403 CREDENTIAL_STATUS_INVALID` | Cert hết hạn / chưa kích hoạt | Login app SmartCA → kích hoạt cert |
| `Không tìm thấy config SmartCA cho CCCD ...` | NguoiDung.CCCD không khớp với entry trong `Users[]` | Sửa CCCD trong /admin/users hoặc bổ sung vào Users[] |
| `User ... thiếu Password hoặc SerialNumber` | Thiếu field trong appsettings.local.json | Bổ sung |
| OTP sai liên tục | TOTP key sai format hoặc giờ máy lệch | Kiểm tra giờ Windows (tự động sync NTP) |

## 4. Webhook (nếu dùng V1)

V1 yêu cầu webhook hoặc poll. Hiện EMR **chưa expose webhook endpoint** (vì cần public HTTPS). Khi cần:
- Dev: dùng `ngrok http 5198` để forward localhost
- Prod: deploy sau reverse proxy
- Endpoint mới: `POST /api/smartca/webhook` (chưa code, sẽ thêm khi cần)

## 5. Quay về Self-signed (dev)

Đặt `Enabled: false` trong `appsettings.local.json` → fallback tự động về `SelfSignedDocumentSigner` đã có sẵn.

## 6. Bảo mật credentials

- ✅ `appsettings.local.json` đã trong `.gitignore`
- ✅ Không log password/TOTP key ra console
- ⚠️ Nếu deploy production: dùng **Azure Key Vault** / **Hashicorp Vault** thay vì file JSON
- ⚠️ Quay vòng password VNPT định kỳ
