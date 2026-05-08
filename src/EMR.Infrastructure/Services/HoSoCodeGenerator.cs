using EMR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EMR.Infrastructure.Services;

/// <summary>
/// Sinh mã hồ sơ format YY.NNNNNN (vd 26.004951).
/// Lock-free dựa trên COUNT trong năm — cho dev OK, production cần SEQUENCE/lock để tránh race.
/// </summary>
public class HoSoCodeGenerator(EmrDbContext db)
{
    private static readonly SemaphoreSlim _gate = new(1, 1);

    public async Task<string> NextAsync(CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            var yy = (DateTime.Now.Year % 100).ToString("D2");
            var prefix = yy + ".";
            var maxThisYear = await db.HoSoBenhAns
                .Where(x => x.MaHoSo.StartsWith(prefix))
                .Select(x => x.MaHoSo)
                .OrderByDescending(s => s)
                .FirstOrDefaultAsync(ct);

            int next = 1;
            if (maxThisYear is not null && maxThisYear.Length >= 9 && int.TryParse(maxThisYear[3..], out var cur))
                next = cur + 1;

            return $"{prefix}{next:D6}";
        }
        finally { _gate.Release(); }
    }
}
