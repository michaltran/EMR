# Tham chiếu pháp lý — EMR Liên Chiểu

> Hệ thống thiết kế tuân theo các văn bản dưới đây. Khi quy định thay đổi, cập nhật file này + diff sang code.

## 1. Văn bản trực tiếp về HSBA điện tử

### TT 13/2025/TT-BYT — Hướng dẫn triển khai HSBA điện tử
- **Hiệu lực**: 21/07/2025
- **Thay thế**: TT 46/2018/TT-BYT (cũ) + Mục VIII Phụ lục I của TT 54/2017
- **Lộ trình bắt buộc**:
  - Bệnh viện: hoàn thành **trước 30/9/2025**
  - Cơ sở khác (nội/ngoại trú/điều trị ban ngày): trước **31/12/2026**

#### Điều 3 — 3 hình thức ký/xác nhận điện tử:
1. **Chữ ký điện tử hợp pháp** — cert CA hợp lệ (VNPT, Viettel, FPT, BkavCA…). Áp dụng SmartCA trong dự án này.
2. **Sinh trắc học** — vân tay, khuôn mặt, mống mắt, giọng nói (theo chuẩn).
3. **Phương tiện điện tử khác** theo khoản 4 Điều 22 Luật Giao dịch điện tử 2023 (OTP, password kết hợp với hệ thống xác thực mạnh).

#### Điều 1 — Nguyên tắc:
- HSBA điện tử **phải kết nối với số định danh cá nhân** (CCCD/VNeID).
- Tuân thủ Luật KBCB 2023, Luật GDDT 2023, Luật ATTT mạng, NĐ 13/2023 (DLCN).
- Cập nhật, lưu trữ, khai thác **bằng phương tiện điện tử**.

→ **EMR Liên Chiểu hiện tại** dùng self-signed cert (loại 1 – mức dev). Production sẽ chuyển sang VNPT SmartCA. Cần thiết kế thêm path 2 (sinh trắc) cho mobile app (FaceID/TouchID).

### TT 32/2023/TT-BYT — Chi tiết Luật KBCB 2023
- **Hiệu lực**: từ 01/01/2024
- **Chương X (Điều 51-52)**: HSBA — quy định mẫu, cách ghi chép
- **Phụ lục XXVIII**: Mẫu bệnh án (24 mẫu theo chuyên khoa)
- **Phụ lục XXIX**: Mẫu giấy/phiếu y (53 mẫu, mã `01/BV2`–`53/BV2`) + 2 hướng dẫn
- **Tổng 82 mẫu chuẩn** (Điều 51 khoản 1)

#### Điều 52 — Yêu cầu ghi chép:
- Chính xác, trung thực, đầy đủ
- Tuân thủ hướng dẫn chuyên môn
- Từ ngữ rõ ràng, **không viết tắt** trong các tài liệu cung cấp cho BN (tóm tắt HSBA, chuyển tuyến...)
- Mỗi nội dung phải **thể hiện rõ thời gian + người ghi chép**
  → DB của EMR cần có `NgayTao`, `NguoiTaoId` cho mọi entity ghi chép, và phải lock immutable sau khi ký.

## 2. Văn bản nền

| Văn bản | Phạm vi |
|---|---|
| **Luật KBCB 2023** (số 15/2023/QH15) | Điều 69 (HSBA) — căn cứ chính |
| **Luật GDDT 2023** (số 20/2023/QH15) | Điều 22 (xác nhận điện tử), Điều 27 (chữ ký số) |
| **Luật ATTT mạng** | Bảo mật, lưu trữ |
| **NĐ 137/2024/NĐ-CP** | GDDT của cơ quan nhà nước |
| **NĐ 13/2023/NĐ-CP** | Bảo vệ dữ liệu cá nhân (DLCN) |
| **NĐ 42/2025/NĐ-CP** | Cơ cấu Bộ Y tế (TT 13/2025 căn cứ) |

## 3. Tham chiếu kỹ thuật

| Chuẩn | Sử dụng |
|---|---|
| **PAdES (PDF Advanced Electronic Signatures)** | Định dạng chữ ký số nhúng vào PDF — tuân theo [ETSI EN 319 142-1](https://www.etsi.org/) |
| **SHA-256 + RSA-2048 (tối thiểu)** | Hash + chữ ký |
| **TSA timestamp** (RFC 3161) | Đóng dấu thời gian, chống chối bỏ |
| **VNPT SmartCA API v4.0** | xem `docs/smartca-api.md` |

## 4. Mapping điều khoản → tính năng EMR

| Điều / yêu cầu | Tính năng trong EMR | Trạng thái |
|---|---|---|
| TT 13/2025 Đ.1 — kết nối CCCD | Cột `NguoiDung.CCCD` (UNIQUE), `BenhNhan.CCCD` | ✅ Có |
| TT 13/2025 Đ.3 — 3 hình thức ký | `IDocumentSigner`, `LoaiCa` enum | ✅ Self-signed; ⏳ VNPT-CA, Sinh trắc |
| TT 13/2025 Đ.2 — hạ tầng CNTT đầy đủ | Backup, monitoring, HTTPS, redundancy | ⏳ Production deploy |
| TT 32/2023 Đ.51 — 82 mẫu BYT | `BieuMauCatalog.cs` | ⏳ Đang thay 11→77 |
| TT 32/2023 Đ.52 — không viết tắt | Frontend validate text inputs | ⏳ |
| TT 32/2023 Đ.52 — thời gian + người ghi | `NgayTao`/`ActorId` ở mọi entity + AuditLog | ✅ Có |
| Luật KBCB Đ.69 — bảo mật, lưu ≥ 5 năm | Storage policy, AuditLog append-only | ⏳ Cần policy + immutable storage |

## 5. TODO compliance trước go-live

- [ ] Implement đủ 77+ mẫu chuẩn theo XXVIII + XXIX
- [ ] Chuyển self-signed → VNPT SmartCA (có hợp đồng UAT)
- [ ] Đường dẫn ký sinh trắc cho mobile (TT 13 Đ.3.2)
- [ ] PAdES embed thật vào PDF (hiện logical sign)
- [ ] Audit log immutable (WORM storage hoặc append-only DB với trigger)
- [ ] Backup tự động + DR plan
- [ ] Đăng ký kết nối VNeID (theo NĐ 137/2024)
- [ ] Quy chế quản lý HSBA điện tử (theo TT 13/2025 Đ.6.3.b — BV phải tự ban hành)
- [ ] Lưu trữ ≥ 5 năm theo Luật KBCB
- [ ] Đánh giá tiêu chí TT 54/2017 đã sửa đổi (bỏ Mục VIII)
