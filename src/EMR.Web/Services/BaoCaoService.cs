using EMR.Domain.Enums;
using EMR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EMR.Web.Services;

public record DashboardStats(
    int TongHoSo,
    int HoanTat,
    int DangXuLy,
    int Draft,
    int TaiLieuDaKy,
    int TaiLieuChuaKy,
    int ChuKy7Ngay,
    int HoSo7Ngay,
    int NguoiDungHoatDong);

public record KhoaStat(string KhoaTen, int SoHoSo, int SoHoanTat, int SoChuKy);
public record BacSiStat(string HoTen, string KhoaTen, int SoChuKy, DateTime? LanCuoi);
public record TrangThaiStat(string TrangThai, int SoLuong);
public record TimeSeriesPoint(DateTime Ngay, int HoSo, int ChuKy);

public class BaoCaoService(EmrDbContext db)
{
    public async Task<DashboardStats> DashboardAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var d7 = now.AddDays(-7);

        var hoSos = db.HoSoBenhAns;
        var taiLieus = db.TaiLieus;
        var chuKys = db.ChuKys;

        var tongHs = await hoSos.CountAsync(ct);
        var hoanTat = await hoSos.CountAsync(h => h.TrangThai == TrangThaiHoSo.HoanTat, ct);
        var draft = await hoSos.CountAsync(h => h.TrangThai == TrangThaiHoSo.Draft, ct);
        var dangXl = tongHs - hoanTat - draft;

        var tlDaKy = await taiLieus.CountAsync(t => t.TrangThaiKy == TrangThaiKyTaiLieu.DaKy, ct);
        var tlChuaKy = await taiLieus.CountAsync(t => t.TrangThaiKy == TrangThaiKyTaiLieu.ChuaKy, ct);

        var ck7 = await chuKys.CountAsync(c => c.TrangThai == TrangThaiChuKy.DaKy && c.NgayHoanTat >= d7, ct);
        var hs7 = await hoSos.CountAsync(h => h.NgayTao >= d7, ct);

        var users = await db.NguoiDungs.CountAsync(u => u.TrangThai == 1, ct);

        return new DashboardStats(tongHs, hoanTat, dangXl, draft, tlDaKy, tlChuaKy, ck7, hs7, users);
    }

    public async Task<List<KhoaStat>> KhoaStatsAsync(CancellationToken ct = default)
    {
        var data = await db.HoSoBenhAns
            .Include(h => h.Khoa)
            .Include(h => h.TaiLieus).ThenInclude(t => t.ChuKys)
            .ToListAsync(ct);

        return data.GroupBy(h => h.Khoa.Ten)
            .Select(g => new KhoaStat(
                g.Key,
                g.Count(),
                g.Count(h => h.TrangThai == TrangThaiHoSo.HoanTat),
                g.SelectMany(h => h.TaiLieus).SelectMany(t => t.ChuKys).Count(c => c.TrangThai == TrangThaiChuKy.DaKy)))
            .OrderByDescending(k => k.SoHoSo)
            .ToList();
    }

    public async Task<List<BacSiStat>> TopBacSiAsync(int top = 10, CancellationToken ct = default)
    {
        var groups = await db.ChuKys
            .Where(c => c.TrangThai == TrangThaiChuKy.DaKy)
            .GroupBy(c => c.NguoiKyId)
            .Select(g => new
            {
                NguoiKyId = g.Key,
                SoChuKy = g.Count(),
                LanCuoi = g.Max(c => (DateTime?)c.NgayHoanTat)
            })
            .OrderByDescending(g => g.SoChuKy)
            .Take(top)
            .ToListAsync(ct);

        var ids = groups.Select(g => g.NguoiKyId).ToList();
        var users = await db.NguoiDungs
            .Include(u => u.Khoa)
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, ct);

        return groups.Select(g =>
        {
            users.TryGetValue(g.NguoiKyId, out var u);
            return new BacSiStat(
                u?.HoTen ?? "—",
                u?.Khoa?.Ten ?? "—",
                g.SoChuKy,
                g.LanCuoi);
        }).ToList();
    }

    public async Task<List<TrangThaiStat>> TrangThaiHoSoStatsAsync(CancellationToken ct = default)
    {
        var raw = await db.HoSoBenhAns.GroupBy(h => h.TrangThai)
            .Select(g => new { Tt = g.Key, Sl = g.Count() })
            .ToListAsync(ct);
        return raw.Select(x => new TrangThaiStat(TenTrangThai(x.Tt), x.Sl)).ToList();
    }

    public async Task<List<TimeSeriesPoint>> TimeSeries30DaysAsync(CancellationToken ct = default)
    {
        var d30 = DateTime.UtcNow.Date.AddDays(-30);
        var hsByDate = await db.HoSoBenhAns
            .Where(h => h.NgayTao >= d30)
            .GroupBy(h => h.NgayTao.Date)
            .Select(g => new { Date = g.Key, Cnt = g.Count() })
            .ToListAsync(ct);

        var ckByDate = await db.ChuKys
            .Where(c => c.TrangThai == TrangThaiChuKy.DaKy && c.NgayHoanTat >= d30)
            .GroupBy(c => c.NgayHoanTat!.Value.Date)
            .Select(g => new { Date = g.Key, Cnt = g.Count() })
            .ToListAsync(ct);

        var dates = Enumerable.Range(0, 31).Select(i => d30.AddDays(i)).ToList();
        return dates.Select(d => new TimeSeriesPoint(
            d,
            hsByDate.FirstOrDefault(x => x.Date == d)?.Cnt ?? 0,
            ckByDate.FirstOrDefault(x => x.Date == d)?.Cnt ?? 0
        )).ToList();
    }

    private static string TenTrangThai(TrangThaiHoSo t) => t switch
    {
        TrangThaiHoSo.Draft => "Nháp",
        TrangThaiHoSo.ChoKy => "Chờ ký",
        TrangThaiHoSo.DaKyBacSi => "BS đã ký",
        TrangThaiHoSo.DaKyTruongKhoa => "TK đã ký",
        TrangThaiHoSo.DaKyLanhDao => "LĐ đã ký",
        TrangThaiHoSo.HoanTat => "Hoàn tất",
        TrangThaiHoSo.Huy => "Hủy",
        _ => t.ToString()
    };
}
