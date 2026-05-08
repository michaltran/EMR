using System.Security.Cryptography;
using EMR.Domain.Entities;
using EMR.Domain.Entities.His;
using EMR.Domain.Enums;
using EMR.Infrastructure.Persistence;
using EMR.Infrastructure.Services;
using EMR.Infrastructure.Storage;
using EMR.Signing;
using Microsoft.EntityFrameworkCore;

namespace EMR.Web.Services;

public class HoSoService(
    EmrDbContext db,
    HisDemoDbContext his,
    IFileStorage storage,
    HoSoCodeGenerator codeGen,
    IDocumentSigner signer)
{
    public async Task<List<HoSoListVM>> ListAsync(Guid? khoaId, string? keyword, CancellationToken ct = default)
    {
        var q = db.HoSoBenhAns.Include(h => h.Khoa).Include(h => h.TaiLieus).AsQueryable();
        if (khoaId.HasValue) q = q.Where(h => h.KhoaId == khoaId.Value);
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            q = q.Where(h => h.MaHoSo.Contains(k) || h.HoTenBenhNhan.Contains(k) || h.MaBenhNhanHIS.Contains(k));
        }
        return await q.OrderByDescending(h => h.NgayTao).Take(200)
            .Select(h => new HoSoListVM(h.Id, h.MaHoSo, h.MaBenhNhanHIS, h.HoTenBenhNhan, h.Khoa.Ten, h.TrangThai, h.TaiLieus.Count, h.NgayTao))
            .ToListAsync(ct);
    }

    public async Task<HoSoDetailVM?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var h = await db.HoSoBenhAns
            .Include(x => x.Khoa)
            .Include(x => x.BacSiTao)
            .Include(x => x.TaiLieus).ThenInclude(t => t.ChuKys).ThenInclude(c => c.NguoiKy)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return h is null ? null : new HoSoDetailVM(h);
    }

    public async Task<List<BenhNhan>> SearchHisAsync(string keyword, CancellationToken ct = default)
    {
        var k = (keyword ?? "").Trim();
        var q = his.BenhNhans.AsQueryable();
        if (!string.IsNullOrEmpty(k))
            q = q.Where(b => b.Ma.Contains(k) || b.HoTen.Contains(k) || (b.CCCD != null && b.CCCD.Contains(k)));
        return await q.OrderBy(b => b.HoTen).Take(30).ToListAsync(ct);
    }

    public async Task<(Guid HoSoId, string MaHoSo)> TaoHoSoAsync(string maBenhNhanHis, string? maLanKham, Guid bacSiId, CancellationToken ct = default)
    {
        var bn = await his.BenhNhans.FirstOrDefaultAsync(x => x.Ma == maBenhNhanHis, ct)
            ?? throw new InvalidOperationException($"Không tìm thấy BN {maBenhNhanHis} trong HIS");
        var bs = await db.NguoiDungs.FirstAsync(u => u.Id == bacSiId, ct);
        if (bs.KhoaId is null) throw new InvalidOperationException("User không thuộc khoa nào");

        var maHoSo = await codeGen.NextAsync(ct);
        var h = new HoSoBenhAn
        {
            MaHoSo = maHoSo,
            MaBenhNhanHIS = bn.Ma,
            MaLanKhamHIS = maLanKham,
            HoTenBenhNhan = bn.HoTen,
            NgaySinh = bn.NgaySinh,
            GioiTinh = bn.GioiTinh,
            KhoaId = bs.KhoaId.Value,
            BacSiTaoId = bs.Id,
            TrangThai = TrangThaiHoSo.Draft
        };
        db.HoSoBenhAns.Add(h);

        db.AuditLogs.Add(new AuditLog
        {
            HanhDong = "TAO_HOSO",
            ActorId = bs.Id,
            ActorTen = bs.HoTen,
            LoaiDoiTuong = nameof(HoSoBenhAn),
            DoiTuongId = h.Id,
            Chitiet = $$"""{"maHoSo":"{{maHoSo}}","maBN":"{{bn.Ma}}"}"""
        });

        await db.SaveChangesAsync(ct);
        return (h.Id, h.MaHoSo);
    }

    public async Task<Guid> ImportBieuMauAsync(Guid hoSoId, string loaiTaiLieu, string fileName, Stream content, Guid uploaderId, CancellationToken ct = default)
    {
        var h = await db.HoSoBenhAns.FirstOrDefaultAsync(x => x.Id == hoSoId, ct)
            ?? throw new InvalidOperationException("Hồ sơ không tồn tại");

        await using var ms = new MemoryStream();
        await content.CopyToAsync(ms, ct);
        var bytes = ms.ToArray();
        var sha = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

        var subPath = $"{DateTime.UtcNow:yyyy/MM}/{h.Id}";
        await using var src = new MemoryStream(bytes);
        var rel = await storage.SaveAsync(src, subPath, fileName, ct);

        var t = new TaiLieu
        {
            HoSoBenhAnId = h.Id,
            LoaiTaiLieu = loaiTaiLieu,
            TenFile = fileName,
            DuongDanLuuTru = rel,
            KichThuoc = bytes.LongLength,
            MimeType = "application/pdf",
            Sha256 = sha,
            NguoiUploadId = uploaderId,
            TrangThaiKy = TrangThaiKyTaiLieu.ChuaKy
        };
        db.TaiLieus.Add(t);

        if (h.TrangThai == TrangThaiHoSo.Draft) h.TrangThai = TrangThaiHoSo.ChoKy;
        h.NgayCapNhat = DateTime.UtcNow;

        db.AuditLogs.Add(new AuditLog
        {
            HanhDong = "UPLOAD_TAILIEU",
            ActorId = uploaderId,
            LoaiDoiTuong = nameof(TaiLieu),
            DoiTuongId = t.Id,
            Chitiet = $$"""{"loaiTL":"{{loaiTaiLieu}}","fileSize":{{bytes.LongLength}},"sha256":"{{sha}}"}"""
        });

        await db.SaveChangesAsync(ct);
        return t.Id;
    }

    public async Task<(bool Success, string? Error)> KyTaiLieuAsync(Guid hoSoId, Guid taiLieuId, string vaiTroKy, Guid nguoiKyId, IEnumerable<string> userRoles, CancellationToken ct = default)
    {
        var t = await db.TaiLieus.Include(x => x.HoSoBenhAn).ThenInclude(h => h.TaiLieus).ThenInclude(tt => tt.ChuKys)
            .Include(x => x.ChuKys)
            .FirstOrDefaultAsync(x => x.Id == taiLieuId && x.HoSoBenhAnId == hoSoId, ct);
        if (t is null) return (false, "Tài liệu không tồn tại");

        var nk = await db.NguoiDungs.FirstAsync(u => u.Id == nguoiKyId, ct);
        if (string.IsNullOrEmpty(nk.CCCD)) return (false, "User chưa có CCCD");

        // Workflow rule check
        var avail = WorkflowRules.KiemTraQuyenKy(t, userRoles, nguoiKyId, t.HoSoBenhAn.KhoaId, nk.KhoaId);
        if (avail.LyDoChan is not null) return (false, avail.LyDoChan);
        if (avail.VaiTroKyKeTiep != vaiTroKy) return (false, $"Vai trò kế tiếp phải là {avail.VaiTroKyKeTiep}, không phải {vaiTroKy}");

        byte[] bytes;
        await using (var s = await storage.OpenReadAsync(t.DuongDanLuuTru, ct))
        await using (var ms = new MemoryStream())
        {
            await s.CopyToAsync(ms, ct);
            bytes = ms.ToArray();
        }
        var hash = SHA256.HashData(bytes);
        var hashHex = Convert.ToHexString(hash).ToLowerInvariant();

        var ck = new ChuKy
        {
            TaiLieuId = t.Id,
            NguoiKyId = nk.Id,
            VaiTroKy = vaiTroKy,
            LoaiCa = LoaiCa.SelfSigned,
            Sha256TruocKy = hashHex,
            TrangThai = TrangThaiChuKy.ChoXacNhan
        };
        db.ChuKys.Add(ck);

        try
        {
            var r = await signer.SignHashAsync(hash, nk.CCCD!, nk.HoTen, ct);
            ck.SignatureValue = r.SignatureValueBase64;
            ck.CertSubject = r.CertSubject;
            ck.SmartCa_SerialNumber = r.CertSerialNumber;
            ck.CertNotBefore = r.NotBefore;
            ck.CertNotAfter = r.NotAfter;
            ck.TrangThai = TrangThaiChuKy.DaKy;
            ck.NgayHoanTat = DateTime.UtcNow;
            t.TrangThaiKy = TrangThaiKyTaiLieu.DaKy;

            t.HoSoBenhAn.TrangThai = WorkflowRules.TinhTrangThaiHoSo(t.HoSoBenhAn);
            t.HoSoBenhAn.NgayCapNhat = DateTime.UtcNow;

            db.AuditLogs.Add(new AuditLog
            {
                HanhDong = "KY_SO_SUCCESS",
                ActorId = nk.Id, ActorTen = nk.HoTen,
                LoaiDoiTuong = nameof(ChuKy), DoiTuongId = ck.Id,
                Chitiet = $$"""{"vaiTro":"{{vaiTroKy}}","cert":"{{r.CertSubject}}"}"""
            });
            await db.SaveChangesAsync(ct);
            return (true, null);
        }
        catch (Exception ex)
        {
            ck.TrangThai = TrangThaiChuKy.ThatBai;
            ck.LyDoLoi = ex.Message;
            t.TrangThaiKy = TrangThaiKyTaiLieu.Loi;
            await db.SaveChangesAsync(ct);
            return (false, ex.Message);
        }
    }

    public async Task<Stream?> OpenPdfAsync(Guid hoSoId, Guid taiLieuId, CancellationToken ct = default)
    {
        var t = await db.TaiLieus.FirstOrDefaultAsync(x => x.Id == taiLieuId && x.HoSoBenhAnId == hoSoId, ct);
        if (t is null || !storage.Exists(t.DuongDanLuuTru)) return null;
        return await storage.OpenReadAsync(t.DuongDanLuuTru, ct);
    }

    public async Task<TaiLieu?> GetTaiLieuAsync(Guid hoSoId, Guid taiLieuId, CancellationToken ct = default) =>
        await db.TaiLieus.FirstOrDefaultAsync(x => x.Id == taiLieuId && x.HoSoBenhAnId == hoSoId, ct);

    /// <summary>
    /// Hủy 1 chữ ký (chỉ Admin). Chữ ký không bị xóa khỏi DB - đánh dấu Huy + LyDoLoi để giữ trail.
    /// Tài liệu sẽ chuyển về ChuaKy nếu không còn chữ ký DaKy nào.
    /// </summary>
    public async Task<(bool Ok, string? Err)> HuyChuKyAsync(Guid chuKyId, Guid actorId, string lyDo, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(lyDo)) return (false, "Phải nhập lý do hủy");
        var ck = await db.ChuKys.Include(x => x.TaiLieu).ThenInclude(t => t.HoSoBenhAn).ThenInclude(h => h.TaiLieus).ThenInclude(t => t.ChuKys)
            .FirstOrDefaultAsync(x => x.Id == chuKyId, ct);
        if (ck is null) return (false, "Không tìm thấy chữ ký");
        if (ck.TrangThai != TrangThaiChuKy.DaKy) return (false, "Chỉ hủy chữ ký đã ký thành công");

        ck.TrangThai = TrangThaiChuKy.Huy;
        ck.LyDoLoi = lyDo;

        // Tính lại trạng thái tài liệu
        var conChuKy = ck.TaiLieu.ChuKys.Any(c => c.Id != ck.Id && c.TrangThai == TrangThaiChuKy.DaKy);
        ck.TaiLieu.TrangThaiKy = conChuKy ? TrangThaiKyTaiLieu.DaKy : TrangThaiKyTaiLieu.ChuaKy;

        ck.TaiLieu.HoSoBenhAn.TrangThai = WorkflowRules.TinhTrangThaiHoSo(ck.TaiLieu.HoSoBenhAn);
        ck.TaiLieu.HoSoBenhAn.NgayCapNhat = DateTime.UtcNow;

        db.AuditLogs.Add(new AuditLog
        {
            HanhDong = "HUY_CHU_KY",
            ActorId = actorId,
            LoaiDoiTuong = nameof(ChuKy),
            DoiTuongId = ck.Id,
            Chitiet = $$"""{"taiLieuId":"{{ck.TaiLieuId}}","vaiTro":"{{ck.VaiTroKy}}","lyDo":"{{lyDo.Replace("\"","'")}}"}"""
        });

        await db.SaveChangesAsync(ct);
        return (true, null);
    }

    /// <summary>
    /// Xóa 1 tài liệu (chỉ khi chưa có chữ ký).
    /// </summary>
    public async Task<(bool Ok, string? Err)> XoaTaiLieuAsync(Guid hoSoId, Guid taiLieuId, Guid actorId, CancellationToken ct = default)
    {
        var t = await db.TaiLieus.Include(x => x.ChuKys)
            .FirstOrDefaultAsync(x => x.Id == taiLieuId && x.HoSoBenhAnId == hoSoId, ct);
        if (t is null) return (false, "Không tìm thấy tài liệu");
        if (t.ChuKys.Any(c => c.TrangThai == TrangThaiChuKy.DaKy))
            return (false, "Tài liệu đã có chữ ký — không thể xóa. Hủy chữ ký trước (admin)");

        // Xóa file
        try
        {
            await storage.DeleteAsync(t.DuongDanLuuTru);
        }
        catch { /* ignore */ }

        db.ChuKys.RemoveRange(t.ChuKys);
        db.TaiLieus.Remove(t);
        db.AuditLogs.Add(new AuditLog
        {
            HanhDong = "XOA_TAILIEU",
            ActorId = actorId,
            LoaiDoiTuong = nameof(TaiLieu),
            DoiTuongId = t.Id,
            Chitiet = $$"""{"hoSoId":"{{hoSoId}}","loaiTL":"{{t.LoaiTaiLieu}}","tenFile":"{{t.TenFile}}"}"""
        });
        await db.SaveChangesAsync(ct);
        return (true, null);
    }
}

public record HoSoListVM(Guid Id, string MaHoSo, string MaBenhNhanHIS, string HoTenBenhNhan, string KhoaTen, TrangThaiHoSo TrangThai, int SoTaiLieu, DateTime NgayTao);

public class HoSoDetailVM
{
    public HoSoBenhAn HoSo { get; }
    public IReadOnlyList<TaiLieu> TaiLieus => HoSo.TaiLieus.ToList();
    public HoSoDetailVM(HoSoBenhAn h) { HoSo = h; }
}
