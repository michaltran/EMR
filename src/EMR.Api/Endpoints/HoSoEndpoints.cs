using System.Security.Claims;
using System.Security.Cryptography;
using EMR.Domain.Entities;
using EMR.Domain.Entities.His;
using EMR.Domain.Enums;
using EMR.Infrastructure.Persistence;
using EMR.Infrastructure.Services;
using EMR.Infrastructure.Storage;
using EMR.Signing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EMR.Api.Endpoints;

public static class HoSoEndpoints
{
    public record CreateHoSoForm(string MaBenhNhanHIS, string? MaLanKhamHIS, string LoaiTaiLieu);

    public record HoSoListItem(Guid Id, string MaHoSo, string MaBenhNhanHIS, string HoTenBenhNhan, string KhoaTen, string TrangThai, int SoTaiLieu, DateTime NgayTao);

    public record HoSoDetail(
        Guid Id, string MaHoSo, string MaBenhNhanHIS, string? MaLanKhamHIS, string HoTenBenhNhan,
        DateTime? NgaySinh, byte? GioiTinh, string KhoaTen, string TrangThai, string KhoLuuTru,
        DateTime NgayTao, IEnumerable<TaiLieuItem> TaiLieus);

    public record TaiLieuItem(Guid Id, string LoaiTaiLieu, string TenFile, long KichThuoc, string TrangThaiKy, IEnumerable<ChuKyItem> ChuKys);
    public record ChuKyItem(Guid Id, string VaiTroKy, string LoaiCa, string TrangThai, string? CertSubject, DateTime NgayYeuCau, DateTime? NgayHoanTat);

