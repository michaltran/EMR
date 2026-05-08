using EMR.Domain.Entities;
using EMR.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EMR.Infrastructure.Persistence.Seeds;

public static class EmrSeedData
{
    public static void Apply(ModelBuilder b)
    {
        SeedVaiTro(b);
        SeedKhoa(b);
    }

    private static void SeedVaiTro(ModelBuilder b)
    {
        var roles = new[]
        {
            ("BACSI",       "Bác sĩ",                   "Tạo hồ sơ, ký bệnh án mình tạo"),
            ("DIEUDUONG",   "Điều dưỡng",               "Hỗ trợ tạo/cập nhật hồ sơ, ký phiếu chăm sóc"),
            ("DUOCSI",      "Dược sĩ",                  "Quản lý đơn thuốc, ký xác nhận cấp phát"),
            ("TRUONGKHOA",  "Trưởng khoa",              "Ký duyệt cấp khoa, xem hồ sơ trong khoa"),
            ("KHTH",        "Kế hoạch nghiệp vụ",       "Kiểm tra hồ sơ, chuyển kho BV, báo cáo"),
            ("LANHDAO_BV",  "Lãnh đạo Bệnh viện",       "Ký duyệt cấp BV, xem toàn bộ"),
            ("ADMIN",       "Quản trị hệ thống",        "Quản lý người dùng, vai trò, audit"),
            ("BENHNHAN",    "Bệnh nhân",                "Đăng nhập app, xem hồ sơ của mình"),
        };

        var seeds = roles.Select((r, i) => new VaiTro
        {
            Id = DeterministicGuid("vaitro_" + r.Item1),
            Ma = r.Item1,
            Ten = r.Item2,
            MoTa = r.Item3
        }).ToArray();

        b.Entity<VaiTro>().HasData(seeds);
    }

    private static void SeedKhoa(ModelBuilder b)
    {
        var k_lck = DeterministicGuid("khoa_K_LCK");
        var k_yhct = DeterministicGuid("khoa_K_YHCT");

        var rows = new (string Ma, string Ten, NhomKhoa Nhom, Guid? ChaId, int Thu)[]
        {
            ("BGD",            "Ban Giám đốc",                              NhomKhoa.BGD,   null,    1),

            ("P_TCHC",         "Phòng Tổ chức Hành chính",                  NhomKhoa.Phong, null,   10),
            ("P_KHNV",         "Phòng Kế hoạch Nghiệp vụ",                  NhomKhoa.Phong, null,   11),
            ("P_TCKT",         "Phòng Tài chính kế toán",                   NhomKhoa.Phong, null,   12),
            ("P_DD",           "Phòng Điều dưỡng",                          NhomKhoa.Phong, null,   13),
            ("P_TCCB",         "Phòng Tổ chức cán bộ",                      NhomKhoa.Phong, null,   14),
            ("P_DS",           "Phòng Dân số",                              NhomKhoa.Phong, null,   15),
            ("P_TC",           "Phòng Tiêm chủng",                          NhomKhoa.Phong, null,   16),
            ("P_KSBT",         "Phòng Khám KSBT",                           NhomKhoa.Phong, null,   17),

            ("K_NGOAI",        "Khoa Ngoại",                                NhomKhoa.Khoa,  null,   20),
            ("K_NOI",          "Khoa Nội",                                  NhomKhoa.Khoa,  null,   21),
            ("K_COVID",        "Khoa Điều trị COVID-19",                    NhomKhoa.Khoa,  null,   22),
            ("K_SAN",          "Khoa Sản",                                  NhomKhoa.Khoa,  null,   23),
            ("K_NHI",          "Khoa Nhi",                                  NhomKhoa.Khoa,  null,   24),
            ("K_LCK",          "Khoa Liên Chuyên Khoa",                     NhomKhoa.Khoa,  null,   25),
            ("K_YHCT",         "Khoa YHCT - VLTL&PHCN",                     NhomKhoa.Khoa,  null,   26),
            ("K_HSCC",         "Khoa Hồi sức Cấp cứu",                      NhomKhoa.Khoa,  null,   27),
            ("K_CDHA",         "Khoa Chẩn đoán Hình ảnh",                   NhomKhoa.Khoa,  null,   28),
            ("K_XN",           "Khoa Xét nghiệm",                           NhomKhoa.Khoa,  null,   29),
            ("K_DUOC",         "Khoa Dược-TTB-VTYT",                        NhomKhoa.Khoa,  null,   30),
            ("K_KSBT",         "Khoa Kiểm soát bệnh tật và HIV/AIDS",       NhomKhoa.Khoa,  null,   31),
            ("K_YTCC",         "Khoa YTCC - DD & ATTP",                     NhomKhoa.Khoa,  null,   32),
            ("K_KSNK",         "Khoa Kiểm soát nhiễm khuẩn",                NhomKhoa.Khoa,  null,   33),
            ("K_PK",           "Khoa Phòng khám",                           NhomKhoa.Khoa,  null,   34),
            ("K_LCK_NTRU_RHM", "Liên Chuyên Khoa (Ngoại trú RHM)",          NhomKhoa.Khoa,  k_lck,  35),
            ("K_YHCT_NTRU",    "Khoa YHCT - VLTL&PHCN (Ngoại trú)",         NhomKhoa.Khoa,  k_yhct, 36),

            ("TYT_HKB",        "TYT Hòa Khánh Bắc",                          NhomKhoa.TYT,   null,   50),
            ("TYT_HKN",        "TYT Hòa Khánh Nam",                          NhomKhoa.TYT,   null,   51),
            ("TYT_HHB",        "TYT Hòa Hiệp Bắc",                           NhomKhoa.TYT,   null,   52),
            ("TYT_HHN",        "TYT Hòa Hiệp Nam",                           NhomKhoa.TYT,   null,   53),
            ("TYT_HM",         "TYT Hòa Minh",                               NhomKhoa.TYT,   null,   54),
            ("TYT_HV",         "Trạm y tế phường Hải Vân",                   NhomKhoa.TYT,   null,   55),
            ("TYT_KHAC",       "Trạm Y tế (chung)",                          NhomKhoa.TYT,   null,   56),
        };

        var seeds = rows.Select(r => new Khoa
        {
            Id = DeterministicGuid("khoa_" + r.Ma),
            Ma = r.Ma,
            Ten = r.Ten,
            Nhom = r.Nhom,
            KhoaChaId = r.ChaId,
            ThuTu = r.Thu
        }).ToArray();

        b.Entity<Khoa>().HasData(seeds);
    }

    private static Guid DeterministicGuid(string seed)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var bytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(seed));
        return new Guid(bytes);
    }
}
