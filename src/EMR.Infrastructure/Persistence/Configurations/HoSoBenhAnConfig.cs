using EMR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EMR.Infrastructure.Persistence.Configurations;

public class HoSoBenhAnConfig : IEntityTypeConfiguration<HoSoBenhAn>
{
    public void Configure(EntityTypeBuilder<HoSoBenhAn> e)
    {
        e.ToTable("HoSoBenhAn");
        e.HasKey(x => x.Id);
        e.Property(x => x.MaHoSo).HasMaxLength(20).IsRequired();
        e.Property(x => x.MaBenhNhanHIS).HasMaxLength(50).IsRequired();
        e.Property(x => x.MaLanKhamHIS).HasMaxLength(50);
        e.Property(x => x.HoTenBenhNhan).HasMaxLength(200).IsRequired();
        e.Property(x => x.TrangThai).HasConversion<byte>();
        e.Property(x => x.KhoLuuTru).HasConversion<byte>();

        e.HasIndex(x => x.MaHoSo).IsUnique();
        e.HasIndex(x => x.MaBenhNhanHIS);
        e.HasIndex(x => new { x.KhoaId, x.TrangThai });
        e.HasIndex(x => x.NgayTao);

        e.HasOne(x => x.Khoa).WithMany(k => k.HoSoBenhAns).HasForeignKey(x => x.KhoaId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.BacSiTao).WithMany().HasForeignKey(x => x.BacSiTaoId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class TaiLieuConfig : IEntityTypeConfiguration<TaiLieu>
{
    public void Configure(EntityTypeBuilder<TaiLieu> e)
    {
        e.ToTable("TaiLieu");
        e.HasKey(x => x.Id);
        e.Property(x => x.LoaiTaiLieu).HasMaxLength(50).IsRequired();
        e.Property(x => x.TenFile).HasMaxLength(500).IsRequired();
        e.Property(x => x.DuongDanLuuTru).HasMaxLength(1000).IsRequired();
        e.Property(x => x.MimeType).HasMaxLength(100).IsRequired();
        e.Property(x => x.Sha256).HasMaxLength(64).IsRequired();
        e.Property(x => x.TrangThaiKy).HasConversion<byte>();

        e.HasIndex(x => x.HoSoBenhAnId);
        e.HasIndex(x => x.TrangThaiKy);

        e.HasOne(x => x.HoSoBenhAn).WithMany(h => h.TaiLieus).HasForeignKey(x => x.HoSoBenhAnId).OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.NguoiUpload).WithMany().HasForeignKey(x => x.NguoiUploadId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ChuKyConfig : IEntityTypeConfiguration<ChuKy>
{
    public void Configure(EntityTypeBuilder<ChuKy> e)
    {
        e.ToTable("ChuKy");
        e.HasKey(x => x.Id);
        e.Property(x => x.VaiTroKy).HasMaxLength(50).IsRequired();
        e.Property(x => x.LoaiCa).HasConversion<byte>();
        e.Property(x => x.SmartCa_TransactionId).HasMaxLength(100);
        e.Property(x => x.SmartCa_TranCode).HasMaxLength(100);
        e.Property(x => x.SmartCa_CertId).HasMaxLength(100);
        e.Property(x => x.SmartCa_SerialNumber).HasMaxLength(100);
        e.Property(x => x.CertSubject).HasMaxLength(500);
        e.Property(x => x.Sha256TruocKy).HasMaxLength(64).IsRequired();
        e.Property(x => x.DuongDanFileSau).HasMaxLength(1000);
        e.Property(x => x.LyDoLoi).HasMaxLength(1000);
        e.Property(x => x.TrangThai).HasConversion<byte>();

        e.HasIndex(x => x.TaiLieuId);
        e.HasIndex(x => x.NguoiKyId);
        e.HasIndex(x => x.SmartCa_TransactionId).IsUnique().HasFilter("[SmartCa_TransactionId] IS NOT NULL");
        e.HasIndex(x => x.TrangThai);

        e.HasOne(x => x.TaiLieu).WithMany(t => t.ChuKys).HasForeignKey(x => x.TaiLieuId).OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.NguoiKy).WithMany().HasForeignKey(x => x.NguoiKyId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class AuditLogConfig : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> e)
    {
        e.ToTable("AuditLog");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedOnAdd();
        e.Property(x => x.ThoiGian).HasDefaultValueSql("SYSUTCDATETIME()");
        e.Property(x => x.ActorTen).HasMaxLength(200);
        e.Property(x => x.HanhDong).HasMaxLength(50).IsRequired();
        e.Property(x => x.LoaiDoiTuong).HasMaxLength(50);
        e.Property(x => x.IpAddress).HasMaxLength(50);
        e.Property(x => x.UserAgent).HasMaxLength(500);

        e.HasIndex(x => x.ThoiGian).IsDescending();
        e.HasIndex(x => x.ActorId);
        e.HasIndex(x => new { x.DoiTuongId, x.HanhDong });
    }
}