    public static IEndpointRouteBuilder MapHoSo(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/hoso").WithTags("HoSoBenhAn").RequireAuthorization();

        g.MapGet("/", async (EmrDbContext db, CancellationToken ct) =>
        {
            var items = await db.HoSoBenhAns
                .Include(h => h.Khoa)
                .Include(h => h.TaiLieus)
                .OrderByDescending(h => h.NgayTao)
                .Take(100)
                .Select(h => new HoSoListItem(
                    h.Id, h.MaHoSo, h.MaBenhNhanHIS, h.HoTenBenhNhan, h.Khoa.Ten,
                    h.TrangThai.ToString(), h.TaiLieus.Count, h.NgayTao))
                .ToListAsync(ct);
            return Results.Ok(items);
        });

        g.MapGet("/{id:guid}", async (Guid id, EmrDbContext db, CancellationToken ct) =>
        {
            var h = await db.HoSoBenhAns
                .Include(x => x.Khoa)
                .Include(x => x.TaiLieus).ThenInclude(t => t.ChuKys)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
            if (h is null) return Results.NotFound();

            var dto = new HoSoDetail(
                h.Id, h.MaHoSo, h.MaBenhNhanHIS, h.MaLanKhamHIS, h.HoTenBenhNhan,
                h.NgaySinh, h.GioiTinh, h.Khoa.Ten, h.TrangThai.ToString(), h.KhoLuuTru.ToString(),
                h.NgayTao,
                h.TaiLieus.Select(t => new TaiLieuItem(
                    t.Id, t.LoaiTaiLieu, t.TenFile, t.KichThuoc, t.TrangThaiKy.ToString(),
                    t.ChuKys.Select(c => new ChuKyItem(c.Id, c.VaiTroKy, c.LoaiCa.ToString(), c.TrangThai.ToString(), c.CertSubject, c.NgayYeuCau, c.NgayHoanTat))
                )));
            return Results.Ok(dto);
        });

        g.MapPost("/", async (
            HttpRequest req,
            ClaimsPrincipal user,
            EmrDbContext db,
            HisDemoDbContext his,
            IFileStorage storage,
            HoSoCodeGenerator codeGen,
            HttpContext http,
            CancellationToken ct) =>
        {
            if (!req.HasFormContentType) return Results.BadRequest(new { error = "multipart/form-data required" });
            var form = await req.ReadFormAsync(ct);
            var maBN = form["maBenhNhanHIS"].ToString();
            var maLK = form["maLanKhamHIS"].ToString();
            var loaiTL = string.IsNullOrWhiteSpace(form["loaiTaiLieu"]) ? "BENH_AN_TONG_HOP" : form["loaiTaiLieu"].ToString();
            var file = form.Files["file"];

            if (string.IsNullOrWhiteSpace(maBN)) return Results.BadRequest(new { error = "Thiếu maBenhNhanHIS" });
            if (file is null || file.Length == 0) return Results.BadRequest(new { error = "Thiếu file PDF" });
            if (file.ContentType != "application/pdf" && !file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return Results.BadRequest(new { error = "Chỉ chấp nhận file PDF" });

            var bn = await his.BenhNhans.FirstOrDefaultAsync(x => x.Ma == maBN, ct);
            if (bn is null) return Results.NotFound(new { error = $"Không tìm thấy bệnh nhân {maBN} trong HIS" });

            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var actor = await db.NguoiDungs.Include(u => u.Khoa).FirstAsync(u => u.Id == userId, ct);
            if (actor.KhoaId is null) return Results.BadRequest(new { error = "User không thuộc khoa nào, không thể tạo hồ sơ" });

            var maHoSo = await codeGen.NextAsync(ct);
            var hoSo = new HoSoBenhAn
            {
                MaHoSo = maHoSo,
                MaBenhNhanHIS = bn.Ma,
                MaLanKhamHIS = string.IsNullOrWhiteSpace(maLK) ? null : maLK,
                HoTenBenhNhan = bn.HoTen,
                NgaySinh = bn.NgaySinh,
                GioiTinh = bn.GioiTinh,
                KhoaId = actor.KhoaId.Value,
                BacSiTaoId = actor.Id,
                TrangThai = TrangThaiHoSo.ChoKy
            };
            db.HoSoBenhAns.Add(hoSo);

            // Save file
            string sha;
            byte[] bytes;
            await using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms, ct);
                bytes = ms.ToArray();
                sha = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
            }
            var subPath = $"{DateTime.UtcNow:yyyy/MM}/{hoSo.Id}";
            string relPath;
            await using (var src = new MemoryStream(bytes))
                relPath = await storage.SaveAsync(src, subPath, file.FileName, ct);

            var taiLieu = new TaiLieu
            {
                HoSoBenhAnId = hoSo.Id,
                LoaiTaiLieu = loaiTL,
                TenFile = file.FileName,
                DuongDanLuuTru = relPath,
                KichThuoc = bytes.LongLength,
                MimeType = "application/pdf",
                Sha256 = sha,
                NguoiUploadId = actor.Id,
                TrangThaiKy = TrangThaiKyTaiLieu.ChuaKy
            };
            db.TaiLieus.Add(taiLieu);

            db.AuditLogs.Add(new AuditLog
            {
                HanhDong = "TAO_HOSO",
                ActorId = actor.Id,
                ActorTen = actor.HoTen,
                LoaiDoiTuong = nameof(HoSoBenhAn),
                DoiTuongId = hoSo.Id,
                IpAddress = http.Connection.RemoteIpAddress?.ToString(),
                Chitiet = $$"""{"maHoSo":"{{maHoSo}}","maBN":"{{bn.Ma}}","fileSize":{{bytes.LongLength}}}"""
            });

            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/hoso/{hoSo.Id}", new { hoSo.Id, hoSo.MaHoSo, taiLieuId = taiLieu.Id });
        }).DisableAntiforgery();

        g.MapGet("/{id:guid}/tailieu/{taiLieuId:guid}/file", async (
            Guid id, Guid taiLieuId, EmrDbContext db, IFileStorage storage, CancellationToken ct) =>
        {
            var t = await db.TaiLieus.FirstOrDefaultAsync(x => x.Id == taiLieuId && x.HoSoBenhAnId == id, ct);
            if (t is null) return Results.NotFound();
            if (!storage.Exists(t.DuongDanLuuTru)) return Results.NotFound(new { error = "File không tồn tại" });
            var stream = await storage.OpenReadAsync(t.DuongDanLuuTru, ct);
            return Results.File(stream, t.MimeType, t.TenFile);
        });

        return app;
    }
}
