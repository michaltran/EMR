using System.Security.Claims;
using System.Security.Cryptography;
using EMR.Domain.Entities;
using EMR.Domain.Enums;
using EMR.Infrastructure.Persistence;
using EMR.Infrastructure.Storage;
using EMR.Signing;
using Microsoft.EntityFrameworkCore;

namespace EMR.Api.Endpoints;

public static class SignEndpoints
{
    public record SignRequest(Guid TaiLieuId, string VaiTroKy);
    public record SignResponse(Guid ChuKyId, string TrangThai, string CertSubject, DateTime NgayHoanTat);
    public record VerifyResponse(bool IsValid, string? CertSubject, DateTime? NotAfter, string? Reason, string FileSha256);

    public static IEndpointRouteBuilder MapSign(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/hoso").WithTags("ChuKy").RequireAuthorization();

        g.MapPost("/{id:guid}/sign", async (
            Guid id,
            SignRequest req,
            ClaimsPrincipal user,
            EmrDbContext db,
            IFileStorage storage,
            IDocumentSigner signer,
            HttpContext http,
            CancellationToken ct) =>
        {
            var t = await db.TaiLieus.Include(x => x.HoSoBenhAn)
                .FirstOrDefaultAsync(x => x.Id == req.TaiLieuId && x.HoSoBenhAnId == id, ct);
            if (t is null) return Results.NotFound(new { error = "Không tìm thấy tài liệu trong hồ sơ này" });

            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var nguoiKy = await db.NguoiDungs.FirstAsync(u => u.Id == userId, ct);
            if (string.IsNullOrEmpty(nguoiKy.CCCD))
                return Results.BadRequest(new { error = "User chưa có CCCD, không thể ký số" });

            // Read file bytes & hash
            byte[] bytes;
            await using (var s = await storage.OpenReadAsync(t.DuongDanLuuTru, ct))
            await using (var ms = new MemoryStream())
            {
                await s.CopyToAsync(ms, ct);
                bytes = ms.ToArray();
            }
            var hash = SHA256.HashData(bytes);
            var hashHex = Convert.ToHexString(hash).ToLowerInvariant();

            t.TrangThaiKy = TrangThaiKyTaiLieu.DangKy;

            var chuKy = new ChuKy
            {
                TaiLieuId = t.Id,
                NguoiKyId = nguoiKy.Id,
                VaiTroKy = req.VaiTroKy,
                LoaiCa = LoaiCa.SelfSigned,
                Sha256TruocKy = hashHex,
                TrangThai = TrangThaiChuKy.ChoXacNhan
            };
            db.ChuKys.Add(chuKy);
            await db.SaveChangesAsync(ct);

            try
            {
                var result = await signer.SignHashAsync(hash, nguoiKy.CCCD!, nguoiKy.HoTen, ct);
                chuKy.SignatureValue = result.SignatureValueBase64;
                chuKy.CertSubject = result.CertSubject;
                chuKy.SmartCa_SerialNumber = result.CertSerialNumber;
                chuKy.CertNotBefore = result.NotBefore;
                chuKy.CertNotAfter = result.NotAfter;
                chuKy.TrangThai = TrangThaiChuKy.DaKy;
                chuKy.NgayHoanTat = DateTime.UtcNow;
                t.TrangThaiKy = TrangThaiKyTaiLieu.DaKy;

                // Cập nhật trạng thái hồ sơ theo vai trò ký
                t.HoSoBenhAn.TrangThai = req.VaiTroKy switch
                {
                    "BACSI" => TrangThaiHoSo.DaKyBacSi,
                    "TRUONGKHOA" => TrangThaiHoSo.DaKyTruongKhoa,
                    "LANHDAO_BV" => TrangThaiHoSo.DaKyLanhDao,
                    _ => t.HoSoBenhAn.TrangThai
                };
                t.HoSoBenhAn.NgayCapNhat = DateTime.UtcNow;

                db.AuditLogs.Add(new AuditLog
                {
                    HanhDong = "KY_SO_SUCCESS",
                    ActorId = nguoiKy.Id,
                    ActorTen = nguoiKy.HoTen,
                    LoaiDoiTuong = nameof(ChuKy),
                    DoiTuongId = chuKy.Id,
                    IpAddress = http.Connection.RemoteIpAddress?.ToString(),
                    Chitiet = $$"""{"vaiTro":"{{req.VaiTroKy}}","loaiCa":"SELF_SIGNED","cert":"{{result.CertSubject}}"}"""
                });

                await db.SaveChangesAsync(ct);
                return Results.Ok(new SignResponse(chuKy.Id, chuKy.TrangThai.ToString(), result.CertSubject, chuKy.NgayHoanTat!.Value));
            }
            catch (Exception ex)
            {
                chuKy.TrangThai = TrangThaiChuKy.ThatBai;
                chuKy.LyDoLoi = ex.Message;
                t.TrangThaiKy = TrangThaiKyTaiLieu.Loi;
                db.AuditLogs.Add(new AuditLog
                {
                    HanhDong = "KY_SO_FAIL",
                    ActorId = nguoiKy.Id,
                    ActorTen = nguoiKy.HoTen,
                    LoaiDoiTuong = nameof(ChuKy),
                    DoiTuongId = chuKy.Id,
                    Chitiet = $$"""{"error":"{{ex.Message.Replace("\"", "'")}}"}"""
                });
                await db.SaveChangesAsync(ct);
                return Results.Problem("Ký số thất bại: " + ex.Message);
            }
        });

        g.MapGet("/{id:guid}/tailieu/{taiLieuId:guid}/verify", async (
            Guid id, Guid taiLieuId, EmrDbContext db, IFileStorage storage, IDocumentSigner signer, CancellationToken ct) =>
        {
            var t = await db.TaiLieus.Include(x => x.ChuKys)
                .FirstOrDefaultAsync(x => x.Id == taiLieuId && x.HoSoBenhAnId == id, ct);
            if (t is null) return Results.NotFound();

            byte[] bytes;
            await using (var s = await storage.OpenReadAsync(t.DuongDanLuuTru, ct))
            await using (var ms = new MemoryStream())
            {
                await s.CopyToAsync(ms, ct);
                bytes = ms.ToArray();
            }
            var hash = SHA256.HashData(bytes);
            var hashHex = Convert.ToHexString(hash).ToLowerInvariant();

            var results = new List<object>();
            foreach (var ck in t.ChuKys.Where(c => c.TrangThai == TrangThaiChuKy.DaKy))
            {
                if (ck.SignatureValue is null || ck.CertSubject is null) continue;
                var v = await signer.VerifyAsync(hash, ck.SignatureValue, ck.CertSubject, ct);
                results.Add(new { chuKyId = ck.Id, vaiTroKy = ck.VaiTroKy, isValid = v.IsValid, certSubject = v.CertSubject, notAfter = v.NotAfter, reason = v.Reason });
            }

            return Results.Ok(new
            {
                taiLieuId = t.Id,
                fileSha256 = hashHex,
                fileSha256_KhoiTao = t.Sha256,
                fileToanVen = hashHex == t.Sha256,
                soChuKy = t.ChuKys.Count,
                ketQua = results
            });
        });

        return app;
    }
}
