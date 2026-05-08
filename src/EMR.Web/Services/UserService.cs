using EMR.Domain.Entities;
using EMR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EMR.Web.Services;

public record UserListItem(Guid Id, string TenDangNhap, string HoTen, string? CCCD, string? KhoaTen, string[] VaiTros, byte TrangThai, DateTime NgayTao);

public class UserEditModel
{
    public Guid? Id { get; set; }
    public string TenDangNhap { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string? CCCD { get; set; }
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public Guid? KhoaId { get; set; }
    public string[] VaiTros { get; set; } = [];
    public byte TrangThai { get; set; } = 1;
}

public class UserService(EmrDbContext db)
{
    public async Task<List<UserListItem>> ListAsync(string? keyword, CancellationToken ct = default)
    {
        var q = db.NguoiDungs
            .Include(u => u.Khoa)
            .Include(u => u.VaiTros).ThenInclude(v => v.VaiTro)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            q = q.Where(u => u.TenDangNhap.Contains(k) || u.HoTen.Contains(k) || (u.CCCD != null && u.CCCD.Contains(k)));
        }

        return await q.OrderBy(u => u.HoTen)
            .Select(u => new UserListItem(
                u.Id, u.TenDangNhap, u.HoTen, u.CCCD, u.Khoa != null ? u.Khoa.Ten : null,
                u.VaiTros.Select(v => v.VaiTro.Ma).ToArray(),
                u.TrangThai, u.NgayTao))
            .ToListAsync(ct);
    }

    public async Task<UserEditModel?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var u = await db.NguoiDungs
            .Include(x => x.VaiTros)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (u is null) return null;
        return new UserEditModel
        {
            Id = u.Id, TenDangNhap = u.TenDangNhap, HoTen = u.HoTen,
            CCCD = u.CCCD, Email = u.Email, SoDienThoai = u.SoDienThoai,
            KhoaId = u.KhoaId,
            VaiTros = u.VaiTros.Select(v => v.VaiTroId.ToString()).ToArray(),
            TrangThai = u.TrangThai
        };
    }

    public async Task<List<Khoa>> GetKhoasAsync(CancellationToken ct = default) =>
        await db.Khoas.OrderBy(k => k.ThuTu).ToListAsync(ct);

    public async Task<List<VaiTro>> GetVaiTrosAsync(CancellationToken ct = default) =>
        await db.VaiTros.OrderBy(v => v.Ma).ToListAsync(ct);

    public async Task<(bool Ok, string? Err, Guid? Id)> CreateAsync(UserEditModel m, string matKhau, Guid actorId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(m.TenDangNhap)) return (false, "Tên đăng nhập trống", null);
        if (string.IsNullOrWhiteSpace(matKhau) || matKhau.Length < 6) return (false, "Mật khẩu tối thiểu 6 ký tự", null);
        if (await db.NguoiDungs.AnyAsync(u => u.TenDangNhap == m.TenDangNhap, ct))
            return (false, "Tên đăng nhập đã tồn tại", null);
        if (!string.IsNullOrEmpty(m.CCCD) && await db.NguoiDungs.AnyAsync(u => u.CCCD == m.CCCD, ct))
            return (false, "CCCD đã được dùng cho user khác", null);
        if (m.VaiTros.Length == 0) return (false, "Phải gán ít nhất 1 vai trò", null);

        var u = new NguoiDung
        {
            TenDangNhap = m.TenDangNhap.Trim().ToLowerInvariant(),
            MatKhauHash = BCrypt.Net.BCrypt.HashPassword(matKhau, workFactor: 11),
            HoTen = m.HoTen.Trim(),
            CCCD = string.IsNullOrWhiteSpace(m.CCCD) ? null : m.CCCD.Trim(),
            Email = string.IsNullOrWhiteSpace(m.Email) ? null : m.Email.Trim(),
            SoDienThoai = string.IsNullOrWhiteSpace(m.SoDienThoai) ? null : m.SoDienThoai.Trim(),
            KhoaId = m.KhoaId,
            TrangThai = m.TrangThai
        };
        db.NguoiDungs.Add(u);
        foreach (var vId in m.VaiTros)
            if (Guid.TryParse(vId, out var vg))
                db.NguoiDungVaiTros.Add(new NguoiDungVaiTro { NguoiDungId = u.Id, VaiTroId = vg });

        db.AuditLogs.Add(new AuditLog
        {
            HanhDong = "TAO_USER", ActorId = actorId,
            LoaiDoiTuong = nameof(NguoiDung), DoiTuongId = u.Id,
            Chitiet = $$"""{"username":"{{u.TenDangNhap}}","hoTen":"{{u.HoTen}}"}"""
        });
        await db.SaveChangesAsync(ct);
        return (true, null, u.Id);
    }

    public async Task<(bool Ok, string? Err)> UpdateAsync(UserEditModel m, Guid actorId, CancellationToken ct = default)
    {
        if (m.Id is null) return (false, "Thiếu Id");
        var u = await db.NguoiDungs.Include(x => x.VaiTros).FirstOrDefaultAsync(x => x.Id == m.Id, ct);
        if (u is null) return (false, "Không tìm thấy user");

        if (!string.IsNullOrEmpty(m.CCCD) && await db.NguoiDungs.AnyAsync(x => x.CCCD == m.CCCD && x.Id != u.Id, ct))
            return (false, "CCCD đã được dùng cho user khác");
        if (m.VaiTros.Length == 0) return (false, "Phải gán ít nhất 1 vai trò");

        u.HoTen = m.HoTen.Trim();
        u.CCCD = string.IsNullOrWhiteSpace(m.CCCD) ? null : m.CCCD.Trim();
        u.Email = string.IsNullOrWhiteSpace(m.Email) ? null : m.Email.Trim();
        u.SoDienThoai = string.IsNullOrWhiteSpace(m.SoDienThoai) ? null : m.SoDienThoai.Trim();
        u.KhoaId = m.KhoaId;
        u.TrangThai = m.TrangThai;
        u.NgayCapNhat = DateTime.UtcNow;

        db.NguoiDungVaiTros.RemoveRange(u.VaiTros);
        foreach (var vId in m.VaiTros)
            if (Guid.TryParse(vId, out var vg))
                db.NguoiDungVaiTros.Add(new NguoiDungVaiTro { NguoiDungId = u.Id, VaiTroId = vg });

        db.AuditLogs.Add(new AuditLog
        {
            HanhDong = "SUA_USER", ActorId = actorId,
            LoaiDoiTuong = nameof(NguoiDung), DoiTuongId = u.Id
        });
        await db.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Ok, string? Err)> ResetPasswordAsync(Guid userId, string newPassword, Guid actorId, CancellationToken ct = default)
    {
        if (newPassword.Length < 6) return (false, "Mật khẩu tối thiểu 6 ký tự");
        var u = await db.NguoiDungs.FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (u is null) return (false, "Không tìm thấy user");
        u.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 11);
        u.NgayCapNhat = DateTime.UtcNow;
        db.AuditLogs.Add(new AuditLog
        {
            HanhDong = "RESET_PASSWORD", ActorId = actorId,
            LoaiDoiTuong = nameof(NguoiDung), DoiTuongId = u.Id
        });
        await db.SaveChangesAsync(ct);
        return (true, null);
    }
}
