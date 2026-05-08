# HANDOVER — Đọc file này đầu tiên khi mở Claude trên máy mới

## Đối với USER

Khi sang máy mới và mở Claude Code lần đầu trong folder này, **copy nguyên đoạn dưới đây gửi cho Claude**:

```
Tôi đang tiếp tục dự án EMR Liên Chiểu trên máy mới.

Hãy đọc theo thứ tự để nắm ngữ cảnh:
1. D:\EMR_LienChieu\PROJECT_BRIEF.md — bối cảnh, scope, kiến trúc, roadmap
2. D:\EMR_LienChieu\SETUP_GUIDE.md — checklist môi trường dev

Sau đó xác nhận với tôi: bạn đã đọc xong, đang ở bước nào của roadmap, và tôi cần làm gì tiếp theo.
```

Claude sẽ đọc 2 file đó và biết chính xác đang ở đâu.

---

## Đối với CLAUDE (instance mới)

Nếu bạn là Claude Code đang đọc file này lần đầu:

1. **Đọc đầy đủ** `PROJECT_BRIEF.md` (đặc biệt mục 5 "Trạng thái HIỆN TẠI" để biết đã làm tới đâu)
2. **Đọc** `SETUP_GUIDE.md` để biết môi trường dev cần gì
3. **Hỏi user**: máy đã cài xong môi trường chưa? Nếu rồi thì xin output 2 lệnh `dotnet --version` + `sqlcmd -S localhost -Q "SELECT @@VERSION" -C`
4. **Nếu môi trường OK** → bắt đầu tạo solution skeleton theo `PROJECT_BRIEF.md` mục 3.3
5. **Quy ước**: tiếng Việt, mỗi bước xong báo user xem trước khi đi tiếp, KHÔNG sửa folder `D:\BIN_new\` (HIS cũ)

Cập nhật `PROJECT_BRIEF.md` mục 5 mỗi khi hoàn thành 1 mốc lớn để các session sau biết tiến độ.

---

## Files trong folder này

| File | Mục đích |
|---|---|
| `HANDOVER.md` | File này — điểm bắt đầu khi mở Claude lần đầu |
| `PROJECT_BRIEF.md` | Toàn bộ ngữ cảnh dự án (scope, kiến trúc, roadmap, trạng thái) |
| `SETUP_GUIDE.md` | Hướng dẫn cài môi trường dev |
| `docs/` | Tài liệu chi tiết (sẽ tạo dần: architecture.md, db-schema.md, api-spec.md) |
| `src/` | Source code (chưa tạo, sẽ tạo sau khi môi trường sẵn sàng) |
