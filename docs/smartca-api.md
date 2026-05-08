# VNPT SmartCA — Tóm tắt API tích hợp

> Nguồn: `docs/VNPT_SmartCA_v4.1.pdf` (tài liệu chính thức VNPT, v4.0.0, 03/05/2024)

## 1. Hai loại tài khoản

| Loại | Đặc điểm | Khi nào dùng |
|---|---|---|
| **SmartCA thường** (mục 3) | User confirm trên app SmartCA mỗi lần ký | BS, lãnh đạo ký bệnh án — đúng quy trình pháp lý |
| **SmartCA TH** (Tích hợp, mục 4) | Ký bằng password + OTP, không cần confirm trên app | Ký hàng loạt automation, dấu phòng (eseal) |

EMR Liên Chiểu **MVP dùng SmartCA thường** cho ký bệnh án (đúng quy trình BS nhấn ký → confirm trên điện thoại). SmartCA TH có thể dùng sau cho dấu phòng / ký hàng loạt báo cáo.

## 2. Endpoints

| Môi trường | Base URL |
|---|---|
| UAT (test) | `https://rmgateway.vnptit.vn/sca/sp769` |
| Production | `https://gwsca.vnpt.vn/sca/sp769` |

Tất cả method **POST**, Content-Type `application/json`.

## 3. Flow ký SmartCA thường (3 API)

```
[User trên Web/App] → [EMR Backend] → [SmartCA] → [User App SmartCA confirm] → [Webhook → EMR]
```

### Bước 1: Lấy chứng thư số — `POST /v1/credentials/get_certificate`

Request:
```json
{
  "sp_id": "<cấp bởi VNPT>",
  "sp_password": "<cấp bởi VNPT>",
  "user_id": "<CCCD/CMND của BS>",
  "serial_number": "",
  "transaction_id": "<UUID do EMR sinh>"
}
```

Response: trả về `user_certificates[]` chứa `cert_id`, `serial_number`, `cert_data` (Base64), `chain_data.ca_cert`, `chain_data.root_cert`. Lưu cache theo `user_id` (cert valid ~3 năm).

### Bước 2: Yêu cầu ký — `POST /v1/signatures/sign`

Request:
```json
{
  "sp_id": "...",
  "sp_password": "...",
  "user_id": "<CCCD BS>",
  "transaction_id": "<UUID giao dịch ký>",
  "transaction_desc": "Ký bệnh án BA-2026-0001",
  "serial_number": "<lấy từ bước 1>",
  "time_stamp": "20260508120000Z",
  "sign_files": [
    {
      "doc_id": "<ID tài liệu nội bộ>",
      "file_type": "pdf",
      "sign_type": "hash",
      "data_to_be_signed": "<hex hash của PDF placeholder PAdES>"
    }
  ]
}
```

Response:
```json
{
  "status_code": 200,
  "message": "sig_wait_for_user_confirm",
  "data": {
    "transaction_id": "<echo lại>",
    "tran_code": "<UUID dùng cho bước 3>"
  }
}
```

⚠️ **SmartCA chỉ ký HASH, không ký file**. EMR phải tự:
1. Tạo PDF có signature placeholder (PAdES dictionary trống) bằng iText/PdfPig
2. Tính SHA-256 của byte range cần ký
3. Gửi hash hex cho SmartCA
4. Nhận `signature_value` về → embed vào placeholder

### Bước 3: Lấy signature — 2 cách

**Cách A: Webhook** (CA chủ động push, mục 3.5)
- EMR đăng ký webhook URL với VNPT
- CA POST tới webhook khi user confirm xong, body chứa `signed_files[]` với `doc_id`, `signature_value`, `timestamp_signature`

**Cách B: Poll** (EMR chủ động, mục 3.4)
- `POST /v1/signatures/sign/{tran_code}/status` — gọi định kỳ tới khi có `signatures[]`

→ Nên dùng **cả hai**: webhook là chính, poll là fallback nếu webhook miss.

## 4. Mã lỗi cần xử lý

| HTTP | Code | Ý nghĩa |
|---|---|---|
| 200 | SUCCESS | OK |
| 400 | BAD_REQUEST | Sai format request |
| 401 | SP_CREDENTIAL_INVALID | Sai sp_id/sp_password |
| 403 | CREDENTIAL_STATUS_INVALID | Cert hết hạn / bị thu hồi |
| 500 | SERVER_INTERNAL_ERROR | Lỗi phía CA |

## 5. Đăng ký tài khoản (cần làm trước khi tích hợp thật)

**3rd Party (EMR)**: cung cấp cho VNPT
- Tên hệ thống: `EMR Liên Chiểu`
- Mô tả: ứng dụng ký số bệnh án điện tử
- Email admin: nhận `client_id` + `client_secret`

**End user (BS, lãnh đạo)**: VNPT cần
- Họ tên, CCCD, email, số ĐT
- User kích hoạt qua app VNPT SmartCA

## 6. Sample code chính thức

- SmartCA: https://drive.google.com/drive/folders/15XKfk_PV4eiLpa4xvZlV2EnSEBBfIsV0
- SmartCA TH: https://drive.google.com/drive/folders/1RVmqD2yOguMo6NriGhHYBG26UPHLCwaV

## 7. Implication cho thiết kế EMR

1. **Module `EMR.Signing`** cần wrap 3 API + xử lý hash PAdES + embed signature
2. **DB cần** lưu: `transaction_id` (do EMR sinh), `tran_code` (CA trả), `cert_id`, status (`pending|signed|failed|timeout`), webhook payload raw
3. **Endpoint webhook** phải public (HTTPS) — môi trường dev có thể dùng ngrok / cloudflare tunnel
4. **Phase 1 (MVP)** dùng self-signed cert mô phỏng flow này (không gọi VNPT thật) để test logic. Khi có tài khoản UAT VNPT mới chuyển qua API thật.
