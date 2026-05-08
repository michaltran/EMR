using EMR.Domain.Entities;
using EMR.Domain.Entities.His;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EMR.Infrastructure.Persistence.Seeds;

public static class DemoDataSeeder
{
    public static async Task RunAsync(IServiceProvider sp, CancellationToken ct = default)
    {
        using var scope = sp.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Seed");
        var emr = scope.ServiceProvider.GetRequiredService<EmrDbContext>();
        var his = scope.ServiceProvider.GetRequiredService<HisDemoDbContext>();

        await SeedUsers(emr, logger, ct);
        await SeedHis(his, logger, ct);
    }

    private static async Task SeedUsers(EmrDbContext db, ILogger logger, CancellationToken ct)
    {
        if (await db.NguoiDungs.AnyAsync(ct))
        {
            logger.LogInformation("NguoiDung đã có data, skip seed user");
            return;
        }

        var roles = await db.VaiTros.ToDictionaryAsync(x => x.Ma, ct);
        var khoaNoi = await db.Khoas.FirstAsync(x => x.Ma == "K_NOI", ct);
        var bgd = await db.Khoas.FirstAsync(x => x.Ma == "BGD", ct);
        var khth = await db.Khoas.FirstAsync(x => x.Ma == "P_KHNV", ct);

        string Hash(string p) => BCrypt.Net.BCrypt.HashPassword(p, workFactor: 11);

        var admin = new NguoiDung
        {
            TenDangNhap = "admin",
            MatKhauHash = Hash("admin@123"),
            HoTen = "Quản trị hệ thống",
            CCCD = "048123456789",
            Email = "admin@ttytlienchieu.local",
            KhoaId = null
        };
        var bs = new NguoiDung
        {
            TenDangNhap = "bs.an",
            MatKhauHash = Hash("bs@123"),
            HoTen = "Nguyễn Văn An",
            CCCD = "048123450001",
            Email = "an.nguyen@ttytlienchieu.local",
            KhoaId = khoaNoi.Id
        };
        var tk = new NguoiDung
        {
            TenDangNhap = "tk.binh",
            MatKhauHash = Hash("tk@123"),
            HoTen = "Trần Thị Bình",
            CCCD = "048123450002",
            Email = "binh.tran@ttytlienchieu.local",
            KhoaId = khoaNoi.Id
        };
        var ld = new NguoiDung
        {
            TenDangNhap = "ld.cuong",
            MatKhauHash = Hash("ld@123"),
            HoTen = "Lê Văn Cường",
            CCCD = "048123450003",
            Email = "cuong.le@ttytlienchieu.local",
            KhoaId = bgd.Id
        };
        var khthNd = new NguoiDung
        {
            TenDangNhap = "khth.dung",
            MatKhauHash = Hash("khth@123"),
            HoTen = "Phạm Thị Dung",
            CCCD = "048123450004",
            Email = "dung.pham@ttytlienchieu.local",
            KhoaId = khth.Id
        };

        db.NguoiDungs.AddRange(admin, bs, tk, ld, khthNd);
        db.NguoiDungVaiTros.AddRange(
            new() { NguoiDungId = admin.Id,   VaiTroId = roles["ADMIN"].Id },
            new() { NguoiDungId = bs.Id,      VaiTroId = roles["BACSI"].Id },
            new() { NguoiDungId = tk.Id,      VaiTroId = roles["BACSI"].Id },
            new() { NguoiDungId = tk.Id,      VaiTroId = roles["TRUONGKHOA"].Id },
            new() { NguoiDungId = ld.Id,      VaiTroId = roles["LANHDAO_BV"].Id },
            new() { NguoiDungId = khthNd.Id,  VaiTroId = roles["KHTH"].Id }
        );
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed 5 user demo: admin, bs.an, tk.binh, ld.cuong, khth.dung");
    }

