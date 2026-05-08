using EMR.Domain.Entities;
using EMR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EMR.Web.Services;

public record AuditLogVM(long Id, DateTime ThoiGian, string? ActorTen, string HanhDong, string? LoaiDoiTuong, Guid? DoiTuongId, string? IpAddress, string? Chitiet);

public class AuditService(EmrDbContext db)
{
    public async Task<List<AuditLogVM>> ListAsync(string? actor, string? hanhDong, DateTime? tu, DateTime? den, int take = 200, CancellationToken ct = default)
    {
        var q = db.AuditLogs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(actor))
        {
            var k = actor.Trim();
            q = q.Where(a => a.ActorTen != null && a.ActorTen.Contains(k));
        }
        if (!string.IsNullOrWhiteSpace(hanhDong)) q = q.Where(a => a.HanhDong == hanhDong);
        if (tu.HasValue) q = q.Where(a => a.ThoiGian >= tu.Value.ToUniversalTime());
        if (den.HasValue) q = q.Where(a => a.ThoiGian <= den.Value.ToUniversalTime());

        return await q.OrderByDescending(a => a.ThoiGian).Take(take)
            .Select(a => new AuditLogVM(a.Id, a.ThoiGian, a.ActorTen, a.HanhDong, a.LoaiDoiTuong, a.DoiTuongId, a.IpAddress, a.Chitiet))
            .ToListAsync(ct);
    }

    public async Task<List<string>> GetAllActionsAsync(CancellationToken ct = default) =>
        await db.AuditLogs.Select(a => a.HanhDong).Distinct().OrderBy(x => x).ToListAsync(ct);
}
