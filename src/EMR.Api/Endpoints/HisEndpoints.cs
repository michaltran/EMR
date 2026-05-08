using EMR.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EMR.Api.Endpoints;

public static class HisEndpoints
{
    public record BenhNhanItem(string Ma, string HoTen, DateTime? NgaySinh, byte? GioiTinh, string? CCCD, string? DiaChi);
    public record LanKhamItem(string Ma, DateTime NgayVaoVien, DateTime? NgayRaVien, string? KhoaDieuTri, string? ChanDoanRaVien);

    public static IEndpointRouteBuilder MapHis(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/his").WithTags("HIS").RequireAuthorization();

        g.MapGet("/benhnhan", async (string? keyword, HisDemoDbContext his, CancellationToken ct) =>
        {
            var q = his.BenhNhans.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                q = q.Where(b => b.Ma.Contains(kw) || b.HoTen.Contains(kw) || (b.CCCD != null && b.CCCD.Contains(kw)));
            }
            var items = await q.OrderBy(b => b.HoTen)
                .Take(50)
                .Select(b => new BenhNhanItem(b.Ma, b.HoTen, b.NgaySinh, b.GioiTinh, b.CCCD, b.DiaChi))
                .ToListAsync(ct);
            return Results.Ok(items);
        });

        g.MapGet("/benhnhan/{ma}/lankham", async (string ma, HisDemoDbContext his, CancellationToken ct) =>
        {
            var items = await his.LanKhams.Where(x => x.MaBenhNhan == ma)
                .OrderByDescending(x => x.NgayVaoVien)
                .Select(x => new LanKhamItem(x.Ma, x.NgayVaoVien, x.NgayRaVien, x.KhoaDieuTri, x.ChanDoanRaVien))
                .ToListAsync(ct);
            return Results.Ok(items);
        });

        return app;
    }
}