    private static async Task SeedHis(HisDemoDbContext db, ILogger logger, CancellationToken ct)
    {
        if (await db.BenhNhans.AnyAsync(ct))
        {
            logger.LogInformation("HIS demo đã có data, skip");
            return;
        }

        var bn = new[]
        {
            new BenhNhan { Ma = "BN0001", HoTen = "Nguyễn Thị Hoa",     NgaySinh = new(1985, 3, 12), GioiTinh = 0, DiaChi = "Hòa Khánh Bắc, Liên Chiểu, Đà Nẵng", SoDienThoai = "0905111111", CCCD = "048185000001" },
            new BenhNhan { Ma = "BN0002", HoTen = "Trần Văn Hùng",      NgaySinh = new(1972, 7, 1),  GioiTinh = 1, DiaChi = "Hòa Hiệp Nam, Liên Chiểu, Đà Nẵng",  SoDienThoai = "0905222222", CCCD = "048172000002" },
            new BenhNhan { Ma = "BN0003", HoTen = "Phạm Thị Lan",       NgaySinh = new(1990, 11, 23), GioiTinh = 0, DiaChi = "Hòa Minh, Liên Chiểu, Đà Nẵng",      SoDienThoai = "0905333333", CCCD = "048190000003" },
            new BenhNhan { Ma = "BN0004", HoTen = "Lê Quang Đạt",       NgaySinh = new(2015, 5, 8),  GioiTinh = 1, DiaChi = "Hòa Khánh Nam, Liên Chiểu, Đà Nẵng", SoDienThoai = "0905444444" },
            new BenhNhan { Ma = "BN0005", HoTen = "Hoàng Thị Mai",      NgaySinh = new(1955, 1, 30), GioiTinh = 0, DiaChi = "Hòa Hiệp Bắc, Liên Chiểu, Đà Nẵng",  SoDienThoai = "0905555555", CCCD = "048155000005" },
            new BenhNhan { Ma = "BN0006", HoTen = "Vũ Đình Sơn",        NgaySinh = new(1988, 9, 17), GioiTinh = 1, DiaChi = "Hải Vân, Liên Chiểu, Đà Nẵng",       SoDienThoai = "0905666666", CCCD = "048188000006" },
            new BenhNhan { Ma = "BN0007", HoTen = "Đỗ Thị Hằng",        NgaySinh = new(1995, 4, 4),  GioiTinh = 0, DiaChi = "Hòa Khánh Bắc, Liên Chiểu, Đà Nẵng", SoDienThoai = "0905777777", CCCD = "048195000007" },
            new BenhNhan { Ma = "BN0008", HoTen = "Bùi Văn Tâm",        NgaySinh = new(1965, 12, 15), GioiTinh = 1, DiaChi = "Hòa Minh, Liên Chiểu, Đà Nẵng",     SoDienThoai = "0905888888", CCCD = "048165000008" },
            new BenhNhan { Ma = "BN0009", HoTen = "Cao Thị Yến",        NgaySinh = new(2000, 8, 21), GioiTinh = 0, DiaChi = "Hòa Hiệp Nam, Liên Chiểu, Đà Nẵng", SoDienThoai = "0905999999", CCCD = "048100000009" },
            new BenhNhan { Ma = "BN0010", HoTen = "Phan Đức Thịnh",     NgaySinh = new(1978, 6, 10), GioiTinh = 1, DiaChi = "Hòa Hiệp Bắc, Liên Chiểu, Đà Nẵng", SoDienThoai = "0905101010", CCCD = "048178000010" },
        };
        db.BenhNhans.AddRange(bn);

        var rng = new Random(42);
        var lk = new List<LanKham>();
        var khoa = new[] { "Khoa Nội", "Khoa Ngoại", "Khoa Sản", "Khoa Nhi", "Khoa Hồi sức Cấp cứu", "Khoa YHCT - VLTL&PHCN" };
        var cd = new[] { "Tăng huyết áp", "Viêm dạ dày", "Cảm cúm", "Theo dõi sau mổ", "Đái tháo đường týp 2", "Viêm phổi", "Theo dõi thai kỳ", "Sốt virus", "Đau lưng cơ năng" };
        int seq = 1;
        foreach (var p in bn)
        {
            int n = rng.Next(1, 4);
            for (int i = 0; i < n; i++)
            {
                var vao = DateTime.UtcNow.AddDays(-rng.Next(1, 365));
                lk.Add(new LanKham
                {
                    Ma = $"LK{seq++:D5}",
                    MaBenhNhan = p.Ma,
                    NgayVaoVien = vao,
                    NgayRaVien = vao.AddDays(rng.Next(0, 7)),
                    KhoaDieuTri = khoa[rng.Next(khoa.Length)],
                    ChanDoanRaVien = cd[rng.Next(cd.Length)]
                });
            }
        }
        db.LanKhams.AddRange(lk);

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed HIS demo: {Bn} bệnh nhân, {Lk} lần khám", bn.Length, lk.Count);
    }
}
